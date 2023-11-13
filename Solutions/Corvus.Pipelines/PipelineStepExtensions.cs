// <copyright file="PipelineStepExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Corvus.Pipelines;

/// <summary>
/// Operators for <see cref="PipelineStep{TState}"/>.
/// </summary>
public static class PipelineStepExtensions
{
    private const string NameFeature = "Name";

    /// <summary>
    /// Create a named step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="step">The step.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>A named step.</returns>
    public static SyncPipelineStepProvider<TState> WithName<TState>(this SyncPipelineStep<TState> step, [CallerArgumentExpression(nameof(step))] string? name = null)
        where TState : struct
        => new SyncPipelineStepProvider<TState>(step).AddFeature(NameFeature, name!);

    /// <summary>
    /// Create a named step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="step">The step.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>A named step.</returns>
    public static SyncPipelineStepProvider<TState> WithName<TState>(this SyncPipelineStepProvider<TState> step, [CallerArgumentExpression(nameof(step))] string? name = null)
        where TState : struct
        => step.AddFeature(NameFeature, name!);

    /// <summary>
    /// Gets the name of the step.
    /// </summary>
    /// <typeparam name="TState">The type of the state for the step.</typeparam>
    /// <param name="step">The step provider.</param>
    /// <returns>the name of the step, or <see cref="string.Empty"/> if the step has no name.</returns>
    public static string Name<TState>(this SyncPipelineStepProvider<TState> step)
        where TState : struct
    {
        if (step.TryGetFeature(NameFeature, out string? name))
        {
            return name;
        }

        return string.Empty;
    }

    /// <summary>
    /// Gets the name of the step.
    /// </summary>
    /// <typeparam name="TState">The type of the state for the step.</typeparam>
    /// <param name="step">The step provider.</param>
    /// <returns>the name of the step, or <see cref="string.Empty"/> if the step has no name.</returns>
    public static bool HasName<TState>(this SyncPipelineStepProvider<TState> step)
        where TState : struct
    {
        return step.HasFeature<string>(NameFeature);
    }

    /// <summary>
    /// Create a named step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="step">The step.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>A named step.</returns>
    public static PipelineStepProvider<TState> WithName<TState>(this PipelineStep<TState> step, [CallerArgumentExpression(nameof(step))] string? name = null)
        where TState : struct
        => new PipelineStepProvider<TState>(step).AddFeature(NameFeature, name!);

    /// <summary>
    /// Create a named step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="step">The step.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>A named step.</returns>
    public static PipelineStepProvider<TState> WithName<TState>(this PipelineStepProvider<TState> step, [CallerArgumentExpression(nameof(step))] string? name = null)
        where TState : struct
        => step.AddFeature(NameFeature, name!);

    /// <summary>
    /// Gets the name of the step.
    /// </summary>
    /// <typeparam name="TState">The type of the state for the step.</typeparam>
    /// <param name="step">The step provider.</param>
    /// <returns>the name of the step, or <see cref="string.Empty"/> if the step has no name.</returns>
    public static string Name<TState>(this PipelineStepProvider<TState> step)
        where TState : struct
    {
        if (step.TryGetFeature(NameFeature, out string? name))
        {
            return name;
        }

        return string.Empty;
    }

    /// <summary>
    /// Gets the name of the step.
    /// </summary>
    /// <typeparam name="TState">The type of the state for the step.</typeparam>
    /// <param name="step">The step provider.</param>
    /// <returns>the name of the step, or <see cref="string.Empty"/> if the step has no name.</returns>
    public static bool HasName<TState>(this PipelineStepProvider<TState> step)
        where TState : struct
    {
        return step.HasFeature<string>(NameFeature);
    }

    /// <summary>
    /// An operator which returns a step which executes an action on entry to and exit from the input step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="step">The step for which to log exit.</param>
    /// <param name="logEntry">The action to perform on entry. It is provided with the state before the step.</param>
    /// <param name="logExit">The action to perform on exit. It is provided with the state both before and after the step.</param>
    /// <returns>A step which wraps the input step and logs on entry and exit.</returns>
    public static PipelineStep<TState> OnEntryAndExit<TState>(this PipelineStep<TState> step, Action<TState> logEntry, Action<TState, TState> logExit)
        where TState : struct
    {
        return step.Bind(
            (TState state) =>
            {
                logEntry(state);
                return state;
            },
            (state, innerState) =>
            {
                logExit(state, innerState);
                return innerState;
            });
    }

    /// <summary>
    /// An operator which returns a step which executes an action on entry to the input step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="step">The step for which to log exit.</param>
    /// <param name="logEntry">The action to perform on entry. It is provided with the state before the step.</param>
    /// <returns>A step which wraps the input step and logs on entry.</returns>
    public static PipelineStep<TState> OnEntry<TState>(this PipelineStep<TState> step, Action<TState> logEntry)
        where TState : struct
    {
        return step.Bind(
            (TState state) =>
            {
                logEntry(state);
                return state;
            },
            (_, innerState) => innerState);
    }

    /// <summary>
    /// An operator which returns a step which executes an action on exit from the input step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="step">The step for which to log exit.</param>
    /// <param name="onExit">The action to perform on exit. It is provided with the state both before and after the step.</param>
    /// <returns>A step which wraps the input step and logs on entry.</returns>
    public static PipelineStep<TState> OnExit<TState>(this PipelineStep<TState> step, Action<TState, TState> onExit)
        where TState : struct
    {
        return step.Bind(
            (TState state) => state,
            (entryState, exitState) =>
            {
                onExit(entryState, exitState);
                return exitState;
            });
    }

    /// <summary>
    /// An operator which provides a step that catches an exception thrown by a step, and passes it to a handler.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TException">The type of the exception.</typeparam>
    /// <param name="step">The step to wrap with exception handling.</param>
    /// <param name="exceptionHandler">The exception handler, which takes the input state and exception, and returns a resulting state instance.</param>
    /// <returns>A step which wraps the input step, and provides the exception handling capability.</returns>
    /// <remarks>
    /// This is commonly used in conjunction with the termination capability of a <see cref="Pipeline"/>, and/or a
    /// <see cref="ICanFail"/> step with permanent or transient failure handling via
    /// operators like <see cref="Retry{TState}(PipelineStep{TState}, Predicate{RetryContext{TState}}, PipelineStep{RetryContext{TState}}?)"/> and
    /// <see cref="OnError{TState}(PipelineStep{TState}, PipelineStep{TState})"/>.
    /// </remarks>
    public static PipelineStep<TState> Catch<TState, TException>(this PipelineStep<TState> step, Func<TState, TException, ValueTask<TState>> exceptionHandler)
        where TState : struct
        where TException : Exception
    {
        return async state =>
        {
            try
            {
                return await step(state).ConfigureAwait(false);
            }
            catch (TException ex)
            {
                return await exceptionHandler(state, ex).ConfigureAwait(false);
            }
        };
    }

    /// <summary>
    /// An operator which provides a step that catches an exception thrown by a step, and passes it to a handler.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TException">The type of the exception.</typeparam>
    /// <param name="step">The step to wrap with exception handling.</param>
    /// <param name="exceptionHandler">The exception handler, which takes the input state and exception, and returns a resulting state instance.</param>
    /// <returns>A step which wraps the input step, and provides the exception handling capability.</returns>
    /// <remarks>
    /// This is commonly used in conjunction with the termination capability of a <see cref="Pipeline"/>, and/or a
    /// <see cref="ICanFail"/> step with permanent or transient failure handling via
    /// operators like <see cref="Retry{TState}(PipelineStep{TState}, Predicate{RetryContext{TState}}, PipelineStep{RetryContext{TState}}?)"/> and
    /// <see cref="OnError{TState}(PipelineStep{TState}, PipelineStep{TState})"/>.
    /// </remarks>
    public static PipelineStep<TState> Catch<TState, TException>(this PipelineStep<TState> step, Func<TState, TException, TState> exceptionHandler)
        where TState : struct
        where TException : Exception
    {
        return async state =>
        {
            try
            {
                return await step(state).ConfigureAwait(false);
            }
            catch (TException ex)
            {
                return exceptionHandler(state, ex);
            }
        };
    }

    /// <summary>
    /// An operator which provides a step that catches an exception thrown by a step, and passes it to a handler.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TException">The type of the exception.</typeparam>
    /// <param name="step">The step to wrap with exception handling.</param>
    /// <param name="exceptionHandler">The exception handler, which takes the input state and exception, and returns a resulting state instance.</param>
    /// <returns>A step which wraps the input step, and provides the exception handling capability.</returns>
    /// <remarks>
    /// This is commonly used in conjunction with the termination capability of a <see cref="Pipeline"/>, and/or a
    /// <see cref="ICanFail"/> step with permanent or transient failure handling via
    /// operators like <see cref="Retry{TState}(PipelineStep{TState}, Predicate{RetryContext{TState}}, PipelineStep{RetryContext{TState}}?)"/> and
    /// <see cref="OnError{TState}(SyncPipelineStep{TState}, SyncPipelineStep{TState})"/>.
    /// </remarks>
    public static SyncPipelineStep<TState> Catch<TState, TException>(this SyncPipelineStep<TState> step, Func<TState, TException, TState> exceptionHandler)
        where TState : struct
        where TException : Exception
    {
        return state =>
        {
            try
            {
                return step(state);
            }
            catch (TException ex)
            {
                return exceptionHandler(state, ex);
            }
        };
    }

