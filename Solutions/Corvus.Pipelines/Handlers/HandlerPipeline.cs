// <copyright file="HandlerPipeline.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Corvus.Pipelines.Handlers;

/// <summary>
/// A <see cref="Pipeline"/> for a handler pattern.
/// </summary>
/// <remarks>
/// This will pass an instance of the <see cref="HandlerState{TInput, TResult}"/> to each step in the pipeline in turn,
/// and returns the result of the first step for which <see cref="HandlerState{TInput, TResult}.WasHandled"/> is true.
/// </remarks>
public static class HandlerPipeline
{
    /// <summary>
    /// The identity operator. This returns a <see cref="PipelineStep{THandlerState}"/> which, when executed, produces the current state of the pipeline.
    /// </summary>
    /// <typeparam name="TInput">The type of the input to the handler pipeline.</typeparam>
    /// <typeparam name="TResult">The type of the result of handling the input.</typeparam>
    /// <returns>A <see cref="PipelineStep{THandlerState}"/> which, when executed, produces the current state of the pipeline.</returns>
    public static PipelineStep<HandlerState<TInput, TResult>> GetCurrent<TInput, TResult>() => Pipeline.Current<HandlerState<TInput, TResult>>();

    /// <summary>
    /// Create a named step.
    /// </summary>
    /// <typeparam name="TInput">The type of the input to the handler pipeline.</typeparam>
    /// <typeparam name="TResult">The type of the result of handling the input.</typeparam>
    /// <param name="step">The step.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>A named step.</returns>
    public static SyncPipelineStepProvider<HandlerState<TInput, TResult>> WithName<TInput, TResult>(this SyncPipelineStep<HandlerState<TInput, TResult>> step, [CallerArgumentExpression(nameof(step))] string? name = null) => PipelineStepExtensions.WithName(step, name);

    /// <summary>
    /// Create a named step.
    /// </summary>
    /// <typeparam name="TInput">The type of the input to the handler pipeline.</typeparam>
    /// <typeparam name="TResult">The type of the result of handling the input.</typeparam>
    /// <param name="step">The step.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>A named step.</returns>
    public static PipelineStepProvider<HandlerState<TInput, TResult>> WithName<TInput, TResult>(this PipelineStep<HandlerState<TInput, TResult>> step, [CallerArgumentExpression(nameof(step))] string? name = null) => PipelineStepExtensions.WithName(step, name);

    /// <summary>
    /// Builds a handler pipeline from an ordered array of handlers.
    /// </summary>
    /// <typeparam name="TInput">The type of the input to the handler pipeline.</typeparam>
    /// <typeparam name="TResult">The type of the result of handling the input.</typeparam>
    /// <param name="steps">The handlers.</param>
    /// <returns>A <see cref="PipelineStep{THandlerState}"/> which will execute the handler pipeline.</returns>
    /// <remarks>
    /// <para>
    /// When you build and execute the <see cref="HandlerPipeline"/>, you start with the
    /// state provided by <see cref="HandlerState{TInput, TResult}.For(TInput, ILogger?)"/>. The state is passed to the
    /// the first <see cref="PipelineStep{HandlerState}"/>. If the step returns
    /// <see cref="HandlerState{TInput, TResult}.NotHandled()"/>, it is passed to the
    /// next step, until one successfully handles the input and returns a result using
    /// <see cref="HandlerState{TInput, TResult}.Handled(TResult)"/>. At this point the pipeline
    /// will be terminated.
    /// </para>
    /// <para>
    /// On termination, you can inspect the resulting value using <see cref="HandlerState{TInput, TResult}.WasHandled(out TResult)"/>.
    /// </para>
    /// </remarks>
    public static PipelineStep<HandlerState<TInput, TResult>> Build<TInput, TResult>(params PipelineStep<HandlerState<TInput, TResult>>[] steps)
    {
        return Pipeline.Build(
            ctx => ctx.ShouldTerminate(),
            steps);
    }

    /// <summary>
    /// Builds a handler pipeline from an ordered array of handlers.
    /// </summary>
    /// <typeparam name="TInput">The type of the input to the handler pipeline.</typeparam>
    /// <typeparam name="TResult">The type of the result of handling the input.</typeparam>
    /// <param name="steps">The handlers.</param>
    /// <returns>A <see cref="PipelineStep{THandlerState}"/> which will execute the handler pipeline.</returns>
    /// <remarks>
    /// <para>
    /// When you build and execute the <see cref="HandlerPipeline"/>, you start with the
    /// state provided by <see cref="HandlerState{TInput, TResult}.For(TInput, ILogger?)"/>. The state is passed to the
    /// the first <see cref="PipelineStep{HandlerState}"/>. If the step returns
    /// <see cref="HandlerState{TInput, TResult}.NotHandled()"/>, it is passed to the
    /// next step, until one successfully handles the input and returns a result using
    /// <see cref="HandlerState{TInput, TResult}.Handled(TResult)"/>. At this point the pipeline
    /// will be terminated.
    /// </para>
    /// <para>
    /// On termination, you can inspect the resulting value using <see cref="HandlerState{TInput, TResult}.WasHandled(out TResult)"/>.
    /// </para>
    /// </remarks>
    public static SyncPipelineStep<HandlerState<TInput, TResult>> Build<TInput, TResult>(params SyncPipelineStep<HandlerState<TInput, TResult>>[] steps)
    {
        return Pipeline.Build(
            ctx => ctx.ShouldTerminate(),
            steps);
    }

