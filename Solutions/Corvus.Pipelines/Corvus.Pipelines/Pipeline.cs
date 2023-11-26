﻿// <copyright file="Pipeline.cs" company="Endjin Limited">
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

    /// <summary>
    /// An operator that binds the output of one <see cref="PipelineStep{TState}"/> to another <see cref="PipelineStep{TState}"/>
    /// based on a <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="predicate">The predicate which takes the state, and determines the next step.</param>
    /// <param name="thenStep">The step to execute if the predicate is true.</param>
    /// <param name="elseStep">The step to execute if the predicate is false.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which, when executed, will execute the step, choose the appropriate pipeline based on a predicate,
    /// and execute it using the result.</returns>
    public static PipelineStep<TState> If<TState>(Predicate<TState> predicate, PipelineStep<TState> thenStep, PipelineStep<TState> elseStep)
        where TState : struct
    {
        return state => predicate(state) ? thenStep(state) : elseStep(state);
    }

    /// <summary>
    /// An operator that binds the output of one <see cref="PipelineStep{TState}"/> to another <see cref="PipelineStep{TState}"/>
    /// based on a <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="predicate">The predicate which takes the state, and determines the next step.</param>
    /// <param name="thenStep">The step to execute if the predicate is true.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which, when executed, will execute the step, choose the appropriate pipeline based on a predicate,
    /// and execute it using the result.</returns>
    public static PipelineStep<TState> If<TState>(Predicate<TState> predicate, PipelineStep<TState> thenStep)
        where TState : struct
    {
        return state => predicate(state) ? thenStep(state) : ValueTask.FromResult(state);
    }

    /// <summary>
    /// An operator that binds the output of one <see cref="PipelineStep{TState}"/> to another <see cref="PipelineStep{TState}"/>
    /// based on a <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="predicate">The predicate which takes the state, and determines the next step.</param>
    /// <param name="thenStep">The step to execute if the predicate is true.</param>
    /// <param name="elseStep">The step to execute if the predicate is false.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which, when executed, will execute the step, choose the appropriate pipeline based on a predicate,
    /// and execute it using the result.</returns>
    public static SyncPipelineStep<TState> If<TState>(Predicate<TState> predicate, SyncPipelineStep<TState> thenStep, SyncPipelineStep<TState> elseStep)
        where TState : struct
    {
        return state => predicate(state) ? thenStep(state) : elseStep(state);
    }

    /// <summary>
    /// An operator that binds the output of one <see cref="PipelineStep{TState}"/> to another <see cref="PipelineStep{TState}"/>
    /// based on a <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="predicate">The predicate which takes the state, and determines the next step.</param>
    /// <param name="thenStep">The step to execute if the predicate is true.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which, when executed, will execute the step, choose the appropriate pipeline based on a predicate,
    /// and execute it using the result. If the predicate is false, a step which returns the current state will be provided.</returns>
    public static SyncPipelineStep<TState> If<TState>(Predicate<TState> predicate, SyncPipelineStep<TState> thenStep)
        where TState : struct
    {
        return state => predicate(state) ? thenStep(state) : state;
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