    /// <summary>
    /// An operator which provides the ability to retry a step which might fail.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="step">The step to execute.</param>
    /// <param name="shouldRetry">A predicate which determines if the step should be retried.</param>
    /// <param name="beforeRetry">An step to carry out before retrying. This is commonly an asynchronous delay, but can be used to return
    /// an updated version of the state before retyring the action (e.g. incrementing an execution count.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which, when executed, will execute the step, choose the appropriate pipeline, based on the result,
    /// and execute it using the result.</returns>
    public static PipelineStep<TState> Retry<TState>(this PipelineStep<TState> step, Predicate<RetryContext<TState>> shouldRetry, PipelineStep<RetryContext<TState>>? beforeRetry = null)
        where TState : struct, ICanFail
    {
        return async state =>
        {
            TState currentState = state;
            DateTimeOffset initialTime = DateTimeOffset.UtcNow;

            int tryCount = 1;

            while (true)
            {
                currentState = await step(currentState).ConfigureAwait(false);
                if (currentState.ExecutionStatus == PipelineStepStatus.Success)
                {
                    return currentState;
                }

                RetryContext<TState> retryContext = new(currentState, DateTimeOffset.UtcNow - initialTime, tryCount);

                if (!shouldRetry(retryContext))
                {
                    return currentState;
                }

                if (beforeRetry is not null)
                {
                    (currentState, TimeSpan _, int _) = await beforeRetry(retryContext).ConfigureAwait(false);
                }

                tryCount++;
            }
        };
    }

    /// <summary>
    /// An operator which provides the ability to retry a step which might fail.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="step">The step to execute.</param>
    /// <param name="shouldRetry">A predicate which determines if the step should be retried.</param>
    /// <param name="beforeRetry">An step to carry out before retrying. This is commonly an asynchronous delay, but can be used to return
    /// an updated version of the state before retyring the action (e.g. incrementing an execution count.</param>
    /// <returns>A <see cref="SyncPipelineStep{TState}"/> which, when executed, will execute the step, choose the appropriate pipeline, based on the result,
    /// and execute it using the result.</returns>
    public static SyncPipelineStep<TState> Retry<TState>(this SyncPipelineStep<TState> step, Predicate<RetryContext<TState>> shouldRetry, SyncPipelineStep<RetryContext<TState>>? beforeRetry = null)
        where TState : struct, ICanFail
    {
        return state =>
        {
            TState currentState = state;
            DateTimeOffset initialTime = DateTimeOffset.UtcNow;

            int tryCount = 1;

            while (true)
            {
                currentState = step(currentState);
                if (currentState.ExecutionStatus == PipelineStepStatus.Success)
                {
                    return currentState;
                }

                RetryContext<TState> retryContext = new(currentState, DateTimeOffset.UtcNow - initialTime, tryCount);

                if (!shouldRetry(retryContext))
                {
                    return currentState;
                }

                if (beforeRetry is not null)
                {
                    (currentState, TimeSpan _, int _) = beforeRetry(retryContext);
                }

                tryCount++;
            }
        };
    }

    /// <summary>
    /// An operator which provides the ability to choose a step to run if the bound step fails.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="step">The step to execute.</param>
    /// <param name="onError">The step to execute if the step fails.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which, when executed, will execute the step, and, if an error occurs,
    /// execute the error step before returning the final result.</returns>
    public static PipelineStep<TState> OnError<TState>(
        this PipelineStep<TState> step,
        PipelineStep<TState> onError)
        where TState : struct, ICanFail
    {
        return step.Bind(state =>
        {
            if (state.ExecutionStatus != PipelineStepStatus.Success)
            {
                return onError(state);
            }

            return ValueTask.FromResult(state);
        });
    }

    /// <summary>
    /// An operator which provides the ability to choose a step to run if the bound step fails.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="step">The step to execute.</param>
    /// <param name="onError">The step to execute if the step fails.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which, when executed, will execute the step, and, if an error occurs,
    /// execute the error step before returning the final result.</returns>
    public static PipelineStep<TState> OnError<TState>(
        this PipelineStep<TState> step,
        SyncPipelineStep<TState> onError)
        where TState : struct, ICanFail
    {
        return step.Bind(state =>
        {
            if (state.ExecutionStatus != PipelineStepStatus.Success)
            {
                return ValueTask.FromResult(onError(state));
            }

            return ValueTask.FromResult(state);
        });
    }

    /// <summary>
    /// An operator which provides the ability to choose a step to run if the bound step fails.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="step">The step to execute.</param>
    /// <param name="onError">The step to execute if the step fails.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which, when executed, will execute the step, and, if an error occurs,
    /// execute the error step before returning the final result.</returns>
    public static SyncPipelineStep<TState> OnError<TState>(
        this SyncPipelineStep<TState> step,
        SyncPipelineStep<TState> onError)
        where TState : struct, ICanFail
    {
        return step.Bind(state =>
        {
            if (state.ExecutionStatus != PipelineStepStatus.Success)
            {
                return onError(state);
            }

            return state;
        });
    }

    /// <summary>
    /// An operator that binds the output of one <see cref="PipelineStep{TState}"/> to another <see cref="PipelineStep{TState}"/>
    /// provided by a <paramref name="selector"/> function.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="step">The step whose output is bound to the selected <see cref="PipelineStep{TState}"/>.</param>
    /// <param name="selector">The selector which takes the output of the <paramref name="step"/> and chooses a pipeline with which to proceed.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which, when executed, will execute the step, choose the appropriate pipeline, based on the result,
    /// and execute it using the result.</returns>
    public static PipelineStep<TState> Choose<TState>(this PipelineStep<TState> step, Func<TState, PipelineStep<TState>> selector)
        where TState : struct
    {
        return step.Bind(state => selector(state)(state));
    }

    /// <summary>
    /// An operator that binds the output of one <see cref="PipelineStep{TState}"/> to another <see cref="PipelineStep{TState}"/>
    /// provided by a <paramref name="selector"/> function.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="step">The step whose output is bound to the selected <see cref="PipelineStep{TState}"/>.</param>
    /// <param name="selector">The selector which takes the output of the <paramref name="step"/> and chooses a pipeline with which to proceed.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which, when executed, will execute the step, choose the appropriate pipeline, based on the result,
    /// and execute it using the result.</returns>
    public static SyncPipelineStep<TState> Choose<TState>(this SyncPipelineStep<TState> step, Func<TState, SyncPipelineStep<TState>> selector)
        where TState : struct
    {
        return step.Bind(state => selector(state)(state));
    }

#pragma warning disable RCS1047 // Non-asynchronous method name should not end with 'Async'. These methods convert to async.

    /// <summary>
    /// Convert a synchronous pipeline step to an asynchronous one.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="step">The step to be converted to an async step.</param>
    /// <returns>An async version of the synchronous step.</returns>
    public static PipelineStep<TState> ToAsync<TState>(this SyncPipelineStep<TState> step)
        where TState : struct
    {
        return
            [DebuggerStepThrough]
            (state) => ValueTask.FromResult(step(state));
    }

    /// <summary>
    /// Convert a synchronous named pipeline step to an asynchronous one.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="step">The step to be converted to an async step.</param>
    /// <returns>An async version of the synchronous step.</returns>
    public static PipelineStepProvider<TState> ToAsync<TState>(this SyncPipelineStepProvider<TState> step)
        where TState : struct
    {
        return new PipelineStepProvider<TState>(step);
    }
#pragma warning restore RCS1047 // Non-asynchronous method name should not end with 'Async'.

    /// <summary>
    /// An operator that produces a step which executes two steps in parallel, and returns the result of the first step
    /// to complete, cancelling the other.
    /// </summary>
    /// <typeparam name="TState">The type of the state of the steps.</typeparam>
    /// <param name="attempt1">The step for the first attempt.</param>
    /// <param name="attempt2">The step for the second attempt.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which returns the result of the first step to return a value.</returns>
    /// <remarks>This executes the steps in parallel, returning the value from the first step that produces a result, and cancelling the other operations.</remarks>
    public static PipelineStep<TState> FirstToComplete<TState>(
        this PipelineStep<TState> attempt1,
        PipelineStep<TState> attempt2)
        where TState : struct, ICancellable<TState>
    {
        return async (TState input) =>
        {
            // Create a linked cancellation token source with whatever the current cancellation token might be
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(input.CancellationToken);
            TState wrappedInput = input.WithCancellationToken(cancellationTokenSource.Token);

            try
            {
                ValueTask<TState> task1 = attempt1(wrappedInput);

                if (task1.IsCompleted)
                {
                    return task1.Result;
                }

                ValueTask<TState> task2 = attempt2(wrappedInput);

                if (task2.IsCompleted)
                {
                    return task2.Result;
                }

                // Wait for any task to complete
                Task<TState> result = await Task.WhenAny(
                    task1.AsTask(),
                    task2.AsTask()).ConfigureAwait(false);

                return result.Result;
            }
            finally
            {
                cancellationTokenSource.Cancel();
            }
        };
    }

