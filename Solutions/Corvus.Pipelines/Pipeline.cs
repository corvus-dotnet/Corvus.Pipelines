// <copyright file="Pipeline.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Corvus.Pipelines;

/// <summary>
/// Build a pipeline of sequentially executed steps from an array of <see cref="PipelineStep{TState}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Each step operates on the instance of the <c>TState</c>, and returns an instance of the <c>TState</c>. That result is fed as the input
/// to the next step in the pipeline.
/// </para>
/// <para>
/// It is usual for the <c>TState</c> to be an immutable type, but side-effects are permitted.
/// </para>
/// <para>
/// A function that returns an instance of a step, rather than the result of a step, is called an <b>operator</b>. These operators
/// implement common patterns, that are resolved when the pipeline is executed.
/// </para>
/// <para>
/// (For the mathematically minded, we use "operator" broadly in the sense of a function that maps one function space to another,
/// rather than the dotnet <c>operator</c>s, such as addition, subtraction and instance type conversion).
/// </para>
/// <para>
/// The <see cref="PipelineStep{TState}"/> is an asynchronous function, which
/// returns a <see cref="ValueTask{TState}"/>.
/// </para>
/// <para>
/// For purely synchronous pipelines, you can use the overloads <see cref="Build{TState}(SyncPipelineStep{TState}[])"/>
/// and <see cref="Build{TState}(Predicate{TState}, SyncPipelineStep{TState}[])"/> that take a <see cref="SyncPipelineStep{TState}"/>
/// and optimize for that case.
/// </para>
/// <para>
/// For mixed sync and async pipelines, you should wrap your <see cref="SyncPipelineStep{TState}"/> instances in a call to
/// <see cref="ValueTask.FromResult{TResult}(TResult)"/>.
/// </para>
/// </remarks>
public static class Pipeline
{
    /// <summary>
    /// The identity operator. An operator that provides current value of the state.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <returns>A pipeline step which, when executed, provides the current version of the state.</returns>
    public static PipelineStep<TState> Current<TState>()
        where TState : struct
    {
        return static state => ValueTask.FromResult(state);
    }

    /// <summary>
    /// The identity operator. An operator that provides current value of the state.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <returns>A pipeline step which, when executed, provides the current version of the state.</returns>
    public static SyncPipelineStep<TState> CurrentSync<TState>()
        where TState : struct
    {
        return static state => state;
    }

    /// <summary>
    /// An operator that provides a <see cref="PipelineStep{TState}"/> which executes a series of steps in order.
    /// </summary>
    /// <typeparam name="TState">The type of the state for the pipeline.</typeparam>
    /// <param name="steps">The steps to be executed, in order.</param>
    /// <returns>A step representing the pipeline.</returns>
    /// <remarks>
    /// <para>
    /// When you build and execute the <see cref="Pipeline"/>, you pass it an initial instance of the <typeparamref name="TState"/>.
    /// </para>
    /// <para>The initial state is passed to the first <see cref="PipelineStep{TState}"/> which returns an updated state instance, which is
    /// passed to the next step, and so on, until the final resulting state is returned.
    /// </para>
    /// </remarks>
    public static PipelineStep<TState> Build<TState>(params PipelineStep<TState>[] steps)
        where TState : struct
    {
        return async state =>
        {
            TState currentResult = state;
            foreach (PipelineStep<TState> step in steps)
            {
                currentResult = await step(currentResult).ConfigureAwait(false);
            }

            return currentResult;
        };
    }

    /// <summary>
    /// An operator that provides a <see cref="PipelineStep{TState}"/> which executes a series of steps in order.
    /// </summary>
    /// <typeparam name="TState">The type of the state for the pipeline.</typeparam>
    /// <param name="steps">The steps to be executed, in order. In this overload, they are all synchronous functions.</param>
    /// <returns>A step representing the pipeline.</returns>
    /// <remarks>
    /// <para>
    /// When you build and execute the <see cref="Pipeline"/>, you pass it an initial instance of the <typeparamref name="TState"/>.
    /// </para>
    /// <para>The initial state is passed to the first <see cref="PipelineStep{TState}"/> which returns an updated state instance, which is
    /// passed to the next step, and so on, until the final resulting state is returned.
    /// </para>
    /// </remarks>
    public static SyncPipelineStep<TState> Build<TState>(params SyncPipelineStep<TState>[] steps)
        where TState : struct
    {
        return state =>
        {
            TState currentResult = state;
            foreach (SyncPipelineStep<TState> step in steps)
            {
                currentResult = step(currentResult);
            }

            return currentResult;
        };
    }

