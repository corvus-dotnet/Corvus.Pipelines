// <copyright file="YarpRequestPipeline.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Corvus.Pipelines;
using Corvus.Pipelines.Handlers;

namespace Corvus.YarpPipelines;

/// <summary>
/// A <see cref="Pipeline"/> for handling YARP transforms.
/// </summary>
public static class YarpRequestPipeline
{
    /// <summary>
    /// Gets the an instance of a <see cref="PipelineStep{YarpRequestPipelineState}"/> that returns
    /// the current pipeline state (the Identity operator).
    /// </summary>
    public static PipelineStep<YarpRequestPipelineState> Current { get; } = Pipeline.Current<YarpRequestPipelineState>();

    /// <summary>
    /// Gets the an instance of a <see cref="PipelineStep{YarpRequestPipelineState}"/> that returns
    /// the current pipeline state (the Identity operator).
    /// </summary>
    public static SyncPipelineStep<YarpRequestPipelineState> CurrentSync { get; } = Pipeline.CurrentSync<YarpRequestPipelineState>();

    /// <summary>
    /// Builds an asynchronous pipeline of <see cref="PipelineStep{YarpRequestPipelineState}"/>.
    /// </summary>
    /// <param name="steps">The ordered array of steps in the pipeline.</param>
    /// <returns>A <see cref="PipelineStep{YarpRequestPipelineState}"/> that executes the pipeline.</returns>
    public static PipelineStep<YarpRequestPipelineState> Build(params PipelineStep<YarpRequestPipelineState>[] steps)
    {
        return Pipeline.Build(
            ctx => ctx.ShouldTerminatePipeline,
            steps);
    }

    /// <summary>
    /// Builds a synchronous pipeline of <see cref="PipelineStep{YarpRequestPipelineState}"/>.
    /// </summary>
    /// <param name="steps">The ordered array of steps in the pipeline.</param>
    /// <returns>A <see cref="PipelineStep{YarpRequestPipelineState}"/> that executes the pipeline.</returns>
    public static SyncPipelineStep<YarpRequestPipelineState> Build(params SyncPipelineStep<YarpRequestPipelineState>[] steps)
    {
        return Pipeline.Build(
            ctx => ctx.ShouldTerminatePipeline,
            steps);
    }

    /// <summary>
    /// An operator that produces a <see cref="PipelineStep{YarpRequestPipelineState}"/> that executes a <see cref="PipelineStep{YarpRequestPipelineState}"/>
    /// chosen by a <paramref name="selector"/> function.
    /// </summary>
    /// <param name="selector">The selector which takes the input state and chooses a pipeline with which to proceed.</param>
    /// <returns>A <see cref="PipelineStep{YarpRequestPipelineState}"/> which, when executed, will execute the selector to choose the appropriate pipeline,
    /// and execute it.</returns>
    public static PipelineStep<YarpRequestPipelineState> Choose(Func<YarpRequestPipelineState, PipelineStep<YarpRequestPipelineState>> selector)
         => Pipeline.Choose(selector);

    /// <summary>
    /// An operator that produces a <see cref="PipelineStep{YarpRequestPipelineState}"/> that executes a <see cref="PipelineStep{YarpRequestPipelineState}"/>
    /// chosen by a <paramref name="selector"/> function.
    /// </summary>
    /// <param name="selector">The selector which takes the input state and chooses a pipeline with which to proceed.</param>
    /// <returns>A <see cref="PipelineStep{YarpRequestPipelineState}"/> which, when executed, will execute the selector to choose the appropriate pipeline,
    /// and execute it.</returns>
    public static SyncPipelineStep<YarpRequestPipelineState> Choose(Func<YarpRequestPipelineState, SyncPipelineStep<YarpRequestPipelineState>> selector)
         => Pipeline.Choose(selector);