    /// <summary>
    /// An operator that produces a step which executes a number of steps in parallel, and returns the result of the first step
    /// to complete, cancelling the other.
    /// </summary>
    /// <typeparam name="TState">The type of the state of the steps.</typeparam>
    /// <param name="step">The step for the first attempt.</param>
    /// <param name="attempts">The steps to attempt.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which returns the result of the first step to return a value.</returns>
    /// <remarks>This executes the steps in parallel, returning the value from the first step that produces a result, and cancelling the other operations.</remarks>
    public static PipelineStep<TState> FirstToComplete<TState>(
        this PipelineStep<TState> step,
        params PipelineStep<TState>[] attempts)
        where TState : struct, ICancellable<TState>
    {
        return async (TState input) =>
        {
            // Create a linked cancellation token source with whatever the current cancellation token might be
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(input.CancellationToken);
            TState wrappedInput = input.WithCancellationToken(cancellationTokenSource.Token);

            int length = attempts.Length + 1;
            ValueTask<TState>[] valueTasks = ArrayPool<ValueTask<TState>>.Shared.Rent(length);
            Task<TState>[]? tasks = null;

            try
            {
#pragma warning disable CA2012 // Use ValueTasks correctly - we are deliberately storing this for a brief time for perf reasons
                valueTasks[0] = step(wrappedInput);
                if (valueTasks[0].IsCompleted)
                {
                    return valueTasks[0].Result;
                }

                for (int i = 1; i < length; ++i)
                {
                    valueTasks[i] = attempts[i - 1](wrappedInput);
                    if (valueTasks[i].IsCompleted)
                    {
                        return valueTasks[i].Result;
                    }
                }
#pragma warning restore CA2012 // Use ValueTasks correctly

                // We didn't get a synchronous run, so we wait for any task to complete.
                tasks = ArrayPool<Task<TState>>.Shared.Rent(length);
                for (int i = 0; i < length; ++i)
                {
                    tasks[i] = valueTasks[i].AsTask();
                }

                Task<TState> result = await Task.WhenAny(new ArraySegment<Task<TState>>(tasks, 0, length)).ConfigureAwait(false);
                return result.Result;
            }
            finally
            {
                ArrayPool<ValueTask<TState>>.Shared.Return(valueTasks);

                if (tasks is Task<TState>[] t)
                {
                    ArrayPool<Task<TState>>.Shared.Return(t);
                }

                cancellationTokenSource.Cancel();
            }
        };
    }

    /// <summary>
    /// An operator that combines two steps to produce a step which takes a tuple of the
    /// state types of the input steps, and processes each value in the tuple with the
    /// appropriate step, returning a tuple of the results.
    /// </summary>
    /// <typeparam name="TState1">The type of the state of the first step.</typeparam>
    /// <typeparam name="TState2">The type of the state of the second step.</typeparam>
    /// <param name="step1">The first input step.</param>
    /// <param name="step2">The second input step.</param>
    /// <returns>A <see cref="PipelineStep{Tuple}"/> which returns a tuple of the results of the input steps.</returns>
    /// <remarks>This executes the steps in parallel. See <see cref="CombineSteps{TState1, TState2}(PipelineStep{TState1}, PipelineStep{TState2})"/> for the sequential case.</remarks>
    public static PipelineStep<(TState1 State1, TState2 State2)> ParallelCombineSteps<TState1, TState2>(
        this PipelineStep<TState1> step1,
        PipelineStep<TState2> step2)
        where TState1 : struct
        where TState2 : struct
    {
        return async ((TState1 State1, TState2 State2) input) =>
        {
            ValueTask<TState1> task1 = step1(input.State1);
            ValueTask<TState2> task2 = step2(input.State2);

            return (await task1.ConfigureAwait(false), await task2.ConfigureAwait(false));
        };
    }

    /// <summary>
    /// An operator that combines three steps to produce a step which takes a tuple of the
    /// state types of the input steps, and processes each value in the tuple with the
    /// appropriate step, returning a tuple of the results.
    /// </summary>
    /// <typeparam name="TState1">The type of the state of the first step.</typeparam>
    /// <typeparam name="TState2">The type of the state of the second step.</typeparam>
    /// <typeparam name="TState3">The type of the state of the third step.</typeparam>
    /// <param name="step1">The first input step.</param>
    /// <param name="step2">The second input step.</param>
    /// <param name="step3">The third input step.</param>
    /// <returns>A <see cref="PipelineStep{Tuple}"/> which returns a tuple of the results of the input steps.</returns>
    /// <remarks>This executes the steps in parallel. See <see cref="CombineSteps{TState1, TState2, TState3}(PipelineStep{TState1}, PipelineStep{TState2}, PipelineStep{TState3})"/> for the sequential case.</remarks>
    public static PipelineStep<(TState1 State1, TState2 State2, TState3 State3)> ParallelCombineSteps<TState1, TState2, TState3>(
        this PipelineStep<TState1> step1,
        PipelineStep<TState2> step2,
        PipelineStep<TState3> step3)
        where TState1 : struct
        where TState2 : struct
        where TState3 : struct
    {
        return async ((TState1 State1, TState2 State2, TState3 State3) input) =>
        {
            ValueTask<TState1> task1 = step1(input.State1);
            ValueTask<TState2> task2 = step2(input.State2);
            ValueTask<TState3> task3 = step3(input.State3);

            return (await task1.ConfigureAwait(false), await task2.ConfigureAwait(false), await task3.ConfigureAwait(false));
        };
    }

    /// <summary>
    /// An operator that combines two steps to produce a step which takes a tuple of the
    /// state types of the input steps, and processes each value in the tuple with the
    /// appropriate step, returning a tuple of the results.
    /// </summary>
    /// <typeparam name="TState1">The type of the state of the first step.</typeparam>
    /// <typeparam name="TState2">The type of the state of the second step.</typeparam>
    /// <param name="step1">The first input step.</param>
    /// <param name="step2">The second input step.</param>
    /// <returns>A <see cref="PipelineStep{Tuple}"/> which returns a tuple of the results of the input steps.</returns>
    /// <remarks>
    /// This executes <paramref name="step1"/>, then executes <paramref name="step2"/>, then returns the result. Compare with
    /// <see cref="ParallelCombineSteps{TState1, TState2}(PipelineStep{TState1}, PipelineStep{TState2})"/> which executes the steps
    /// in parallel.
    /// </remarks>
    public static PipelineStep<(TState1 State1, TState2 State2)> CombineSteps<TState1, TState2>(
        this PipelineStep<TState1> step1,
        PipelineStep<TState2> step2)
        where TState1 : struct
        where TState2 : struct
    {
        return async ((TState1 State1, TState2 State2) input) =>
        {
            TState1 value1 = await step1(input.State1).ConfigureAwait(false);
            TState2 value2 = await step2(input.State2).ConfigureAwait(false);
            return (value1, value2);
        };
    }

    /// <summary>
    /// An operator that combines three steps to produce a step which takes a tuple of the
    /// state types of the input steps, and processes each value in the tuple with the
    /// appropriate step, returning a tuple of the results.
    /// </summary>
    /// <typeparam name="TState1">The type of the state of the first step.</typeparam>
    /// <typeparam name="TState2">The type of the state of the second step.</typeparam>
    /// <typeparam name="TState3">The type of the state of the third step.</typeparam>
    /// <param name="step1">The first input step.</param>
    /// <param name="step2">The second input step.</param>
    /// <param name="step3">The third input step.</param>
    /// <returns>A <see cref="PipelineStep{Tuple}"/> which returns a tuple of the results of the input steps.</returns>
    /// <remarks>
    /// This executes <paramref name="step1"/>, then executes <paramref name="step2"/>, then executes <paramref name="step3"/>,
    /// then returns the result. Compare with
    /// <see cref="ParallelCombineSteps{TState1, TState2, TState3}(PipelineStep{TState1}, PipelineStep{TState2}, PipelineStep{TState3})"/> which executes the steps
    /// in parallel.
    /// </remarks>
    public static PipelineStep<(TState1 State1, TState2 State2, TState3 State3)> CombineSteps<TState1, TState2, TState3>(
        this PipelineStep<TState1> step1,
        PipelineStep<TState2> step2,
        PipelineStep<TState3> step3)
        where TState1 : struct
        where TState2 : struct
        where TState3 : struct
    {
        return async ((TState1 State1, TState2 State2, TState3 State3) input) =>
        {
            TState1 value1 = await step1(input.State1).ConfigureAwait(false);
            TState2 value2 = await step2(input.State2).ConfigureAwait(false);
            TState3 value3 = await step3(input.State3).ConfigureAwait(false);
            return (value1, value2, value3);
        };
    }

    /// <summary>
    /// An operator that combines two steps to produce a step which takes a tuple of the
    /// state types of the input steps, and processes each value in the tuple with the
    /// appropriate step, returning a tuple of the results.
    /// </summary>
    /// <typeparam name="TState1">The type of the state of the first step.</typeparam>
    /// <typeparam name="TState2">The type of the state of the second step.</typeparam>
    /// <param name="step1">The first input step.</param>
    /// <param name="step2">The second input step.</param>
    /// <returns>A <see cref="SyncPipelineStep{Tuple}"/> which returns a tuple of the results of the input steps.</returns>
    public static SyncPipelineStep<(TState1 State1, TState2 State2)> CombineSteps<TState1, TState2>(
        this SyncPipelineStep<TState1> step1,
        SyncPipelineStep<TState2> step2)
        where TState1 : struct
        where TState2 : struct
    {
        return ((TState1 State1, TState2 State2) input) =>
        {
            TState1 value1 = step1(input.State1);
            TState2 value2 = step2(input.State2);
            return (value1, value2);
        };
    }