    /// <summary>
    /// An operator that provides a <see cref="PipelineStep{TState}"/> which executes a series of steps in order, with
    /// the ability to terminate early if the <paramref name="shouldTerminate"/> predicate returns true.
    /// </summary>
    /// <typeparam name="TState">The type of the state for the pipeline.</typeparam>
    /// <param name="shouldTerminate">A predicate which returns true if the pipeline should terminate early.</param>
    /// <param name="steps">The steps to be executed, in order.</param>
    /// <returns>A step representing the pipeline.</returns>
    /// <remarks>
    /// <para>
    /// When you build and execute the <see cref="Pipeline"/>, you pass it an initial instance of the <typeparamref name="TState"/>.
    /// </para>
    /// <para>The initial state is passed to the first <see cref="PipelineStep{TState}"/> which returns an updated state instance, which is
    /// passed to the next step, and so on, until one returns an instance for which the <paramref name="shouldTerminate"/> predicate
    /// returns <see langword="true"/>. At this point the pipeline will be terminated, and the resulting state returned.
    /// </para>
    /// </remarks>
    public static PipelineStep<TState> Build<TState>(Predicate<TState> shouldTerminate, params PipelineStep<TState>[] steps)
        where TState : struct
    {
        return async state =>
        {
            TState currentResult = state;

            foreach (PipelineStep<TState> step in steps)
            {
                if (shouldTerminate(currentResult))
                {
                    break;
                }

                currentResult = await step(currentResult).ConfigureAwait(false);
            }

            return currentResult;
        };
    }

    /// <summary>
    /// An operator that provides a <see cref="PipelineStep{TState}"/> which executes a series of steps in order, with
    /// the ability to terminate early if the <paramref name="shouldTerminate"/> predicate returns true.
    /// </summary>
    /// <typeparam name="TState">The type of the state for the pipeline.</typeparam>
    /// <param name="shouldTerminate">A predicate which returns true if the pipeline should terminate early.</param>
    /// <param name="steps">The steps to be executed, in order. In this overload, they are all synchronous functions.</param>
    /// <returns>A step representing the pipeline.</returns>
    /// <remarks>
    /// <para>
    /// When you build and execute the <see cref="Pipeline"/>, you pass it an initial instance of the <typeparamref name="TState"/>.
    /// </para>
    /// <para>The initial state is passed to the first <see cref="PipelineStep{TState}"/> which returns an updated state instance, which is
    /// passed to the next step, and so on, until one returns an instance for which the <paramref name="shouldTerminate"/> predicate
    /// returns <see langword="true"/>. At this point the pipeline will be terminated, and the resulting state returned.
    /// </para>
    /// </remarks>
    public static SyncPipelineStep<TState> Build<TState>(Predicate<TState> shouldTerminate, params SyncPipelineStep<TState>[] steps)
        where TState : struct
    {
        return state =>
        {
            TState currentResult = state;
            foreach (SyncPipelineStep<TState> step in steps)
            {
                if (shouldTerminate(currentResult))
                {
                    break;
                }

                currentResult = step(currentResult);
            }

            return currentResult;
        };
    }

