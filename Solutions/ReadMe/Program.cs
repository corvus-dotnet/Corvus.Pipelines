using Corvus.Pipelines;
using Corvus.Pipelines.Handlers;
using ReadMe;

// ## What are steps?
Console.WriteLine("## What are steps?");

SyncPipelineStep<int> addOne = static state => state + 1;

int result = addOne(1);

Console.WriteLine(result);

// ## Composing steps into a pipeline
Console.WriteLine();
Console.WriteLine("## Composing steps into a pipeline");

SyncPipelineStep<int> syncPipeline = Pipeline.Build<int>(
    static state => state + 1,
    static state => state * 2,
    static state => state - 1);

result = syncPipeline(1);

Console.WriteLine(result);

// ## Sync and Async steps
Console.WriteLine();
Console.WriteLine("## Sync and Async steps");

PipelineStep<int> asyncPipeline = Pipeline.Build<int>(
    static async state =>
    {
        await Task.Delay(1000).ConfigureAwait(false);
        return state * 2;
    });

result = await asyncPipeline(2);

asyncPipeline = Pipeline.Build<int>(
    static state => ValueTask.FromResult(state + 1),
    static async state => { await Task.Delay(0); return state * 2; },
    static state => ValueTask.FromResult(state - 1));

result = await asyncPipeline(1);

Console.WriteLine(result);

// ## Termination
Console.WriteLine();
Console.WriteLine("## Termination");

SyncPipelineStep<int> terminatingPipeline = Pipeline.Build(
    shouldTerminate: state => state > 25,
    CommonSteps.MultiplyBy5,
    CommonSteps.MultiplyBy5
);

result = terminatingPipeline(1);

Console.WriteLine(result);

result = terminatingPipeline(6);

Console.WriteLine(result);

// ## Branching
Console.WriteLine();
Console.WriteLine("## Branching");

SyncPipelineStep<decimal> chooseDiscount =
    Pipeline.Choose(
            selector: static (decimal state) =>
            state switch
            {
                > 1000m => InvoiceSteps.ApplyHighDiscount,
                > 500m => InvoiceSteps.ApplyLowDiscount,
                _ => Pipeline.CurrentSync<decimal>(),
            });

SyncPipelineStep<decimal> invoicePipeline =
    Pipeline.Build(
        chooseDiscount,
        InvoiceSteps.ApplySalesTax);

decimal value = invoicePipeline(1000m);

Console.WriteLine(value);

value = invoicePipeline(2000m);

Console.WriteLine(value);

value = invoicePipeline(100m);

Console.WriteLine(value);

// ### Bind: a simple but powerful operator
Console.WriteLine();
Console.WriteLine("### Bind: a simple but powerful operator");

SyncPipelineStep<decimal> applyHighDiscountAndSalesTax =
       InvoiceSteps.ApplyHighDiscount.Bind(InvoiceSteps.ApplySalesTax);

value = applyHighDiscountAndSalesTax(1000);

Console.WriteLine(value);


// ## Pipelines and Handlers
Console.WriteLine();
Console.WriteLine("## Pipelines and Handlers");

string productId = "Catalog2_Product2";

HandlerState<string, decimal> pricingResult =
    PricingCatalogs.PricingHandler(
        HandlerState<string, decimal>.For(productId));

Console.Write(productId);
Console.Write(" ");

if (pricingResult.WasHandled(out decimal price))
{
    Console.WriteLine(price);
}
else
{
    Console.WriteLine("was not priced");
}

// ### Example: binding to a handler pipeline
Console.WriteLine();
Console.WriteLine("### Example: binding to a handler pipeline");

SyncPipelineStep<ProductPrice> lookupProductPrice =
    PricingCatalogs.PricingHandler.Bind(
        (ProductPrice state) =>
            HandlerState<string, decimal>.For(state.ProductId),
        (outerState, innerState) =>
            new ProductPrice(
                innerState.Input,
                innerState.WasHandled(out decimal result)
                    ? result : null));

SyncPipelineStep<ProductPrice> discountProductPrice =
    invoicePipeline.Bind(
        (ProductPrice state) => state.Price ?? 0m,
        (outerState, innerState) => new ProductPrice(outerState.ProductId, innerState));

SyncPipelineStep<ProductPrice> lookupPriceAndDiscount =
    lookupProductPrice.Bind(discountProductPrice);

ProductPrice productPricingResult = lookupPriceAndDiscount(new ProductPrice(productId, null));

Console.WriteLine(productPricingResult);

productPricingResult = lookupPriceAndDiscount(new ProductPrice("You won't find me!", null));

Console.WriteLine(productPricingResult);

// ### When to use Bind(), and when to build custom operators?
Console.WriteLine();
Console.WriteLine("### When to use Bind(), and when to build custom operators?");