    /// <summary>
    /// An operator that combines three steps to produce a step which takes a tuple of the
    /// state types of the input steps, and processes each value in the tuple with the
    /// appropriate step, returning a tuple of the results.
    /// </summary>
    /// <typeparam name="TState1">The type of the state of the first step.</typeparam>
    /// <typeparam name="TState2">The type of the state of the second step.</typeparam>
    /// <typeparam name="TState3">The type of the state of the third step.</typeparam>
    /// <param name="step1">The first input step.</param>
    /// <param name="step2">The second input step.</param>
    /// <param name="step3">The third input step.</param>
    /// <returns>A <see cref="SyncPipelineStep{Tuple}"/> which returns a tuple of the results of the input steps.</returns>
    public static SyncPipelineStep<(TState1 State1, TState2 State2, TState3 State3)> CombineSteps<TState1, TState2, TState3>(
        this SyncPipelineStep<TState1> step1,
        SyncPipelineStep<TState2> step2,
        SyncPipelineStep<TState3> step3)
        where TState1 : struct
        where TState2 : struct
        where TState3 : struct
    {
        return ((TState1 State1, TState2 State2, TState3 State3) input) =>
        {
            TState1 value1 = step1(input.State1);
            TState2 value2 = step2(input.State2);
            TState3 value3 = step3(input.State3);
            return (value1, value2, value3);
        };
    }

    /// <summary>
    /// An operator that binds a <see cref="SyncPipelineStep{Tuple}"/> of a tuple of the
    /// <typeparamref name="TState"/>, and a <typeparamref name="TValue1"/>, producing
    /// a <see cref="SyncPipelineStep{TState}"/> that will execute the <paramref name="stepWith"/> step
    /// with a tuple of the current state and the value provided by executing the <paramref name="value1ProviderStep"/>
    /// step. The value accessor operates on the default value of <typeparamref name="TValue1"/>.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TValue1">The type of the value.</typeparam>
    /// <param name="stepWith">The step to bind.</param>
    /// <param name="value1ProviderStep">A <see cref="SyncPipelineStep{TValue1}"/> which provides the value to bind.</param>
    /// <returns>A <see cref="SyncPipelineStep{TState}"/> which executes the <paramref name="stepWith"/> with the state and the value,
    /// and returns the updated state.</returns>
    public static SyncPipelineStep<TState> BindWith<TState, TValue1>(
        this SyncPipelineStep<(TState State, TValue1 Value1)> stepWith,
        SyncPipelineStep<TValue1> value1ProviderStep)
        where TState : struct
        where TValue1 : struct
        => BindWith(stepWith, null, value1ProviderStep);

    /// <summary>
    /// An operator that binds a <see cref="SyncPipelineStep{Tuple}"/> of a tuple of the
    /// <typeparamref name="TState"/>, and a <typeparamref name="TValue1"/>, producing
    /// a <see cref="SyncPipelineStep{TState}"/> that will execute the <paramref name="stepWith"/> step
    /// with a tuple of the current state and the value provided by executing the <paramref name="value1ProviderStep"/>
    /// step. The value accessor operates on the value provided by the <paramref name="initialValue1FromState"/>
    /// function. This wraps the input state instance to return the appropriate input state for the accessor step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TValue1">The type of the value.</typeparam>
    /// <param name="stepWith">The step to bind.</param>
    /// <param name="initialValue1FromState">A function which returns the initial value for executing the
    /// <paramref name="value1ProviderStep"/>. It has access to the state to do this.</param>
    /// <param name="value1ProviderStep">A <see cref="SyncPipelineStep{TValue1}"/> which provides the value to bind.</param>
    /// <returns>A <see cref="SyncPipelineStep{TState}"/> which executes the <paramref name="stepWith"/> with the state and the value,
    /// and returns the updated state.</returns>
    public static SyncPipelineStep<TState> BindWith<TState, TValue1>(
        this SyncPipelineStep<(TState State, TValue1 Value1)> stepWith,
        Func<TState, TValue1>? initialValue1FromState,
        SyncPipelineStep<TValue1> value1ProviderStep)
        where TState : struct
        where TValue1 : struct
    {
        return stepWith.Bind(
            (TState state) => (state, value1ProviderStep(GetValueOrDefault(state, initialValue1FromState))),
            (TState _, (TState State, TValue1 Value1) result) => result.State);
    }

    /// <summary>
    /// An operator that binds a <see cref="SyncPipelineStep{Tuple}"/> of a tuple of the
    /// <typeparamref name="TState"/>, and additional values of type <typeparamref name="TValue1"/> and <typeparamref name="TValue1"/>, producing
    /// a <see cref="SyncPipelineStep{TState}"/> that will execute the <paramref name="stepWith"/> step
    /// with a tuple of the current state and the values provided by executing the <paramref name="value1ProviderStep"/>, and <paramref name="value2ProviderStep"/>
    /// steps. The value accessors operate on the default values for <typeparamref name="TValue1"/> and <typeparamref name="TValue1"/>.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TValue1">The type of the first value.</typeparam>
    /// <typeparam name="TValue2">The type of the second value.</typeparam>
    /// <param name="stepWith">The step to bind.</param>
    /// <param name="value1ProviderStep">A <see cref="SyncPipelineStep{TValue1}"/> which provides the first value to bind.</param>
    /// <param name="value2ProviderStep">A <see cref="SyncPipelineStep{TValue2}"/> which provides the second value to bind.</param>
    /// <returns>A <see cref="SyncPipelineStep{TState}"/> which executes the <paramref name="stepWith"/> with the state and the value,
    /// and returns the updated state.</returns>
    public static SyncPipelineStep<TState> BindWith<TState, TValue1, TValue2>(
        this SyncPipelineStep<(TState State, TValue1 Value1, TValue2 Value2)> stepWith,
        SyncPipelineStep<TValue1> value1ProviderStep,
        SyncPipelineStep<TValue2> value2ProviderStep)
        where TState : struct
        where TValue1 : struct
        where TValue2 : struct
        => BindWith(stepWith, null, value1ProviderStep, null, value2ProviderStep);

    /// <summary>
    /// An operator that binds a <see cref="SyncPipelineStep{Tuple}"/> of a tuple of the
    /// <typeparamref name="TState"/>, and additional values of type <typeparamref name="TValue1"/> and <typeparamref name="TValue1"/>, producing
    /// a <see cref="SyncPipelineStep{TState}"/> that will execute the <paramref name="stepWith"/> step
    /// with a tuple of the current state and the values provided by executing the <paramref name="value1ProviderStep"/>, and <paramref name="value2ProviderStep"/>
    /// steps. The value accessors operate on the values provided by the <paramref name="initialValue1FromState"/>, and <paramref name="initialValue2FromState"/>
    /// functions (respectively). These wrap the input state instance to return the appropriate input state for each accessor step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TValue1">The type of the first value.</typeparam>
    /// <typeparam name="TValue2">The type of the second value.</typeparam>
    /// <param name="stepWith">The step to bind.</param>
    /// <param name="initialValue1FromState">A function which returns the initial value for executing the
    /// <paramref name="value1ProviderStep"/>. It has access to the state to do this.</param>
    /// <param name="value1ProviderStep">A <see cref="SyncPipelineStep{TValue1}"/> which provides the first value to bind.</param>
    /// <param name="initialValue2FromState">A function which returns the initial value for executing the
    /// <paramref name="value2ProviderStep"/>. It has access to the state to do this.</param>
    /// <param name="value2ProviderStep">A <see cref="SyncPipelineStep{TValue2}"/> which provides the second value to bind.</param>
    /// <returns>A <see cref="SyncPipelineStep{TState}"/> which executes the <paramref name="stepWith"/> with the state and the value,
    /// and returns the updated state.</returns>
    public static SyncPipelineStep<TState> BindWith<TState, TValue1, TValue2>(
        this SyncPipelineStep<(TState State, TValue1 Value1, TValue2 Value2)> stepWith,
        Func<TState, TValue1>? initialValue1FromState,
        SyncPipelineStep<TValue1> value1ProviderStep,
        Func<TState, TValue2>? initialValue2FromState,
        SyncPipelineStep<TValue2> value2ProviderStep)
        where TState : struct
        where TValue1 : struct
        where TValue2 : struct
    {
        return stepWith.Bind(
            (TState state) =>
                (state,
                 value1ProviderStep(GetValueOrDefault(state, initialValue1FromState)),
                 value2ProviderStep(GetValueOrDefault(state, initialValue2FromState))),
            (TState _, (TState State, TValue1 Value1, TValue2 Value2) result) => result.State);
    }