    /// <summary>
    /// An operator that provides a <see cref="PipelineStep{TState}"/> which executes a series of steps in order, with entry and exit logging.
    /// </summary>
    /// <typeparam name="TState">The type of the state for the pipeline.</typeparam>
    /// <param name="scopeName">The scope name for the pipeline.</param>
    /// <param name="level">The log level to use for entry/exit logging.</param>
    /// <param name="steps">The steps to be executed, in order.</param>
    /// <returns>A step representing the pipeline.</returns>
    /// <remarks>
    /// <para>
    /// When you build and execute the <see cref="Pipeline"/>, you pass it an initial instance of the <typeparamref name="TState"/>.
    /// </para>
    /// <para>The initial state is passed to the first <see cref="PipelineStep{TState}"/> which returns an updated state instance, which is
    /// passed to the next step, and so on, until the final resulting state is returned.
    /// </para>
    /// </remarks>
    public static PipelineStep<TState> Build<TState>(string scopeName, LogLevel level, params PipelineStepProvider<TState>[] steps)
        where TState : struct, ILoggable
    {
        EnsureNames(steps);

        return async state =>
        {
            using IDisposable? scope = state.Logger.BeginScope(scopeName);

            TState currentResult = state;
            foreach (PipelineStepProvider<TState> step in steps)
            {
                using IDisposable? stepScope = state.Logger.BeginScope(step.Name());

                LogEntry(level, state);

                currentResult = await step.Step(currentResult).ConfigureAwait(false);

                LogExit(level, state);
            }

            return currentResult;
        };
    }

    /// <summary>
    /// An operator that provides a <see cref="PipelineStep{TState}"/> which executes a series of steps in order, with entry and exit logging.
    /// </summary>
    /// <typeparam name="TState">The type of the state for the pipeline.</typeparam>
    /// <param name="scopeName">The scope name for the pipeline.</param>
    /// <param name="level">The log level to use for entry/exit logging.</param>
    /// <param name="steps">The steps to be executed, in order. In this overload, they are all synchronous functions.</param>
    /// <returns>A step representing the pipeline.</returns>
    /// <remarks>
    /// <para>
    /// When you build and execute the <see cref="Pipeline"/>, you pass it an initial instance of the <typeparamref name="TState"/>.
    /// </para>
    /// <para>The initial state is passed to the first <see cref="PipelineStep{TState}"/> which returns an updated state instance, which is
    /// passed to the next step, and so on, until the final resulting state is returned.
    /// </para>
    /// </remarks>
    public static SyncPipelineStep<TState> Build<TState>(string scopeName, LogLevel level, params SyncPipelineStepProvider<TState>[] steps)
        where TState : struct, ILoggable
    {
        EnsureNames(steps);

        return state =>
        {
            using IDisposable? scope = state.Logger.BeginScope(scopeName);

            TState currentResult = state;
            foreach (SyncPipelineStepProvider<TState> step in steps)
            {
                using IDisposable? stepScope = state.Logger.BeginScope(step.Name());

                LogEntry(level, state);

                currentResult = step.Step(currentResult);

                LogExit(level, state);
            }

            return currentResult;
        };
    }

    /// <summary>
    /// An operator that provides a <see cref="PipelineStep{TState}"/> which executes a series of steps in order, with
    /// the ability to terminate early if the <paramref name="shouldTerminate"/> predicate returns true. It also logs on entry, and
    /// exit or termination.
    /// </summary>
    /// <typeparam name="TState">The type of the state for the pipeline.</typeparam>
    /// <param name="shouldTerminate">A predicate which returns true if the pipeline should terminate early.</param>
    /// <param name="scopeName">The scope name for the pipeline.</param>
    /// <param name="level">The log level to use for entry/exit logging.</param>
    /// <param name="steps">The steps to be executed, in order.</param>
    /// <returns>A step representing the pipeline.</returns>
    /// <remarks>
    /// <para>
    /// When you build and execute the <see cref="Pipeline"/>, you pass it an initial instance of the <typeparamref name="TState"/>.
    /// </para>
    /// <para>The initial state is passed to the first <see cref="PipelineStep{TState}"/> which returns an updated state instance, which is
    /// passed to the next step, and so on, until one returns an instance for which the <paramref name="shouldTerminate"/> predicate
    /// returns <see langword="true"/>. At this point the pipeline will be terminated, and the resulting state returned.
    /// </para>
    /// </remarks>
    public static PipelineStep<TState> Build<TState>(Predicate<TState> shouldTerminate, string scopeName, LogLevel level, params PipelineStepProvider<TState>[] steps)
        where TState : struct, ILoggable
    {
        EnsureNames(steps);

        return async state =>
        {
            using IDisposable? scope = state.Logger.BeginScope(scopeName);

            TState currentResult = state;
            foreach (PipelineStepProvider<TState> step in steps)
            {
                using IDisposable? stepScope = state.Logger.BeginScope(step.Name());

                if (shouldTerminate(currentResult))
                {
                    LogTerminated(level, state);
                    break;
                }

                LogEntry(level, state);

                currentResult = await step.Step(currentResult).ConfigureAwait(false);

                LogExit(level, state);
            }

            return currentResult;
        };
    }