    /// <summary>
    /// An operator that produces a <see cref="PipelineStep{YarpRequestPipelineState}"/> that executes a <see cref="PipelineStep{TState}"/>
    /// chosen by a handler pipeline that takes a <see cref="RequestSignature"/> as input.
    /// </summary>
    /// <param name="notHandled">Invoked if not handled.</param>
    /// <param name="handlers">
    /// The sequence of handlers, the outcome of which determines the pipeline.
    /// </param>
    /// <returns>A <see cref="PipelineStep{YarpRequestPipelineState}"/> which, when executed, will execute the handlers to choose the appropriate pipeline,
    /// and execute it.</returns>
    public static PipelineStep<YarpRequestPipelineState> Choose(
        PipelineStep<YarpRequestPipelineState> notHandled,
        params PipelineStep<HandlerState<RequestSignature, PipelineStep<YarpRequestPipelineState>>>[] handlers)
        => HandlerPipeline.Choose(
            state => state.GetNominalRequestSignature(),
            notHandled,
            handlers);

    /// <summary>
    /// An operator that produces a <see cref="PipelineStep{YarpRequestPipelineState}"/> that executes a <see cref="PipelineStep{TState}"/>
    /// chosen by a handler pipeline that takes a <see cref="RequestSignature"/> as input.
    /// </summary>
    /// <param name="notHandled">Invoked if not handled.</param>
    /// <param name="handlers">
    /// The sequence of handlers, the outcome of which determines the pipeline.
    /// </param>
    /// <returns>A <see cref="PipelineStep{YarpRequestPipelineState}"/> which, when executed, will execute the handlers to choose the appropriate pipeline,
    /// and execute it.</returns>
    public static PipelineStep<YarpRequestPipelineState> Choose(
        PipelineStep<YarpRequestPipelineState> notHandled,
        params SyncPipelineStep<HandlerState<RequestSignature, PipelineStep<YarpRequestPipelineState>>>[] handlers)
        => HandlerPipeline.Choose(
            state => state.GetNominalRequestSignature(),
            notHandled,
            handlers);

    /// <summary>
    /// An operator that produces a <see cref="PipelineStep{YarpRequestPipelineState}"/> that executes a <see cref="PipelineStep{TState}"/>
    /// chosen by a handler pipeline that takes a <see cref="RequestSignature"/> as input.
    /// </summary>
    /// <param name="notHandled">Invoked if not handled.</param>
    /// <param name="handlers">
    /// The sequence of handlers, the outcome of which determines the pipeline.
    /// </param>
    /// <returns>A <see cref="PipelineStep{YarpRequestPipelineState}"/> which, when executed, will execute the handlers to choose the appropriate pipeline,
    /// and execute it.</returns>
    public static PipelineStep<YarpRequestPipelineState> Choose(
        SyncPipelineStep<YarpRequestPipelineState> notHandled,
        params PipelineStep<HandlerState<RequestSignature, SyncPipelineStep<YarpRequestPipelineState>>>[] handlers) =>
        HandlerPipeline.Choose(
            state => state.GetNominalRequestSignature(),
            notHandled,
            handlers);

    /// <summary>
    /// An operator that produces a <see cref="PipelineStep{YarpRequestPipelineState}"/> that executes a <see cref="PipelineStep{TState}"/>
    /// chosen by a handler pipeline that takes a <see cref="RequestSignature"/> as input.
    /// </summary>
    /// <param name="notHandled">Invoked if not handled.</param>
    /// <param name="handlers">
    /// The sequence of handlers, the outcome of which determines the pipeline.
    /// </param>
    /// <returns>A <see cref="PipelineStep{YarpRequestPipelineState}"/> which, when executed, will execute the handlers to choose the appropriate pipeline,
    /// and execute it.</returns>
    public static SyncPipelineStep<YarpRequestPipelineState> Choose(
        SyncPipelineStep<YarpRequestPipelineState> notHandled,
        params SyncPipelineStep<HandlerState<RequestSignature, SyncPipelineStep<YarpRequestPipelineState>>>[] handlers) =>
        HandlerPipeline.Choose(
            state => state.GetNominalRequestSignature(),
            notHandled,
            handlers);
}