    /// <summary>
    /// An operator that binds a <see cref="SyncPipelineStep{Tuple}"/> of a tuple of the
    /// <typeparamref name="TState"/>, and additional values of type <typeparamref name="TValue1"/>, <typeparamref name="TValue2"/>, and
    /// <typeparamref name="TValue3"/>, producing a <see cref="SyncPipelineStep{TState}"/> that will execute the <paramref name="stepWith"/> step
    /// with a tuple of the current state and the values provided by executing the <paramref name="value1ProviderStep"/>, <paramref name="value2ProviderStep"/>,
    /// and <paramref name="value3ProviderStep"/> steps. The value accessors operate on the default values of <typeparamref name="TValue1"/>, <typeparamref name="TValue2"/>, and
    /// <typeparamref name="TValue3"/>.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TValue1">The type of the first value.</typeparam>
    /// <typeparam name="TValue2">The type of the second value.</typeparam>
    /// <typeparam name="TValue3">The type of the third value.</typeparam>
    /// <param name="stepWith">The step to bind.</param>
    /// <param name="value1ProviderStep">A <see cref="SyncPipelineStep{TValue1}"/> which provides the first value to bind.</param>
    /// <param name="value2ProviderStep">A <see cref="SyncPipelineStep{TValue2}"/> which provides the second value to bind.</param>
    /// <param name="value3ProviderStep">A <see cref="SyncPipelineStep{TValue3}"/> which provides third second value to bind.</param>
    /// <returns>A <see cref="SyncPipelineStep{TState}"/> which executes the <paramref name="stepWith"/> with the state and the value,
    /// and returns the updated state.</returns>
    public static SyncPipelineStep<TState> BindWith<TState, TValue1, TValue2, TValue3>(
        this SyncPipelineStep<(TState State, TValue1 Value1, TValue2 Value2, TValue3 Value3)> stepWith,
        SyncPipelineStep<TValue1> value1ProviderStep,
        SyncPipelineStep<TValue2> value2ProviderStep,
        SyncPipelineStep<TValue3> value3ProviderStep)
        where TState : struct
        where TValue1 : struct
        where TValue2 : struct
        where TValue3 : struct
    => BindWith(stepWith, null, value1ProviderStep, null, value2ProviderStep, null, value3ProviderStep);

    /// <summary>
    /// An operator that binds a <see cref="SyncPipelineStep{Tuple}"/> of a tuple of the
    /// <typeparamref name="TState"/>, and additional values of type <typeparamref name="TValue1"/>, <typeparamref name="TValue2"/>, and
    /// <typeparamref name="TValue3"/>, producing a <see cref="SyncPipelineStep{TState}"/> that will execute the <paramref name="stepWith"/> step
    /// with a tuple of the current state and the values provided by executing the <paramref name="value1ProviderStep"/>, <paramref name="value2ProviderStep"/>,
    /// and <paramref name="value3ProviderStep"/> steps. The value accessors operate on values provided by the <paramref name="initialValue1FromState"/>,
    /// <paramref name="initialValue2FromState"/>, and <paramref name="initialValue3FromState"/> functions (respectively). These wrap the input state to
    /// return the appropriate input state for each value accessor step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TValue1">The type of the first value.</typeparam>
    /// <typeparam name="TValue2">The type of the second value.</typeparam>
    /// <typeparam name="TValue3">The type of the third value.</typeparam>
    /// <param name="stepWith">The step to bind.</param>
    /// <param name="initialValue1FromState">A function which returns the initial value for executing the
    /// <paramref name="value1ProviderStep"/>. It has access to the state to do this.</param>
    /// <param name="value1ProviderStep">A <see cref="SyncPipelineStep{TValue1}"/> which provides the first value to bind.</param>
    /// <param name="initialValue2FromState">A function which returns the initial value for executing the
    /// <paramref name="value2ProviderStep"/>. It has access to the state to do this.</param>
    /// <param name="value2ProviderStep">A <see cref="SyncPipelineStep{TValue2}"/> which provides the second value to bind.</param>
    /// <param name="initialValue3FromState">A function which returns the initial value for executing the
    /// <paramref name="value3ProviderStep"/>. It has access to the state to do this.</param>
    /// <param name="value3ProviderStep">A <see cref="SyncPipelineStep{TValue3}"/> which provides third second value to bind.</param>
    /// <returns>A <see cref="SyncPipelineStep{TState}"/> which executes the <paramref name="stepWith"/> with the state and the value,
    /// and returns the updated state.</returns>
    public static SyncPipelineStep<TState> BindWith<TState, TValue1, TValue2, TValue3>(
        this SyncPipelineStep<(TState State, TValue1 Value1, TValue2 Value2, TValue3 Value3)> stepWith,
        Func<TState, TValue1>? initialValue1FromState,
        SyncPipelineStep<TValue1> value1ProviderStep,
        Func<TState, TValue2>? initialValue2FromState,
        SyncPipelineStep<TValue2> value2ProviderStep,
        Func<TState, TValue3>? initialValue3FromState,
        SyncPipelineStep<TValue3> value3ProviderStep)
        where TState : struct
        where TValue1 : struct
        where TValue2 : struct
        where TValue3 : struct
    {
        return stepWith.Bind(
            (TState state) =>
                (state,
                 value1ProviderStep(GetValueOrDefault(state, initialValue1FromState)),
                 value2ProviderStep(GetValueOrDefault(state, initialValue2FromState)),
                 value3ProviderStep(GetValueOrDefault(state, initialValue3FromState))),
            (TState _, (TState State, TValue1 Value1, TValue2 Value2, TValue3 Value3) result) => result.State);
    }

    /// <summary>
    /// An operator that binds a <see cref="PipelineStep{Tuple}"/> of a tuple of the
    /// <typeparamref name="TState"/>, and a <typeparamref name="TValue1"/>, producing
    /// a <see cref="PipelineStep{TState}"/> that will execute the <paramref name="stepWith"/> step
    /// with a tuple of the current state and the value provided by executing the <paramref name="value1ProviderStep"/>
    /// step. The value accessor operates on the default value of <typeparamref name="TValue1"/>.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TValue1">The type of the value.</typeparam>
    /// <param name="stepWith">The step to bind.</param>
    /// <param name="value1ProviderStep">A <see cref="PipelineStep{TValue1}"/> which provides the value to bind.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which executes the <paramref name="stepWith"/> with the state and the value,
    /// and returns the updated state.</returns>
    public static PipelineStep<TState> BindWith<TState, TValue1>(
        this PipelineStep<(TState State, TValue1 Value1)> stepWith,
        PipelineStep<TValue1> value1ProviderStep)
        where TState : struct
        where TValue1 : struct
        => BindWith(stepWith, null, value1ProviderStep);

    /// <summary>
    /// An operator that binds a <see cref="PipelineStep{Tuple}"/> of a tuple of the
    /// <typeparamref name="TState"/>, and a <typeparamref name="TValue1"/>, producing
    /// a <see cref="PipelineStep{TState}"/> that will execute the <paramref name="stepWith"/> step
    /// with a tuple of the current state and the value provided by executing the <paramref name="value1ProviderStep"/>
    /// step. The value accessor operates on the value provided by the <paramref name="initialValue1FromState"/>
    /// function. This wraps the input state instance to return the appropriate input state for the accessor step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TValue1">The type of the value.</typeparam>
    /// <param name="stepWith">The step to bind.</param>
    /// <param name="initialValue1FromState">A function which returns the initial value for executing the
    /// <paramref name="value1ProviderStep"/>. It has access to the state to do this.</param>
    /// <param name="value1ProviderStep">A <see cref="PipelineStep{TValue1}"/> which provides the value to bind.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which executes the <paramref name="stepWith"/> with the state and the value,
    /// and returns the updated state.</returns>
    public static PipelineStep<TState> BindWith<TState, TValue1>(
        this PipelineStep<(TState State, TValue1 Value1)> stepWith,
        Func<TState, TValue1>? initialValue1FromState,
        PipelineStep<TValue1> value1ProviderStep)
        where TState : struct
        where TValue1 : struct
    {
        return stepWith.Bind(
            async (TState state) => (state, await value1ProviderStep(GetValueOrDefault(state, initialValue1FromState)).ConfigureAwait(false)),
            (TState _, (TState State, TValue1 Value1) result) => result.State);
    }

    /// <summary>
    /// An operator that binds a <see cref="PipelineStep{Tuple}"/> of a tuple of the
    /// <typeparamref name="TState"/>, and additional values of type <typeparamref name="TValue1"/> and <typeparamref name="TValue1"/>, producing
    /// a <see cref="PipelineStep{TState}"/> that will execute the <paramref name="stepWith"/> step
    /// with a tuple of the current state and the values provided by executing the <paramref name="value1ProviderStep"/>, and <paramref name="value2ProviderStep"/>
    /// steps. The value accessors operate on the default values for <typeparamref name="TValue1"/> and <typeparamref name="TValue1"/>.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TValue1">The type of the first value.</typeparam>
    /// <typeparam name="TValue2">The type of the second value.</typeparam>
    /// <param name="stepWith">The step to bind.</param>
    /// <param name="value1ProviderStep">A <see cref="PipelineStep{TValue1}"/> which provides the first value to bind.</param>
    /// <param name="value2ProviderStep">A <see cref="PipelineStep{TValue2}"/> which provides the second value to bind.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which executes the <paramref name="stepWith"/> with the state and the value,
    /// and returns the updated state.</returns>
    public static PipelineStep<TState> BindWith<TState, TValue1, TValue2>(
        this PipelineStep<(TState State, TValue1 Value1, TValue2 Value2)> stepWith,
        PipelineStep<TValue1> value1ProviderStep,
        PipelineStep<TValue2> value2ProviderStep)
        where TState : struct
        where TValue1 : struct
        where TValue2 : struct
        => BindWith(stepWith, null, value1ProviderStep, null, value2ProviderStep);