    /// <summary>
    /// An operator that provides a <see cref="PipelineStep{TState}"/> which executes a series of steps in order, with
    /// the ability to terminate early if the <paramref name="shouldTerminate"/> predicate returns true.
    /// </summary>
    /// <typeparam name="TState">The type of the state for the pipeline.</typeparam>
    /// <param name="shouldTerminate">A predicate which returns true if the pipeline should terminate early.</param>
    /// <param name="scopeName">The name for the pipeline scope.</param>
    /// <param name="level">The log level to use for entry/exit logging.</param>
    /// <param name="steps">The steps to be executed, in order. In this overload, they are all synchronous functions.</param>
    /// <returns>A step representing the pipeline.</returns>
    /// <remarks>
    /// <para>
    /// When you build and execute the <see cref="Pipeline"/>, you pass it an initial instance of the <typeparamref name="TState"/>.
    /// </para>
    /// <para>The initial state is passed to the first <see cref="PipelineStep{TState}"/> which returns an updated state instance, which is
    /// passed to the next step, and so on, until one returns an instance for which the <paramref name="shouldTerminate"/> predicate
    /// returns <see langword="true"/>. At this point the pipeline will be terminated, and the resulting state returned.
    /// </para>
    /// </remarks>
    public static SyncPipelineStep<TState> Build<TState>(Predicate<TState> shouldTerminate, string scopeName, LogLevel level, params SyncPipelineStepProvider<TState>[] steps)
        where TState : struct, ILoggable
    {
        EnsureNames(steps);

        return state =>
        {
            using IDisposable? scope = state.Logger.BeginScope(scopeName);
            TState currentResult = state;
            foreach (SyncPipelineStepProvider<TState> step in steps)
            {
                using IDisposable? stepScope = state.Logger.BeginScope(step.Name());

                if (shouldTerminate(currentResult))
                {
                    LogTerminated(level, state);
                    break;
                }

                LogEntry(level, state);

                currentResult = step.Step(currentResult);

                LogExit(level, state);
            }

            return currentResult;
        };
    }

    /// <summary>
    /// An operator that produces a <see cref="SyncPipelineStep{TState}"/> that executes a <see cref="SyncPipelineStep{TState}"/>
    /// chosen by a <paramref name="selector"/> function.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="selector">The selector which takes the input state and chooses a pipeline with which to proceed.</param>
    /// <returns>A <see cref="SyncPipelineStep{TState}"/> which, when executed, will execute the selector to choose the appropriate pipeline,
    /// and execute it.</returns>
    public static SyncPipelineStep<TState> Choose<TState>(Func<TState, SyncPipelineStep<TState>> selector)
        where TState : struct
    {
        return state => selector(state)(state);
    }

    /// <summary>
    /// An operator that produces a <see cref="PipelineStep{TState}"/> that executes a <see cref="PipelineStep{TState}"/>
    /// chosen by a <paramref name="selector"/> function.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="selector">The selector which takes the input state and chooses a pipeline with which to proceed.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which, when executed, will execute the selector to choose the appropriate pipeline,
    /// and execute it.</returns>
    public static PipelineStep<TState> Choose<TState>(Func<TState, PipelineStep<TState>> selector)
        where TState : struct
    {
        return async state => await selector(state)(state).ConfigureAwait(false);
    }

    private static void EnsureNames<TState>(SyncPipelineStepProvider<TState>[] steps)
        where TState : struct, ILoggable
    {
        for (int i = 0; i < steps.Length; ++i)
        {
            if (!steps[i].HasName())
            {
                steps[i] = steps[i].WithName($"Step {i}");
            }
        }
    }