    /// <summary>
    /// Builds a handler pipeline from an ordered array of handlers.
    /// </summary>
    /// <typeparam name="TInput">The type of the input to the handler pipeline.</typeparam>
    /// <typeparam name="TResult">The type of the result of handling the input.</typeparam>
    /// <param name="scopeName">The scope name for the pipeline.</param>
    /// <param name="level">The level at which to surface step entry/exit logging.</param>
    /// <param name="steps">The handlers.</param>
    /// <returns>A <see cref="PipelineStep{THandlerState}"/> which will execute the handler pipeline.</returns>
    /// <remarks>
    /// <para>
    /// When you build and execute the <see cref="HandlerPipeline"/>, you start with the
    /// state provided by <see cref="HandlerState{TInput, TResult}.For(TInput, ILogger?)"/>. The state is passed to the
    /// the first <see cref="PipelineStep{HandlerState}"/>. If the step returns
    /// <see cref="HandlerState{TInput, TResult}.NotHandled()"/>, it is passed to the
    /// next step, until one successfully handles the input and returns a result using
    /// <see cref="HandlerState{TInput, TResult}.Handled(TResult)"/>. At this point the pipeline
    /// will be terminated.
    /// </para>
    /// <para>
    /// On termination, you can inspect the resulting value using <see cref="HandlerState{TInput, TResult}.WasHandled(out TResult)"/>.
    /// </para>
    /// </remarks>
    public static PipelineStep<HandlerState<TInput, TResult>> Build<TInput, TResult>(string scopeName, LogLevel level, params PipelineStepProvider<HandlerState<TInput, TResult>>[] steps)
    {
        return Pipeline.Build(
            ctx => ctx.ShouldTerminate(),
            scopeName,
            level,
            steps);
    }

    /// <summary>
    /// Builds a handler pipeline from an ordered array of handlers.
    /// </summary>
    /// <typeparam name="TInput">The type of the input to the handler pipeline.</typeparam>
    /// <typeparam name="TResult">The type of the result of handling the input.</typeparam>
    /// <param name="scopeName">The scope name for the pipeline.</param>
    /// <param name="level">The level at which to surface step entry/exit logging.</param>
    /// <param name="steps">The handlers.</param>
    /// <returns>A <see cref="PipelineStep{THandlerState}"/> which will execute the handler pipeline.</returns>
    /// <remarks>
    /// <para>
    /// When you build and execute the <see cref="HandlerPipeline"/>, you start with the
    /// state provided by <see cref="HandlerState{TInput, TResult}.For(TInput, ILogger?)"/>. The state is passed to the
    /// the first <see cref="PipelineStep{HandlerState}"/>. If the step returns
    /// <see cref="HandlerState{TInput, TResult}.NotHandled()"/>, it is passed to the
    /// next step, until one successfully handles the input and returns a result using
    /// <see cref="HandlerState{TInput, TResult}.Handled(TResult)"/>. At this point the pipeline
    /// will be terminated.
    /// </para>
    /// <para>
    /// On termination, you can inspect the resulting value using <see cref="HandlerState{TInput, TResult}.WasHandled(out TResult)"/>.
    /// </para>
    /// </remarks>
    public static SyncPipelineStep<HandlerState<TInput, TResult>> Build<TInput, TResult>(string scopeName, LogLevel level, params SyncPipelineStepProvider<HandlerState<TInput, TResult>>[] steps)
    {
        return Pipeline.Build(
            ctx => ctx.ShouldTerminate(),
            scopeName,
            level,
            steps);
    }

    /// <summary>
    /// An operator that produces a <see cref="PipelineStep{TState}"/> that executes a <see cref="PipelineStep{TState}"/>
    /// chosen by a handler pipeline.
    /// </summary>
    /// <typeparam name="THandlerInput">The handler input type.</typeparam>
    /// <typeparam name="TState">The pipeline state type.</typeparam>
    /// <param name="getInput">
    /// Gets the handler input from the pipeline state.
    /// </param>
    /// <param name="notHandled">Invoked if not handled.</param>
    /// <param name="handlers">
    /// The sequence of handlers, the outcome of which determines the pipeline.
    /// </param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which, when executed, will execute the handlers to choose the appropriate pipeline,
    /// and execute it.</returns>
    public static PipelineStep<TState> Choose<THandlerInput, TState>(
        Func<TState, THandlerInput> getInput,
        PipelineStep<TState> notHandled,
        params PipelineStep<HandlerState<THandlerInput, PipelineStep<TState>>>[] handlers)
        where TState : struct
        => Build(handlers)
            .Bind(
                (TState state) => HandlerState<THandlerInput, PipelineStep<TState>>.For(getInput(state)),
                (TState state, HandlerState<THandlerInput, PipelineStep<TState>> handlerState) =>
                    handlerState.WasHandled(out PipelineStep<TState>? result) ? result(state) : notHandled(state));