    /// <summary>
    /// An operator that binds a <see cref="PipelineStep{Tuple}"/> of a tuple of the
    /// <typeparamref name="TState"/>, and additional values of type <typeparamref name="TValue1"/> and <typeparamref name="TValue1"/>, producing
    /// a <see cref="PipelineStep{TState}"/> that will execute the <paramref name="stepWith"/> step
    /// with a tuple of the current state and the values provided by executing the <paramref name="value1ProviderStep"/>, and <paramref name="value2ProviderStep"/>
    /// steps. The value accessors operate on the values provided by the <paramref name="initialValue1FromState"/>, and <paramref name="initialValue2FromState"/>
    /// functions (respectively). These wrap the input state instance to return the appropriate input state for each accessor step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TValue1">The type of the first value.</typeparam>
    /// <typeparam name="TValue2">The type of the second value.</typeparam>
    /// <param name="stepWith">The step to bind.</param>
    /// <param name="initialValue1FromState">A function which returns the initial value for executing the
    /// <paramref name="value1ProviderStep"/>. It has access to the state to do this.</param>
    /// <param name="value1ProviderStep">A <see cref="PipelineStep{TValue1}"/> which provides the first value to bind.</param>
    /// <param name="initialValue2FromState">A function which returns the initial value for executing the
    /// <paramref name="value2ProviderStep"/>. It has access to the state to do this.</param>
    /// <param name="value2ProviderStep">A <see cref="PipelineStep{TValue2}"/> which provides the second value to bind.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which executes the <paramref name="stepWith"/> with the state and the value,
    /// and returns the updated state.</returns>
    public static PipelineStep<TState> BindWith<TState, TValue1, TValue2>(
        this PipelineStep<(TState State, TValue1 Value1, TValue2 Value2)> stepWith,
        Func<TState, TValue1>? initialValue1FromState,
        PipelineStep<TValue1> value1ProviderStep,
        Func<TState, TValue2>? initialValue2FromState,
        PipelineStep<TValue2> value2ProviderStep)
        where TState : struct
        where TValue1 : struct
        where TValue2 : struct
    {
        return stepWith.Bind(
            async (TState state) =>
                (state,
                 await value1ProviderStep(GetValueOrDefault(state, initialValue1FromState)).ConfigureAwait(false),
                 await value2ProviderStep(GetValueOrDefault(state, initialValue2FromState)).ConfigureAwait(false)),
            (TState _, (TState State, TValue1 Value1, TValue2 Value2) result) => result.State);
    }

    /// <summary>
    /// An operator that binds a <see cref="PipelineStep{Tuple}"/> of a tuple of the
    /// <typeparamref name="TState"/>, and additional values of type <typeparamref name="TValue1"/>, <typeparamref name="TValue2"/>, and
    /// <typeparamref name="TValue3"/>, producing a <see cref="PipelineStep{TState}"/> that will execute the <paramref name="stepWith"/> step
    /// with a tuple of the current state and the values provided by executing the <paramref name="value1ProviderStep"/>, <paramref name="value2ProviderStep"/>,
    /// and <paramref name="value3ProviderStep"/> steps. The value accessors operate on the default values of <typeparamref name="TValue1"/>, <typeparamref name="TValue2"/>, and
    /// <typeparamref name="TValue3"/>.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TValue1">The type of the first value.</typeparam>
    /// <typeparam name="TValue2">The type of the second value.</typeparam>
    /// <typeparam name="TValue3">The type of the third value.</typeparam>
    /// <param name="stepWith">The step to bind.</param>
    /// <param name="value1ProviderStep">A <see cref="PipelineStep{TValue1}"/> which provides the first value to bind.</param>
    /// <param name="value2ProviderStep">A <see cref="PipelineStep{TValue2}"/> which provides the second value to bind.</param>
    /// <param name="value3ProviderStep">A <see cref="PipelineStep{TValue3}"/> which provides third second value to bind.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which executes the <paramref name="stepWith"/> with the state and the value,
    /// and returns the updated state.</returns>
    public static PipelineStep<TState> BindWith<TState, TValue1, TValue2, TValue3>(
        this PipelineStep<(TState State, TValue1 Value1, TValue2 Value2, TValue3 Value3)> stepWith,
        PipelineStep<TValue1> value1ProviderStep,
        PipelineStep<TValue2> value2ProviderStep,
        PipelineStep<TValue3> value3ProviderStep)
        where TState : struct
        where TValue1 : struct
        where TValue2 : struct
        where TValue3 : struct
    => BindWith(stepWith, null, value1ProviderStep, null, value2ProviderStep, null, value3ProviderStep);

    /// <summary>
    /// An operator that binds a <see cref="PipelineStep{Tuple}"/> of a tuple of the
    /// <typeparamref name="TState"/>, and additional values of type <typeparamref name="TValue1"/>, <typeparamref name="TValue2"/>, and
    /// <typeparamref name="TValue3"/>, producing a <see cref="PipelineStep{TState}"/> that will execute the <paramref name="stepWith"/> step
    /// with a tuple of the current state and the values provided by executing the <paramref name="value1ProviderStep"/>, <paramref name="value2ProviderStep"/>,
    /// and <paramref name="value3ProviderStep"/> steps. The value accessors operate on values provided by the <paramref name="initialValue1FromState"/>,
    /// <paramref name="initialValue2FromState"/>, and <paramref name="initialValue3FromState"/> functions (respectively). These wrap the input state to
    /// return the appropriate input state for each value accessor step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TValue1">The type of the first value.</typeparam>
    /// <typeparam name="TValue2">The type of the second value.</typeparam>
    /// <typeparam name="TValue3">The type of the third value.</typeparam>
    /// <param name="stepWith">The step to bind.</param>
    /// <param name="initialValue1FromState">A function which returns the initial value for executing the
    /// <paramref name="value1ProviderStep"/>. It has access to the state to do this.</param>
    /// <param name="value1ProviderStep">A <see cref="PipelineStep{TValue1}"/> which provides the first value to bind.</param>
    /// <param name="initialValue2FromState">A function which returns the initial value for executing the
    /// <paramref name="value2ProviderStep"/>. It has access to the state to do this.</param>
    /// <param name="value2ProviderStep">A <see cref="PipelineStep{TValue2}"/> which provides the second value to bind.</param>
    /// <param name="initialValue3FromState">A function which returns the initial value for executing the
    /// <paramref name="value3ProviderStep"/>. It has access to the state to do this.</param>
    /// <param name="value3ProviderStep">A <see cref="PipelineStep{TValue3}"/> which provides third second value to bind.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which executes the <paramref name="stepWith"/> with the state and the value,
    /// and returns the updated state.</returns>
    public static PipelineStep<TState> BindWith<TState, TValue1, TValue2, TValue3>(
        this PipelineStep<(TState State, TValue1 Value1, TValue2 Value2, TValue3 Value3)> stepWith,
        Func<TState, TValue1>? initialValue1FromState,
        PipelineStep<TValue1> value1ProviderStep,
        Func<TState, TValue2>? initialValue2FromState,
        PipelineStep<TValue2> value2ProviderStep,
        Func<TState, TValue3>? initialValue3FromState,
        PipelineStep<TValue3> value3ProviderStep)
        where TState : struct
        where TValue1 : struct
        where TValue2 : struct
        where TValue3 : struct
    {
        return stepWith.Bind(
            async (TState state) =>
                (state,
                 await value1ProviderStep(GetValueOrDefault(state, initialValue1FromState)).ConfigureAwait(false),
                 await value2ProviderStep(GetValueOrDefault(state, initialValue2FromState)).ConfigureAwait(false),
                 await value3ProviderStep(GetValueOrDefault(state, initialValue3FromState)).ConfigureAwait(false)),
            (TState _, (TState State, TValue1 Value1, TValue2 Value2, TValue3 Value3) result) => result.State);
    }

    /// <summary>
    /// An operator that binds a <see cref="PipelineStep{TInnerState}"/> to a <see cref="PipelineStep{TState}"/>
    /// by returning a <see cref="PipelineStep{TState}"/> that maps the input instance of <typeparamref name="TState"/>
    /// to an instance of the required <typeparamref name="TInnerState"/>, executing the <paramref name="step"/>,
    /// and unwrapping the result to produce an instance of <typeparamref name="TState"/>.
    /// </summary>
    /// <typeparam name="TInnerState">The type of the state of the inner <paramref name="step"/> to execute.</typeparam>
    /// <typeparam name="TState">The type of the input state.</typeparam>
    /// <param name="step">The <see cref="PipelineStep{TInnerState}"/> to execute.</param>
    /// <param name="wrap">A function that maps from an instance of the <typeparamref name="TState"/> to an instance of
    /// the <typeparamref name="TInnerState"/>.</param>
    /// <param name="unwrap">A function that maps from an instance of the <typeparamref name="TState"/> to an instance of
    /// the <typeparamref name="TInnerState"/>. It is also provided with the original <typeparamref name="TState"/> instance.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which wraps the state, calls the <paramref name="step"/> with the wrapped
    /// value, and unwraps the resulting state value.</returns>
    public static PipelineStep<TState> Bind<TInnerState, TState>(
        this PipelineStep<TInnerState> step,
        Func<TState, TInnerState> wrap,
        Func<TState, TInnerState, TState> unwrap)
        where TInnerState : struct
        where TState : struct
    {
        return async state =>
        {
            TInnerState internalState = await step(wrap(state)).ConfigureAwait(false);
            return unwrap(state, internalState);
        };
    }

