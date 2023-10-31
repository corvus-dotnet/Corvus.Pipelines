// <copyright file="PipelineStepExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Buffers;

namespace Corvus.Pipelines;

/// <summary>
/// Operators for <see cref="PipelineStep{TState}"/>.
/// </summary>
public static class PipelineStepExtensions
{
    /// <summary>
    /// An operator which returns a step which logs on entry to and exit from the input step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="step">The step for which to log exit.</param>
    /// <param name="logEntry">The method that performs the logging on entry. It is provided with the state before the step.</param>
    /// <param name="logExit">The method that performs the logging. It is provided with the state both before and after the step.</param>
    /// <returns>A step which wraps the input step and logs on entry and exit.</returns>
    public static PipelineStep<TState> LogEntryAndExit<TState>(this PipelineStep<TState> step, Action<TState> logEntry, Action<TState, TState> logExit)
        where TState : struct, ILoggable
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
    /// An operator which returns a step which logs on entry to the input step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="step">The step for which to log exit.</param>
    /// <param name="logEntry">The method that performs the logging on entry. It is provided with the state before the step.</param>
    /// <returns>A step which wraps the input step and logs on entry.</returns>
    public static PipelineStep<TState> LogEntry<TState>(this PipelineStep<TState> step, Action<TState> logEntry)
        where TState : struct, ILoggable
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
    /// An operator which returns a step which logs on exit from the input step.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="step">The step for which to log exit.</param>
    /// <param name="logExit">The method that performs the logging on exit. It is provided with the state both before and after the step.</param>
    /// <returns>A step which wraps the input step and logs on entry.</returns>
    public static PipelineStep<TState> LogExit<TState>(this PipelineStep<TState> step, Action<TState, TState> logExit)
        where TState : struct, ILoggable
    {
        return step.Bind(
            (TState state) => state,
            (entryState, exitState) =>
            {
                logExit(entryState, exitState);
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
    /// <see cref="ICanFail{TState, TError}"/> step with permanent or transient failure handling via
    /// operators like <see cref="Retry{TState, TError}(PipelineStep{TState}, Predicate{TState}, PipelineStep{TState}?)"/> and
    /// <see cref="OnError{TState, TError}(PipelineStep{TState}, PipelineStep{TState})"/>.
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
    /// <see cref="ICanFail{TState, TError}"/> step with permanent or transient failure handling via
    /// operators like <see cref="Retry{TState, TError}(PipelineStep{TState}, Predicate{TState}, PipelineStep{TState}?)"/> and
    /// <see cref="OnError{TState, TError}(PipelineStep{TState}, PipelineStep{TState})"/>.
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
    /// An operator which provides the ability to retry a step which might fail.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TError">The type of the error details.</typeparam>
    /// <param name="step">The step to execute.</param>
    /// <param name="shouldRetry">A predicate which determines if the step should be retried.</param>
    /// <param name="beforeRetry">An step to carry out before retrying. This is commonly an asynchronous delay, but can be used to return
    /// an updated version of the state before retyring the action (e.g. incrementing an execution count.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which, when executed, will execute the step, choose the appropriate pipeline, based on the result,
    /// and execute it using the result.</returns>
    public static PipelineStep<TState> Retry<TState, TError>(this PipelineStep<TState> step, Predicate<TState> shouldRetry, PipelineStep<TState>? beforeRetry = null)
        where TState : struct, ICanFail<TState, TError>
        where TError : struct
    {
        return async state =>
        {
            TState currentState = state.ResetFailureState();

            while (true)
            {
                currentState = await step(currentState).ConfigureAwait(false);
                if (currentState.ExecutionStatus == PipelineStepStatus.Success || !shouldRetry(currentState))
                {
                    return currentState;
                }

                if (beforeRetry is not null)
                {
                    currentState = await beforeRetry(currentState).ConfigureAwait(false);
                    currentState = currentState.PrepareToRetry();
                }
            }
        };
    }

    /// <summary>
    /// An operator which provides the ability to choose a step to run if the bound step fails.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TError">The type of the error details.</typeparam>
    /// <param name="step">The step to execute.</param>
    /// <param name="onError">The step to execute if the step fails.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which, when executed, will execute the step, and, if an error occurs,
    /// execute the error step before returning the final result.</returns>
    public static PipelineStep<TState> OnError<TState, TError>(
        this PipelineStep<TState> step,
        PipelineStep<TState> onError)
        where TState : struct, ICanFail<TState, TError>
        where TError : struct
    {
        return step.Bind(async state =>
        {
            if (state.ExecutionStatus != PipelineStepStatus.Success)
            {
                return await onError(state).ConfigureAwait(false);
            }

            return state;
        });
    }

    /// <summary>
    /// An operator which provides the ability to choose a step to run if the bound step fails.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TError">The type of the error details.</typeparam>
    /// <param name="step">The step to execute.</param>
    /// <param name="onError">The step to execute if the step fails.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which, when executed, will execute the step, and, if an error occurs,
    /// execute the error step before returning the final result.</returns>
    public static PipelineStep<TState> OnError<TState, TError>(
        this PipelineStep<TState> step,
        SyncPipelineStep<TState> onError)
        where TState : struct, ICanFail<TState, TError>
        where TError : struct
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
    public static PipelineStep<TState> Choose<TState>(this PipelineStep<TState> step, Func<TState, SyncPipelineStep<TState>> selector)
        where TState : struct
    {
        return step.Bind(state => ValueTask.FromResult(selector(state)(state)));
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

    /// <summary>
    /// Convert a synchronous pipeline step to an asynchronous one.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="step">The step to be converted to an async step.</param>
    /// <returns>An async version of the synchronous step.</returns>
    public static PipelineStep<TState> ToAsync<TState>(this SyncPipelineStep<TState> step)
        where TState : struct
    {
        return state => ValueTask.FromResult(step(state));
    }

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
    /// <param name="attempts">The steps to attempt.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which returns the result of the first step to return a value.</returns>
    /// <remarks>This executes the steps in parallel, returning the value from the first step that produces a result, and cancelling the other operations.</remarks>
    public static PipelineStep<TState> FirstToComplete<TState>(
        params PipelineStep<TState>[] attempts)
        where TState : struct, ICancellable<TState>
    {
        return async (TState input) =>
        {
            // Create a linked cancellation token source with whatever the current cancellation token might be
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(input.CancellationToken);
            TState wrappedInput = input.WithCancellationToken(cancellationTokenSource.Token);

            ValueTask<TState>[] valueTasks = ArrayPool<ValueTask<TState>>.Shared.Rent(attempts.Length);
            Task<TState>[]? tasks = null;

            try
            {
                for (int i = 0; i < attempts.Length; ++i)
                {
#pragma warning disable CA2012 // Use ValueTasks correctly - we are deliberately storing this for a brief time for perf reasons
                    valueTasks[i] = attempts[i](wrappedInput);
#pragma warning restore CA2012 // Use ValueTasks correctly
                    if (valueTasks[i].IsCompleted)
                    {
                        return valueTasks[i].Result;
                    }
                }

                // We didn't get a synchronous run, so we wait for any task to complete.
                tasks = ArrayPool<Task<TState>>.Shared.Rent(attempts.Length);
                for (int i = 0; i < attempts.Length; ++i)
                {
                    tasks[i] = valueTasks[i].AsTask();
                }

                Task<TState> result = await Task.WhenAny(tasks.Take(attempts.Length)).ConfigureAwait(false);
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
    /// <remarks>This executes the steps in parallel. See <see cref="CombineSteps{TState1, TState2}"/> for the sequential case.</remarks>
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
    /// <remarks>This executes the steps in parallel. See <see cref="CombineSteps{TState1, TState2, TState3}"/> for the sequential case.</remarks>
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
            async (TState state) => (state, await value1ProviderStep(GetValueOrDefault(state, initialValue1FromState))),
            (TState _, (TState State, TValue1 Value1) result) => ValueTask.FromResult(result.State));
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
                 await value1ProviderStep(GetValueOrDefault(state, initialValue1FromState)),
                 await value2ProviderStep(GetValueOrDefault(state, initialValue2FromState))),
            (TState _, (TState State, TValue1 Value1, TValue2 Value2) result) => ValueTask.FromResult(result.State));
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
                 await value1ProviderStep(GetValueOrDefault(state, initialValue1FromState)),
                 await value2ProviderStep(GetValueOrDefault(state, initialValue2FromState)),
                 await value3ProviderStep(GetValueOrDefault(state, initialValue3FromState))),
            (TState _, (TState State, TValue1 Value1, TValue2 Value2, TValue3 Value3) result) => ValueTask.FromResult(result.State));
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

    private static TValue GetValueOrDefault<TState, TValue>(TState state, Func<TState, TValue>? defaultValueProvider)
        where TState : struct
        where TValue : struct
        => defaultValueProvider is Func<TState, TValue> provider ? provider(state) : default;
}