    /// <summary>
    /// An operator that produces a <see cref="PipelineStep{TState}"/> that executes a <see cref="PipelineStep{TState}"/>
    /// chosen by a handler pipeline.
    /// </summary>
    /// <typeparam name="THandlerInput">The handler input type.</typeparam>
    /// <typeparam name="TState">The pipeline state type.</typeparam>
    /// <param name="getInput">
    /// Gets the handler input from the pipeline state.
    /// </param>
    /// <param name="notHandled">Invoked if not handled.</param>
    /// <param name="handlers">
    /// The sequence of handlers, the outcome of which determines the pipeline.
    /// </param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which, when executed, will execute the handlers to choose the appropriate pipeline,
    /// and execute it.</returns>
    public static PipelineStep<TState> Choose<THandlerInput, TState>(
        Func<TState, THandlerInput> getInput,
        PipelineStep<TState> notHandled,
        params SyncPipelineStep<HandlerState<THandlerInput, PipelineStep<TState>>>[] handlers)
        where TState : struct
        => Build(handlers).ToAsync()
            .Bind(
                (TState state) => HandlerState<THandlerInput, PipelineStep<TState>>.For(getInput(state)),
                (TState state, HandlerState<THandlerInput, PipelineStep<TState>> handlerState) =>
                    handlerState.WasHandled(out PipelineStep<TState>? result) ? result(state) : notHandled(state));

    /// <summary>
    /// An operator that produces a <see cref="PipelineStep{TState}"/> that executes a <see cref="PipelineStep{TState}"/>
    /// chosen by a handler pipeline.
    /// </summary>
    /// <typeparam name="THandlerInput">The handler input type.</typeparam>
    /// <typeparam name="TState">The pipeline state type.</typeparam>
    /// <param name="getInput">
    /// Gets the handler input from the pipeline state.
    /// </param>
    /// <param name="notHandled">Invoked if not handled.</param>
    /// <param name="handlers">
    /// The sequence of handlers, the outcome of which determines the pipeline.
    /// </param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which, when executed, will execute the handlers to choose the appropriate pipeline,
    /// and execute it.</returns>
    public static PipelineStep<TState> Choose<THandlerInput, TState>(
        Func<TState, THandlerInput> getInput,
        SyncPipelineStep<TState> notHandled,
        params PipelineStep<HandlerState<THandlerInput, SyncPipelineStep<TState>>>[] handlers)
        where TState : struct
        => Build(handlers)
            .Bind(
                (TState state) => HandlerState<THandlerInput, SyncPipelineStep<TState>>.For(getInput(state)),
                (TState state, HandlerState<THandlerInput, SyncPipelineStep<TState>> handlerState) =>
                    handlerState.WasHandled(out SyncPipelineStep<TState>? result) ? result(state) : notHandled(state));

    /// <summary>
    /// An operator that produces a <see cref="PipelineStep{TState}"/> that executes a <see cref="PipelineStep{TState}"/>
    /// chosen by a handler pipeline.
    /// </summary>
    /// <typeparam name="THandlerInput">The handler input type.</typeparam>
    /// <typeparam name="TState">The pipeline state type.</typeparam>
    /// <param name="getInput">
    /// Gets the handler input from the pipeline state.
    /// </param>
    /// <param name="notHandled">Invoked if not handled.</param>
    /// <param name="handlers">
    /// The sequence of handlers, the outcome of which determines the pipeline.
    /// </param>
    /// <returns>A <see cref="PipelineStep{TState}"/> which, when executed, will execute the handlers to choose the appropriate pipeline,
    /// and execute it.</returns>
    public static SyncPipelineStep<TState> Choose<THandlerInput, TState>(
        Func<TState, THandlerInput> getInput,
        SyncPipelineStep<TState> notHandled,
        params SyncPipelineStep<HandlerState<THandlerInput, SyncPipelineStep<TState>>>[] handlers)
        where TState : struct
        => Build(handlers)
            .Bind(
                (TState state) => HandlerState<THandlerInput, SyncPipelineStep<TState>>.For(getInput(state)),
                (TState state, HandlerState<THandlerInput, SyncPipelineStep<TState>> handlerState) =>
                    handlerState.WasHandled(out SyncPipelineStep<TState>? result) ? result(state) : notHandled(state));
}