    /// <summary>
    /// An operator that binds a <see cref="PipelineStep{TInnerState}"/> to a <see cref="PipelineStep{TState}"/>
    /// by returning a <see cref="PipelineStep{TState}"/> that maps the input instance of <typeparamref name="TState"/>
    /// to an instance of the required <typeparamref name="TInnerState"/>, executing the <paramref name="step"/>,
    /// and unwrapping the result to produce an instance of <typeparamref name="TState"/>.
    /// </summary>
    /// <typeparam name="TInnerState">The type of the state of the <paramref name="step"/> to execute.</typeparam>
    /// <typeparam name="TState">The type of the input state.</typeparam>
    /// <param name="step">The <see cref="PipelineStep{TInnerState}"/> to execute.</param>
    /// <param name="wrap">A function that maps from an instance of the <typeparamref name="TState"/> to an instance of
    /// the <typeparamref name="TInnerState"/>.</param>
    /// <param name="unwrap">A function that maps from an instance of the <typeparamref name="TState"/> to an instance of
    /// the <typeparamref name="TInnerState"/>. It is also provided with the original <typeparamref name="TState"/> instance.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which wraps the state, calls the <paramref name="step"/> with the wrapped
    /// value, and unwraps the resulting state value.</returns>
    public static PipelineStep<TState> Bind<TInnerState, TState>(
        this PipelineStep<TInnerState> step,
        Func<TState, ValueTask<TInnerState>> wrap,
        Func<TState, TInnerState, TState> unwrap)
        where TInnerState : struct
        where TState : struct
    {
        return async state =>
        {
            TInnerState internalState = await step(await wrap(state).ConfigureAwait(false)).ConfigureAwait(false);
            return unwrap(state, internalState);
        };
    }

    /// <summary>
    /// An operator that binds a <see cref="PipelineStep{TInnerState}"/> to a <see cref="PipelineStep{TState}"/>
    /// by returning a <see cref="PipelineStep{TState}"/> that maps the input instance of <typeparamref name="TState"/>
    /// to an instance of the required <typeparamref name="TInnerState"/>, executing the <paramref name="step"/>,
    /// and unwrapping the result to produce an instance of <typeparamref name="TState"/>.
    /// </summary>
    /// <typeparam name="TInnerState">The type of the state of the <paramref name="step"/> to execute.</typeparam>
    /// <typeparam name="TState">The type of the input state.</typeparam>
    /// <param name="step">The <see cref="PipelineStep{TInnerState}"/> to execute.</param>
    /// <param name="wrap">A function that maps from an instance of the <typeparamref name="TState"/> to an instance of
    /// the <typeparamref name="TInnerState"/>.</param>
    /// <param name="unwrap">A function that maps from an instance of the <typeparamref name="TState"/> to an instance of
    /// the <typeparamref name="TInnerState"/>. It is also provided with the original <typeparamref name="TState"/> instance.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which wraps the state, calls the <paramref name="step"/> with the wrapped
    /// value, and unwraps the resulting state value.</returns>
    public static PipelineStep<TState> Bind<TInnerState, TState>(
        this PipelineStep<TInnerState> step,
        Func<TState, ValueTask<TInnerState>> wrap,
        Func<TState, TInnerState, ValueTask<TState>> unwrap)
        where TInnerState : struct
        where TState : struct
    {
        return async state =>
        {
            TInnerState internalState = await step(await wrap(state).ConfigureAwait(false)).ConfigureAwait(false);
            return await unwrap(state, internalState).ConfigureAwait(false);
        };
    }

    /// <summary>
    /// An operator that binds a <see cref="PipelineStep{TInnerState}"/> to a <see cref="PipelineStep{TState}"/>
    /// by returning a <see cref="PipelineStep{TState}"/> that maps the input instance of <typeparamref name="TState"/>
    /// to an instance of the required <typeparamref name="TInnerState"/>, executing the <paramref name="step"/>,
    /// and unwrapping the result to produce an instance of <typeparamref name="TState"/>.
    /// </summary>
    /// <typeparam name="TInnerState">The type of the state of the <paramref name="step"/> to execute.</typeparam>
    /// <typeparam name="TState">The type of the input state.</typeparam>
    /// <param name="step">The <see cref="PipelineStep{TInnerState}"/> to execute.</param>
    /// <param name="wrap">A function that maps from an instance of the <typeparamref name="TState"/> to an instance of
    /// the <typeparamref name="TInnerState"/>.</param>
    /// <param name="unwrap">A function that maps from an instance of the <typeparamref name="TState"/> to an instance of
    /// the <typeparamref name="TInnerState"/>. It is also provided with the original <typeparamref name="TState"/> instance.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which wraps the state, calls the <paramref name="step"/> with the wrapped
    /// value, and unwraps the resulting state value.</returns>
    public static PipelineStep<TState> Bind<TInnerState, TState>(
        this PipelineStep<TInnerState> step,
        Func<TState, TInnerState> wrap,
        Func<TState, TInnerState, ValueTask<TState>> unwrap)
        where TInnerState : struct
        where TState : struct
    {
        return async state =>
        {
            TInnerState internalState = await step(wrap(state)).ConfigureAwait(false);
            return await unwrap(state, internalState).ConfigureAwait(false);
        };
    }

    /// <summary>
    /// An operator which binds the output of one step to the input of another.
    /// </summary>
    /// <typeparam name="TState">The type of the state of the pipeline step.</typeparam>
    /// <param name="step">The step whose output is provided to the binding step.</param>
    /// <param name="binding">The step whose input is provided by the binding.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which executes <paramref name="step"/>, and
    /// provides its output as the input of the <paramref name="binding"/> step, returning
    /// the resulting state.</returns>
    /// <remarks>This is equivalent to <see cref="Pipeline.Build{TState}(PipelineStep{TState}[])"/> passing the
    /// two steps in order.</remarks>
    public static PipelineStep<TState> Bind<TState>(
        this PipelineStep<TState> step,
        PipelineStep<TState> binding)
        where TState : struct
    {
        return async state =>
        {
            TState result = await step(state).ConfigureAwait(false);
            return await binding(result).ConfigureAwait(false);
        };
    }

    /// <summary>
    /// An operator which binds the output of one step to the input of another.
    /// </summary>
    /// <typeparam name="TState">The type of the state of the pipeline step.</typeparam>
    /// <param name="step">The step whose output is provided to the binding step.</param>
    /// <param name="binding">The step whose input is provided by the binding.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which executes <paramref name="step"/>, and
    /// provides its output as the input of the <paramref name="binding"/> step, returning
    /// the resulting state.</returns>
    /// <remarks>This is equivalent to <see cref="Pipeline.Build{TState}(PipelineStep{TState}[])"/> passing the
    /// two steps in order.</remarks>
    public static SyncPipelineStep<TState> Bind<TState>(
        this SyncPipelineStep<TState> step,
        SyncPipelineStep<TState> binding)
        where TState : struct
    {
        return state =>
        {
            TState result = step(state);
            return binding(result);
        };
    }

    /// <summary>
    /// An operator that binds a <see cref="PipelineStep{TInnerState}"/> to a <see cref="PipelineStep{TState}"/>
    /// by returning a <see cref="PipelineStep{TState}"/> that maps the input instance of <typeparamref name="TState"/>
    /// to an instance of the required <typeparamref name="TInnerState"/>, executing the <paramref name="step"/>,
    /// and unwrapping the result to produce an instance of <typeparamref name="TState"/>.
    /// </summary>
    /// <typeparam name="TInnerState">The type of the state of the <paramref name="step"/> to execute.</typeparam>
    /// <typeparam name="TState">The type of the input state.</typeparam>
    /// <param name="step">The <see cref="PipelineStep{TInnerState}"/> to execute.</param>
    /// <param name="wrap">A function that maps from an instance of the <typeparamref name="TState"/> to an instance of
    /// the <typeparamref name="TInnerState"/>.</param>
    /// <param name="unwrap">A function that maps from an instance of the <typeparamref name="TState"/> to an instance of
    /// the <typeparamref name="TInnerState"/>. It is also provided with the original <typeparamref name="TState"/> instance.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which wraps the state, calls the <paramref name="step"/> with the wrapped
    /// value, and unwraps the resulting state value.</returns>
    public static SyncPipelineStep<TState> Bind<TInnerState, TState>(
        this SyncPipelineStep<TInnerState> step,
        Func<TState, TInnerState> wrap,
        Func<TState, TInnerState, TState> unwrap)
        where TInnerState : struct
        where TState : struct
    {
        return state =>
        {
            TInnerState internalState = step(wrap(state));
            return unwrap(state, internalState);
        };
    }

    /// <summary>
    /// Bind the given services to the function to produce a pipeline step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TService1">The type of the service to bind.</typeparam>
    /// <param name="step">The step to bind.</param>
    /// <param name="service1">The service instance to bind.</param>
    /// <returns>An instance of a pipeline step with the services bound to it.</returns>
    public static SyncPipelineStep<TState> BindServices<TState, TService1>(this Func<TState, TService1, TState> step, TService1 service1)
        where TState : struct
        where TService1 : notnull
    {
        return state => step(state, service1);
    }

