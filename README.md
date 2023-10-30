# Corvus.Pipelines

## What is it?

A low-allocation, extensible pipeline composition library, supporting sync and async pipeline steps arranged into terminating and non-terminating pipelines.

Out-of-the-box, it includes support for exception and error handling, logging, and cancellation.

Amongst other features, it supports (but does not require) ["railway oriented programming"](https://fsharpforfunandprofit.com/rop/) - avoiding exception throwing, and offering a functional approach to termination, error handling, and retries.

## What can I do with it?

The library helps you to build programs dynamically (i.e. at runtime) out of predefined (functional) components, to operate on some particular state, and produce some result.

Those functional components are themselves stateless, and so can be reused without dynamic (re-)compilation/allocations etc.

It is useful when you want to be able to be able declaratively define the way in which a system responds to input, while dynamically adapating the processing to both the input state, and/or other environmental conditions.

It also supports moving (relatively) seamlessly from synchronous to asynchronous contexts, with minimal overhead, making it ideal for request processing and data scenarios where we frequently mix async processing (e.g. calling external services to augment our state) with synchronous processing (operating on our in-memory domain model).

## What kind of applications would find that useful?

It can be used in many applications, e.g. input handling, request processing, or data transformation.

Here are a few examples.

### Corvus.YarpPipelines

We originally built this to help us reduce the complexity of building high-performance HTTP Request Processing in YARP proxies, and have also open-sourced our **Corvus.YarpPipelines** library which we have built on top of it.

### Corvus.Pipelines.Handlers

We use **Corvus.Pipelines** to implement a version of the [Chain of Responsibility](https://en.wikipedia.org/wiki/Chain-of-responsibility_pattern) pattern that doesn't couple each step directly to the next. We call this the Handler Pattern, and it is implemented by the types in the `Corvus.Pipelines.Handlers` namespace.

This is so useful, we include it in the **Corvus.Pipelines** library itself.

### ASP.NET Core
**Corvus.Piplines** is particularly useful when coupled with, for example, [ASP.NET Core Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/overview), or even directly through `HttpContext`, to build equally lightweight pipelines for request processing from common building blocks.

We are using Corvus.Pipelines in code-generation scenarios for HTTP API handlers, where it is simpler to emit the code that builds pipelines than all of the code that they embody.

## Corvus.Pipelines vs. LINQ to objects

Q: But can't I do everything this does, and more, with assorted `Func<,>` and LINQ to objects?

A: Yes; you can. But...

The idea of **Corvus.Pipelines** is to _constrain_ what you can do to make the programming model simpler, and enable some common patterns like retries, cancellation, logging and `Exception`-free error handling.

At the end of the day, it is just built on top of dotnet delegates/`Func<,>`, and you can bring in as much LINQ to objects as you like, around that! There's no special magic going on.

# Getting started

## What are steps?

A [step](./docs/ubiquitous-language.md#step) is a simple function that takes a value and returns another value of the same type. We call this value the [state](./docs/ubiquitous-language.md#state).

Let's illustrate that with a diagram. We will use diagrams like this throughout to explore various features of the library.

```mermaid
flowchart LR
    i2([inputState])-->step-->o2([outputState])
```

Rounded rectangles will represent [states](./docs/ubiquitous-language.md#state). Regular rectangles will represent [steps](./docs/ubiquitous-language.md#step). Arrows represent input and output from a function (in this case, the step which is, as you recall, a function).

Here's an example of a step in code.

```csharp
SyncPipelineStep<int> addOne = static state => state + 1;
```

If we execute the step with some given initial state (in this case the integer `1`), the resulting state is, as you might expect, `2`.

```csharp
// (1 + 1) = 2
int result = addOne(1);
```

## Composing steps in a pipeline
We can compose one or more of these steps into a [pipeline](./docs/ubiquitous-language.md#pipeline).

In a pipeline, each step operates on the output of the previous step, to produce the final result.

```mermaid
flowchart LR
    i1([inputState])-->step1-->o1([output1])
    o1-->step2-->o2([output2])

```

Here is a very simple pipeline whose state is an `int`.

```csharp
PipelineStep<int> pipeline = Pipeline.Build<int>(
    static state => state + 1,
    static state => state * 2,
    static state => state - 1);

// ((1 + 1) * 2) - 1 = 3
int output = await pipeline(1).ConfigureAwait(false);
```

You can see that a pipeline is, itself, just another step.

We are using a function called `Build()` to create the pipeline step. This kind of function is called an [operator](./docs/ubiquitous-language.md#operator). We'll discuss operators in more detail later.

When we want to draw a diagram of a step which is composed of other steps, we will usually represent it inside a box, like this.

```mermaid
flowchart LR
    state1([inputState])-->step1
    subgraph boundStep[composedStep]
        step1-->state2([state2])
        state2-->step2
    end
    step2-->state3([outputState])
```

Sometimes we show the input or result states inside the composed-step box, sometimes outside the box (like this), and sometimes both - it depends on what we are trying to illustrate.

## Sync and Async steps

The individual steps passed to the `Build()` operator were all synchronous - but you can use async steps too.

You may have noticed that the step produced by the `Build()` operator was itself async - we had to `await` the result.

Async steps are the "natural" form in **Corvus.Pipelines**.

But rather than returning a `Task<TState>` we use `ValueTask<TState>`, so you avoid unnecessary overhead when operating in a purely synchronous context.

Here's an example of an async step. It makes use of `Task.Delay()`.

```csharp
PipelineStep<int> pipeline = Pipeline.Build<int>(
    static async state =>
    {
        await Task.Delay(1000).ConfigureAwait(false);
        return state * 2;
    });
```

If you want to mix sync and async steps in a single pipeline, you need to coerce your synchronous steps into the async form. One way to do this is to wrap your synchronous results using `ValueTask.FromResult()`. This offers a very low-overhead adapter from sync-to-async.

```csharp
PipelineStep<int> pipeline = Pipeline.Build<int>(
    static state => ValueTask.FromResult(state + 1),
    static async state => { await Task.Delay(0); return state * 2; },
    static state => ValueTask.FromResult(state - 1));
```

> Alternatively, you can use the `ToAsync()` operator on the step itself. We'll see an example of that in a moment.

## Termination

Piplelines can be _terminating_, or _non-terminating_.

We have already seen examples of non-terminating pipelines (they just run each step in turn, from the first to the last).

A terminating pipeline takes a _predicate_ to determine if it should terminate, along with the usual array of steps.

A predicate is a function that takes a value (in this case the current state), and returns `true` or `false`. We will use the lozenge symbol to indicate a predicate in our diagrams, and dotted lines for the possible results.

```mermaid
flowchart
    p1{{Predicate}}
    p1-.->true([true])
    p1-.->false([false])
```

> In fact, we will use a lozenge for any decision/choice entity, not just a boolean predicate.

A terminating pipeline operates in essentially the same way as a non-terminating pipeline, passing the output of each step as the input to the next one. However, after each step, it executes the predicate to determine if it should stop. If so, it terminates and returns the state at that point - executing no further steps.

```mermaid
flowchart LR
    i1([inputState])-->terminate0{{terminate?}}
    subgraph terminatingPipeline
        terminate0-.(true).->t0o([inputState])
        terminate0-.(false).->step1
        step1-->o1([output1])
        o1-->terminate1{{terminate?}}
        terminate1-.(true).->t1o([output1])
        terminate1-.(false).->step2
        step2-->o2([output2])
        o2-->terminate2{{terminate?}}
        terminate2-.(true).->t2o([output2])
        terminate2-.(false).->step3
        step3==>o([etc])
    end
    t0o-->output([outputState])
    t1o-->output
    t2o-->output
```

> Note that it tests the predicate *before* executing the first step, and terminates if the state already meets the condition, and it *does not* test the predicate after the last step - it just runs to completion.

Let's look at an example of that in code.

First, let's pull out a common step we can reuse.

```csharp
static class CommonSteps
{
    public static SyncPipelineStep<int> MultiplyBy5 =
        state => state * 5;
}
```

> One of the powerful features of **Corvus.Pipelines** is that you can build up a library of steps in your application.
>
> Static instances can be reused and help keep allocations low, and performance high. They also ensure you are not silently capturing extra state at runtime - you'll get a compiler error if you try.

Then, we use the `Build()` operator overload that takes a _termination predicate_.

```csharp
PipelineStep<int> pipeline = Pipeline.Build(
    shouldTerminate: state => state > 25,
    CommonSteps.MultiplyBy5,
    CommonSteps.MultiplyBy5
);
```

In this case we are telling the pipeline to terminate if the state is `>25`.

If we run the pipeline with `1` as the input, we get

- `1` is not `>25`, so it goes on to execute the first step.
- `(1 * 5) = 5` is produced by the first step.
- `5` is not `>25`, so the pipeline continues and
- `(5 * 5) = 25` is produced by the second step.
- `25` is not `>25` so this is passed on and becomes the final output.

```csharp
// (1 * 5) * 5 = 25
int output = await pipeline(1).ConfigureAwait(false);
```

However, if we pass `6` as the input to the pipeline, we get

- `6` is not `>25` so it goes on to execute the first step.
- `(6 * 5) = 30` is produced and
- `30` _is_ `>25` so the pipeline will terminate and return the result. `30`. The second step is not executed.

```csharp
// (6 * 5) [Terminate] = 30
int output = await pipeline(6).ConfigureAwait(false);
```

## Branching

Often, you want to choose a different branch down the pipeline based on the current state.

```mermaid
flowchart
step1-->step2-->c1{{choose your own adventure}}
subgraph s1[path1]
    s1a-->s1b-->s1c
end
subgraph s2[path2]
    s2a-->s2b-->s2c
end
subgraph s3[path3]
    s3a-->s3b-->s3c
end
c1-.->s1[path1]
c1-.->s2[path2]
c1-.->s3[path3]
s1-->step3
s2-->step3
s3-->step3
step3-->step4

```

> I've not illustrated the state in this daigram; you can assume it flows in and out of the steps in the usual way, along the arrows.

This diagram illustrates a case where we execute `step1`  and `step2`, then, based on the state at that point (and maybe other things available to us in our execution environment), we choose to execute either `path1`, `path2`, or `path3`, the result of which is fed into step 3.

We provide a `Choose()` operator for exactly these circumstances.

Before we look at how `Choose()` works, let's see how we use it.

Here are some steps that we can imagine work on values in some invoicing application.

```csharp
static class InvoiceSteps
{
    public static SyncPipelineStep<decimal> ApplyLowDiscount =
        state => Math.Ceiling(state * 100 * 0.8m) / 100;
    public static SyncPipelineStep<decimal> ApplyHighDiscount =
        state => Math.Ceiling(state * 100 * 0.7m) / 100;
    public static SyncPipelineStep<decimal> ApplySalesTax =
        state => Math.Ceiling(state * 100 * 1.2m) / 100;
}
```

We have two discount policy steps, one for a low invoice total, and one for a high invoice total; plus a step that applies sales tax.

And here's a pipeline that uses those steps to process an invoice total:

```csharp
public static PipelineStep<decimal> ApplyDiscountAndTax =
    Pipeline.Build(
        Pipeline.Current<decimal>().Choose(
            selector: static state => state > 1000m
                ? InvoiceSteps.ApplyHighDiscount
                : InvoiceSteps.ApplyLowDiscount),
        InvoiceSteps.ApplySalesTax.ToAsync()
);
```

This pipeline selects a discount step based on the current value, then applies sales tax to the discounted amount.

The discount selection is performed by function provided to the `Choose()` operator, which takes the input state and returns a suitable step.

In this case the function looks at the state, and returns the `ApplyHighDiscount` step if the state is `>1000`, and the `ApplyLowDiscount` step if the state is `<=1000`.

> Remember we mentioned the `ToAsync()` operator earlier, and that it was another way to convert a sync step to an async step for use in an async context. Here we are using it to convert our `ApplySalesTax` step (which is synchronous) to an async step, to match the async step returned by `Choose()`.

So, let's see what happens if we run our pipeline with the value `1000`

```csharp
// 1000m => Choose[<=1000m] =>
//     ApplyLowDiscount () => (1000 * 0.8) = 800 =>
// ApplySalesTax => (800 * 1.2) = 960
decimal output = await pipeline(1000m).ConfigureAwait(false);
```

The step produced by `Choose()` selects and executes the low discount step because `1000` is `<=1000`. It executes that step with the current state, to produce `800`.

Once it has completed that, we are back on the main flow of the pipeline, and it continues with the next step which is `ApplySalesTax`. 

This takes that value and produces `(800 * 1.2) = 960`.

So the overall pipeline has applied the 20% discount, before applying the sales tax.

If on the other hand, we pass `2000` as the input state, we take a different path.

```csharp
// 2000m => Choose[>1000m] =>
//     ApplyHighDiscount () => (2000 * 0.7) = 1400 =>
// ApplySalesTax => (1400 * 1.2) = 1680
decimal output = await pipeline(2000m).ConfigureAwait(false);
```

The step produced by `Choose()` selects and executes the *high* discount step because `2000` is `<=1000`. It executes that step with the current state, to produce `1400`.

Once it has completed that, we are back on the main flow of the pipeline, and it continues with the next step which is `ApplySalesTax`. 

This takes that value and produces `(1400 * 1.2) = 1680`.

So the overall pipeline has applied the 30% discount instead of the 20% discount, before applying the sales tax.

Notice that in this case we used a simple either-or choice - but you could write arbitrarily complex logic to choose between steps to emit - or even build a custom pipeline on-the-fly.

Equally, the chosen step could be an arbitrarily complex pipeline. For example, it could look at anything else it had access to from the state it was passed to determine the discount level. We will look at more complex state later.

So `Choose()` is a function that has allowed us to make a decision about which step to execute next, based on the output from the previous step.

Like `Build()` it has composed multiple steps together, to produce a single step that exhibits more complex behaviour.

We call this kind of a function an [operator](./docs/ubiquitous-language.md#operator).

## Applying operators

We'll start with a definition, introduce some new notation for our digrams, then move on to an example.

### Definition

**An operator is a function that takes a step** (along with other parameters that are available at pipeline-build time, including other steps) **and returns a step.**

> For the mathematically minded, this somewhat akin to the mathematical definition of an operator as a function that maps from one function space to another.

Unlike steps, which execute when the pipeline is _run_, operators execute when the pipeline is _built_, in order to create new steps.

### Notation

We are going to introduce a symbol to represent operators in our diagrams. Operators are functions, so we will stick with the rectangular shape, but they are functions that produce functions, so we will use a double rectangle.

```mermaid
flowchart
    b[[OperatorName]]
```

### Bind: a simple but powerful operator
Now that we have a new symbol to use, let's explore a real operator in more detail.

We'll look at `Choose()` again in a moment, but first let's look at one of the simplest operators:  `Bind()`.

There are several versions of `Bind()` available in the library, but the simplest one takes two steps and produces a step that _binds_ them together.

The step it creates 
- executes the first step with its input
- executes the second step with the output of the first step
- returns the output of the second step as its result

Let's look at a diagram which illustrates how the bind operator takes the two steps and produces a step which combines them in that way.

```mermaid
flowchart
    stepA[step1]-->bind
    stepB[step2]-->bind[[Bind]]
    bind-->boundStep
    subgraph boundStep[boundStep]
        state1([inputState])-->step1
        step1-->state2([state2])
        state2-->step2
        step2-->state3([outputState])
    end
```
You can then use the new compound step in the usual way.

```mermaid
flowchart LR
    i2([inputState])-->boundStep-->o2([outputState])
```

> Remember: the operator *does not* execute any steps; it builds a step that executes steps (and other logic).

Here's an example using our invoicing steps.

```csharp
public static SyncPipelineStep<decimal> ApplyHighDiscountAndSalesTax = ApplyHighDiscount.Bind(ApplySalesTax);
```

We can now execute this step and it is the equivalent of executing each of the previous steps in turn.

```csharp
// (1000 * 0.7) * 1.2 = 840
var result = InvoiceSteps.ApplyHighDiscountAndSalesTax(1000);
```

Notice that `Bind()` *does not* execute the step there and then. It produces a step which _can be_ executed as part of a pipeline. That is the essential distinction between an operator, and a step.

Although it is seemingly simple, this ability to compose steps using `Bind()` underpins the whole concept of a pipeline.

Remember our `Pipeline.Build()` operator? You can think of that being built on `Bind()`.

It is equivalent to calling:

```csharp
steps[0].Bind(steps[1]).Bind(steps[2]) /*...*/ .Bind(steps[n])
```

> In practice, it is implemented differently for optimization purposes. But conceptually, it is built on this `Bind()` operator.

Most of the rest of this document will look at using other operators to compose more sophisticated pipelines.

### Revisiting Choose()

Now that we know how operators work, we should be able to figure out what `Choose()` actually does.

It produces a step that conditionally executes some other step based on its selector.

```mermaid
flowchart LR
    step-->choose
    sel{{selector}}-->choose
    choose[[Choose]]-->chooseStep
    subgraph chooseStep[ ]
        s[step]-->c1
        subgraph selGraph[selector]
            c1{{selector}}-.condition1.->step1
            c1-.condition2.->step2
            c1-.condition3.->step3
        end
        step1-.->selectedStep
        step2-.->selectedStep
        step3-.->selectedStep
    end
        selectedStep[selectedStep]-->output([output])
```

## The current state

What if you want to use an operator, such as `Choose()` but you don't have an existing step to which to apply it?

We've already seen an example of this in our invoice processing code. Let's remind ourselves of that:

```csharp
PipelineStep<decimal> pipeline = Pipeline.Build(
    Pipeline.Current<decimal>().Choose(
        selector: static state => state > 1000m
            ? InvoiceSteps.ApplyHighDiscount
            : InvoiceSteps.ApplyLowDiscount),
    InvoiceSteps.ApplySalesTax.ToAsync()
);
```

Notice that the `Choose()` operator is applied to the output from another special operator called `Pipeline.Current()`.

This operator is special because it doesn't take an existing step, but it still provides step which, when executed, just returns the current state of the pipeline.

This, in fact, gives you the "something" on which an operator can operate, if you do not otherwise have some specific step. We've already seen how this effectively threads the current state through the pipeline when it is executed.

## More ways to Bind()

Sometimes you will have reusable steps that operate on some part of your state, rather than the state as a whole.

Or the step requires some different state which includes information which can be derived from your existing state, or can be augmented with information from elsewhere in your execution environment (such as by calling another API with parameters provided from your state).

Or maybe the step needs its input state to support different capabilities such as error reporting or cancellation, that your state doesn't.

> We will learn more about [capabilities](./docs/ubiquitous-language.md#capability) later.

We will need to be able to convert from our existing step's `TState` to some `TInnerState` used by our bound step.

There are overloads of `Bind()` that do exactly that.

In a addition to the usual _bound step_, they take two mapping functions: `wrap()` and `unwrap()`, and produce a step like this:

```mermaid
flowchart
s[step1]-->bind
bs[step2]-->bind
w[wrap]-->bind
u[unwrap]-->bind
bind[[Bind]]-->adapterStep
subgraph adapterStep[boundStep]
    s0([inputState :TState])-->sA[step1]-->s1
    s1([stepOutput :TState])-->wrap
    wrap-->inner1([innerState :TInnerState])
    inner1-->step2
    step2-->inner2([innerResult :TInnerState])
    inner2-->unwrap
    unwrap-->s2([outputState :TState])
end

style w stroke-dasharray: 5 5
style u stroke-dasharray: 5 5
style wrap stroke-dasharray: 5 5
style unwrap stroke-dasharray: 5 5

```

> We've used a dashed rectangle to indicate a function that is neither a step, nor a predictate/conditional.

The first function (`wrap`) takes the value return by the initial step, and returns an instance of the type required by the bound step.

```csharp
Func<TState, TInnerState> wrap
```

The second function (`unwrap`) takes the _both_ the value returned by the initial step, _and_ the value returned by the bound step, and maps it to an instance of the type required by the initial step.

```csharp
Func<TState, TInnerState, TState> unwrap
```

So, when the step produced by the `Bind()` operator is executed, it:

- Executes the initial step
- Calls `wrap()`, passing it the result of the initial step.
- It then executes the bound step with the result from `wrap()`.
- The result of that is passed to the `unwrap()` function.
- The result of that is returned from the step.

# Concepts

A lot of the language used in this project is heavily overloaded with meaning in other domains, so it is useful to familiarize ourselves with some common terms, and what we take them to mean in this domain.

There is a separate document in this repository called [ubiquitous-language.md](./docs/ubiquitous-language.md) which includes these definitions in alphabetized form for easy reference.