static SyncPipelineStep<TState> ChooseWithHandlerPipeline<TState>(
    SyncPipelineStep<TState> notHandled,
    params SyncPipelineStep<HandlerState<TState, SyncPipelineStep<TState>>>[] handlers)
    where TState : struct
    => HandlerPipeline.Build(handlers)
        .Bind(
            (TState state)
                => HandlerState<TState, SyncPipelineStep<TState>>.For(state),
            (TState state, HandlerState<TState, SyncPipelineStep<TState>> handlerState)
                => handlerState.WasHandled(out SyncPipelineStep<TState>? result)
                    ? result(state)
                    : notHandled(state));

SyncPipelineStep<decimal> chooseDiscountWithHandler =
    ChooseWithHandlerPipeline(
        Pipeline.CurrentSync<decimal>(),
        state => state.Input > 1000m ? state.Handled(InvoiceSteps.ApplyHighDiscount) : state.NotHandled(),
        state => state.Input > 500m ? state.Handled(InvoiceSteps.ApplyLowDiscount) : state.NotHandled());

SyncPipelineStep<decimal> invoicePipelineWithHandler =
    Pipeline.Build(
        chooseDiscountWithHandler,
        InvoiceSteps.ApplySalesTax);

value = invoicePipeline(1000m);

Console.WriteLine(value);

value = invoicePipeline(2000m);

Console.WriteLine(value);

value = invoicePipeline(100m);

Console.WriteLine(value);

// ## Handling exceptions with Catch()
Console.WriteLine();
Console.WriteLine("## Handling exceptions with Catch()");

productPricingResult = lookupPriceAndDiscount(new ProductPrice("You won't find me!", null));

Console.WriteLine(productPricingResult);

SyncPipelineStep<ProductPrice> saferDiscountProductPrice =
    invoicePipeline.Bind(
        (ProductPrice state) => state.Price ?? throw new InvalidOperationException("The base price was null."),
        (outerState, innerState) => new ProductPrice(outerState.ProductId, innerState));

SyncPipelineStep<ProductPrice> saferLookupPriceAndDiscount =
    lookupProductPrice.Bind(saferDiscountProductPrice);

productPricingResult = saferLookupPriceAndDiscount(new ProductPrice(productId, null));

Console.WriteLine(productPricingResult);

try
{
    productPricingResult = saferLookupPriceAndDiscount(new ProductPrice("You won't find me!", null));
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}

SyncPipelineStep<ProductPrice> safestLookupPriceAndDiscount =
    saferLookupPriceAndDiscount.Catch(
        (ProductPrice state, InvalidOperationException ex) => new(state.ProductId, null));


productPricingResult = safestLookupPriceAndDiscount(new ProductPrice("You won't find me!", null));

Console.WriteLine(productPricingResult);

// ## Value Provider
Console.WriteLine();
Console.WriteLine("## Value Provider");


var stateWithValue = StateWithValue.For(12m);

Console.WriteLine(stateWithValue.Value);

stateWithValue = stateWithValue.WithValue(20m);

Console.WriteLine(stateWithValue.Value);

stateWithValue = stateWithValue with { Value = 25m };

Console.WriteLine(stateWithValue.Value);

// ## Can Fail
Console.WriteLine();
Console.WriteLine("## Can Fail");


var stateCanFail = CanFailState.For(12m);

Console.WriteLine($"{stateCanFail.Value} : {stateCanFail.ExecutionStatus}");

stateCanFail = stateCanFail.PermanentFailure();

Console.WriteLine($"{stateCanFail.Value} : {stateCanFail.ExecutionStatus}");

stateCanFail = stateCanFail.TransientFailure();

Console.WriteLine($"{stateCanFail.Value} : {stateCanFail.ExecutionStatus}");

stateCanFail = stateCanFail.Success();

Console.WriteLine($"{stateCanFail.Value} : {stateCanFail.ExecutionStatus}");

// ## The `Retry()` Operator
Console.WriteLine();
Console.WriteLine("## The `Retry()` Operator");

SyncPipelineStep<CanFailState<int>> stepCanFail =
    state =>
        state.Value == 0 && state.ExecutionStatus == PipelineStepStatus.Success
            ? state.TransientFailure()
            : CanFailState.For(state.Value + 1);

CanFailState<int> canFailInt = stepCanFail(CanFailState.For(1));

Console.WriteLine($"{canFailInt.Value} : {canFailInt.ExecutionStatus}");

canFailInt = stepCanFail(CanFailState.For(0));
Console.WriteLine($"{canFailInt.Value} : {canFailInt.ExecutionStatus}");

canFailInt = stepCanFail(canFailInt);
Console.WriteLine($"{canFailInt.Value} : {canFailInt.ExecutionStatus}");

SyncPipelineStep<CanFailState<int>> retryingTransientFailure =
        stepCanFail.Retry(Retry.TransientPolicy<CanFailState<int>>());

canFailInt = retryingTransientFailure(CanFailState.For(0));

Console.WriteLine($"{canFailInt.Value} : {canFailInt.ExecutionStatus}");