    /// <summary>
    /// Bind the given services to the function to produce a pipeline step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TService1">The type of the service to bind.</typeparam>
    /// <typeparam name="TService2">The type of the second service to bind.</typeparam>
    /// <param name="step">The step to bind.</param>
    /// <param name="service1">The service instance to bind.</param>
    /// <param name="service2">The second service instance to bind.</param>
    /// <returns>An instance of a pipeline step with the services bound to it.</returns>
    public static SyncPipelineStep<TState> BindServices<TState, TService1, TService2>(this Func<TState, TService1, TService2, TState> step, TService1 service1, TService2 service2)
        where TState : struct
        where TService1 : notnull
        where TService2 : notnull
    {
        return state => step(state, service1, service2);
    }

    /// <summary>
    /// Bind the given services to the function to produce a pipeline step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TService1">The type of the service to bind.</typeparam>
    /// <typeparam name="TService2">The type of the second service to bind.</typeparam>
    /// <typeparam name="TService3">The type of the third service to bind.</typeparam>
    /// <param name="step">The step to bind.</param>
    /// <param name="service1">The service instance to bind.</param>
    /// <param name="service2">The second service instance to bind.</param>
    /// <param name="service3">The third service instance to bind.</param>
    /// <returns>An instance of a pipeline step with the services bound to it.</returns>
    public static SyncPipelineStep<TState> BindServices<TState, TService1, TService2, TService3>(this Func<TState, TService1, TService2, TService3, TState> step, TService1 service1, TService2 service2, TService3 service3)
        where TState : struct
        where TService1 : notnull
        where TService2 : notnull
        where TService3 : notnull
    {
        return state => step(state, service1, service2, service3);
    }

    /// <summary>
    /// Bind the given services to the function to produce a pipeline step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TService1">The type of the service to bind.</typeparam>
    /// <typeparam name="TService2">The type of the second service to bind.</typeparam>
    /// <typeparam name="TService3">The type of the third service to bind.</typeparam>
    /// <typeparam name="TService4">The type of the fourth service to bind.</typeparam>
    /// <param name="step">The step to bind.</param>
    /// <param name="service1">The service instance to bind.</param>
    /// <param name="service2">The second service instance to bind.</param>
    /// <param name="service3">The third service instance to bind.</param>
    /// <param name="service4">The fourth service instance to bind.</param>
    /// <returns>An instance of a pipeline step with the services bound to it.</returns>
    public static SyncPipelineStep<TState> BindServices<TState, TService1, TService2, TService3, TService4>(this Func<TState, TService1, TService2, TService3, TService4, TState> step, TService1 service1, TService2 service2, TService3 service3, TService4 service4)
        where TState : struct
        where TService1 : notnull
        where TService2 : notnull
        where TService3 : notnull
        where TService4 : notnull
    {
        return state => step(state, service1, service2, service3, service4);
    }

    /// <summary>
    /// Bind the given services to the function to produce a pipeline step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TService1">The type of the service to bind.</typeparam>
    /// <typeparam name="TService2">The type of the second service to bind.</typeparam>
    /// <typeparam name="TService3">The type of the third service to bind.</typeparam>
    /// <typeparam name="TService4">The type of the fourth service to bind.</typeparam>
    /// <typeparam name="TService5">The type of the fifth service to bind.</typeparam>
    /// <param name="step">The step to bind.</param>
    /// <param name="service1">The service instance to bind.</param>
    /// <param name="service2">The second service instance to bind.</param>
    /// <param name="service3">The third service instance to bind.</param>
    /// <param name="service4">The fourth service instance to bind.</param>
    /// <param name="service5">The fifth service instance to bind.</param>
    /// <returns>An instance of a pipeline step with the services bound to it.</returns>
    public static SyncPipelineStep<TState> BindServices<TState, TService1, TService2, TService3, TService4, TService5>(this Func<TState, TService1, TService2, TService3, TService4, TService5, TState> step, TService1 service1, TService2 service2, TService3 service3, TService4 service4, TService5 service5)
        where TState : struct
        where TService1 : notnull
        where TService2 : notnull
        where TService3 : notnull
        where TService4 : notnull
        where TService5 : notnull
    {
        return state => step(state, service1, service2, service3, service4, service5);
    }

    /// <summary>
    /// Bind the given services to the function to produce a pipeline step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TService1">The type of the service to bind.</typeparam>
    /// <param name="step">The step to bind.</param>
    /// <param name="service1">The service instance to bind.</param>
    /// <returns>An instance of a pipeline step with the services bound to it.</returns>
    public static PipelineStep<TState> BindServices<TState, TService1>(this Func<TState, TService1, ValueTask<TState>> step, TService1 service1)
        where TState : struct
        where TService1 : notnull
    {
        return state => step(state, service1);
    }

    /// <summary>
    /// Bind the given services to the function to produce a pipeline step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TService1">The type of the service to bind.</typeparam>
    /// <typeparam name="TService2">The type of the second service to bind.</typeparam>
    /// <param name="step">The step to bind.</param>
    /// <param name="service1">The service instance to bind.</param>
    /// <param name="service2">The second service instance to bind.</param>
    /// <returns>An instance of a pipeline step with the services bound to it.</returns>
    public static PipelineStep<TState> BindServices<TState, TService1, TService2>(this Func<TState, TService1, TService2, ValueTask<TState>> step, TService1 service1, TService2 service2)
        where TState : struct
        where TService1 : notnull
        where TService2 : notnull
    {
        return state => step(state, service1, service2);
    }

    /// <summary>
    /// Bind the given services to the function to produce a pipeline step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TService1">The type of the service to bind.</typeparam>
    /// <typeparam name="TService2">The type of the second service to bind.</typeparam>
    /// <typeparam name="TService3">The type of the third service to bind.</typeparam>
    /// <param name="step">The step to bind.</param>
    /// <param name="service1">The service instance to bind.</param>
    /// <param name="service2">The second service instance to bind.</param>
    /// <param name="service3">The third service instance to bind.</param>
    /// <returns>An instance of a pipeline step with the services bound to it.</returns>
    public static PipelineStep<TState> BindServices<TState, TService1, TService2, TService3>(this Func<TState, TService1, TService2, TService3, ValueTask<TState>> step, TService1 service1, TService2 service2, TService3 service3)
        where TState : struct
        where TService1 : notnull
        where TService2 : notnull
        where TService3 : notnull
    {
        return state => step(state, service1, service2, service3);
    }

    /// <summary>
    /// Bind the given services to the function to produce a pipeline step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TService1">The type of the service to bind.</typeparam>
    /// <typeparam name="TService2">The type of the second service to bind.</typeparam>
    /// <typeparam name="TService3">The type of the third service to bind.</typeparam>
    /// <typeparam name="TService4">The type of the fourth service to bind.</typeparam>
    /// <param name="step">The step to bind.</param>
    /// <param name="service1">The service instance to bind.</param>
    /// <param name="service2">The second service instance to bind.</param>
    /// <param name="service3">The third service instance to bind.</param>
    /// <param name="service4">The fourth service instance to bind.</param>
    /// <returns>An instance of a pipeline step with the services bound to it.</returns>
    public static PipelineStep<TState> BindServices<TState, TService1, TService2, TService3, TService4>(this Func<TState, TService1, TService2, TService3, TService4, ValueTask<TState>> step, TService1 service1, TService2 service2, TService3 service3, TService4 service4)
        where TState : struct
        where TService1 : notnull
        where TService2 : notnull
        where TService3 : notnull
        where TService4 : notnull
    {
        return state => step(state, service1, service2, service3, service4);
    }

    /// <summary>
    /// Bind the given services to the function to produce a pipeline step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TService1">The type of the service to bind.</typeparam>
    /// <typeparam name="TService2">The type of the second service to bind.</typeparam>
    /// <typeparam name="TService3">The type of the third service to bind.</typeparam>
    /// <typeparam name="TService4">The type of the fourth service to bind.</typeparam>
    /// <typeparam name="TService5">The type of the fifth service to bind.</typeparam>
    /// <param name="step">The step to bind.</param>
    /// <param name="service1">The service instance to bind.</param>
    /// <param name="service2">The second service instance to bind.</param>
    /// <param name="service3">The third service instance to bind.</param>
    /// <param name="service4">The fourth service instance to bind.</param>
    /// <param name="service5">The fifth service instance to bind.</param>
    /// <returns>An instance of a pipeline step with the services bound to it.</returns>
    public static PipelineStep<TState> BindServices<TState, TService1, TService2, TService3, TService4, TService5>(this Func<TState, TService1, TService2, TService3, TService4, TService5, ValueTask<TState>> step, TService1 service1, TService2 service2, TService3 service3, TService4 service4, TService5 service5)
        where TState : struct
        where TService1 : notnull
        where TService2 : notnull
        where TService3 : notnull
        where TService4 : notnull
        where TService5 : notnull
    {
        return state => step(state, service1, service2, service3, service4, service5);
    }

    private static TValue GetValueOrDefault<TState, TValue>(TState state, Func<TState, TValue>? defaultValueProvider)
        where TState : struct
        where TValue : struct
        => defaultValueProvider is Func<TState, TValue> provider ? provider(state) : default;
}