    private static void EnsureNames<TState>(PipelineStepProvider<TState>[] steps)
        where TState : struct, ILoggable
    {
        for (int i = 0; i < steps.Length; ++i)
        {
            if (!steps[i].HasName())
            {
                steps[i] = steps[i].WithName($"Step {i}");
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void LogTerminated<TState>(LogLevel level, in TState state)
        where TState : struct, ILoggable
    {
        if (state.Logger.IsEnabled(level))
        {
            switch (level)
            {
                case LogLevel.Debug:
                    state.Logger.LogDebug(EventIds.TerminatedAtStep, "terminated");
                    break;
                case LogLevel.Trace:
                    state.Logger.LogTrace(EventIds.TerminatedAtStep, "terminated");
                    break;
                case LogLevel.Information:
                    state.Logger.LogInformation(EventIds.TerminatedAtStep, "terminated");
                    break;
                case LogLevel.Warning:
                    state.Logger.LogWarning(EventIds.TerminatedAtStep, "terminated");
                    break;
                case LogLevel.Error:
                    state.Logger.LogError(EventIds.TerminatedAtStep, "terminated");
                    break;
                case LogLevel.Critical:
                    state.Logger.LogCritical(EventIds.TerminatedAtStep, "terminated");
                    break;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void LogEntry<TState>(LogLevel level, in TState state)
        where TState : struct, ILoggable
    {
        if (state.Logger.IsEnabled(level))
        {
            switch (level)
            {
                case LogLevel.Debug:
                    state.Logger.LogDebug(EventIds.EnteredStep, "entered");
                    break;
                case LogLevel.Trace:
                    state.Logger.LogTrace(EventIds.EnteredStep, "entered");
                    break;
                case LogLevel.Information:
                    state.Logger.LogInformation(EventIds.EnteredStep, "entered");
                    break;
                case LogLevel.Warning:
                    state.Logger.LogWarning(EventIds.EnteredStep, "entered");
                    break;
                case LogLevel.Error:
                    state.Logger.LogError(EventIds.EnteredStep, "entered");
                    break;
                case LogLevel.Critical:
                    state.Logger.LogCritical(EventIds.EnteredStep, "entered");
                    break;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void LogExit<TState>(LogLevel level, in TState state)
        where TState : struct, ILoggable
    {
        if (state.Logger.IsEnabled(level))
        {
            switch (level)
            {
                case LogLevel.Debug:
                    state.Logger.LogDebug(EventIds.ExitedStep, "exited");
                    break;
                case LogLevel.Trace:
                    state.Logger.LogTrace(EventIds.ExitedStep, "exited");
                    break;
                case LogLevel.Information:
                    state.Logger.LogInformation(EventIds.ExitedStep, "exited");
                    break;
                case LogLevel.Warning:
                    state.Logger.LogWarning(EventIds.ExitedStep, "exited");
                    break;
                case LogLevel.Error:
                    state.Logger.LogError(EventIds.ExitedStep, "exited");
                    break;
                case LogLevel.Critical:
                    state.Logger.LogCritical(EventIds.ExitedStep, "exited");
                    break;
            }
        }
    }

    /// <summary>
    /// Common event IDs for pipeline steps.
    /// </summary>
    public static class EventIds
    {
        /// <summary>
        /// An event that occurs on a normal exit from a step.
        /// </summary>
        public static readonly EventId Result = new(2000, "Result");

        /// <summary>
        /// An event that occurs on entry to a step.
        /// </summary>
        public static readonly EventId EnteredStep = new(2001, "Entered step");

        /// <summary>
        /// An event that occurs on a normal exit from a step.
        /// </summary>
        public static readonly EventId ExitedStep = new(2002, "Exited step");

        /// <summary>
        /// An event that occurs on a terminating exit from a step.
        /// </summary>
        public static readonly EventId TerminatedAtStep = new(2003, "Terminated at step");

        /// <summary>
        /// An event that occurs before retrying a step.
        /// </summary>
        public static readonly EventId Retrying = new(2004, "Retrying a step");

        /// <summary>
        /// An event that occurs on a transient failure.
        /// </summary>
        public static readonly EventId TransientFailure = new(4000, "Transient failure");

        /// <summary>
        /// An event that occurs on a permanent failure.
        /// </summary>
        public static readonly EventId PermanentFailure = new(5000, "Permanent failure");
    }
}