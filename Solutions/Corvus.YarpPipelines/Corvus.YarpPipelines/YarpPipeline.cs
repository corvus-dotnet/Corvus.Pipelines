// <copyright file="YarpPipeline.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;
using Corvus.Pipelines;
using Corvus.Pipelines.Handlers;

using Microsoft.Extensions.Logging;

namespace Corvus.YarpPipelines;

/// <summary>
/// A <see cref="Pipeline"/> for handling YARP transforms.
/// </summary>
public static class YarpPipeline
{
    /// <summary>
    /// Gets the an instance of a <see cref="PipelineStep{YarpPipelineState}"/> that returns
    /// the current pipeline state (the Identity operator).
    /// </summary>
    public static PipelineStep<YarpPipelineState> Current { get; } = Pipeline.Current<YarpPipelineState>();

    /// <summary>
    /// Gets the an instance of a <see cref="PipelineStep{YarpPipelineState}"/> that returns
    /// the current pipeline state (the Identity operator).
    /// </summary>
    public static SyncPipelineStep<YarpPipelineState> CurrentSync { get; } = Pipeline.CurrentSync<YarpPipelineState>();

    /// <summary>
    /// Builds an asynchronous pipeline of <see cref="PipelineStep{YarpPipelineState}"/>.
    /// </summary>
    /// <param name="steps">The ordered array of steps in the pipeline.</param>
    /// <returns>A <see cref="PipelineStep{YarpPipelineState}"/> that executes the pipeline.</returns>
    public static PipelineStep<YarpPipelineState> Build(params PipelineStep<YarpPipelineState>[] steps)
    {
        return Pipeline.Build(
            ctx => ctx.ShouldTerminatePipeline,
            steps);
    }

    /// <summary>
    /// Builds a synchronous pipeline of <see cref="PipelineStep{YarpPipelineState}"/>.
    /// </summary>
    /// <param name="steps">The ordered array of steps in the pipeline.</param>
    /// <returns>A <see cref="PipelineStep{YarpPipelineState}"/> that executes the pipeline.</returns>
    public static SyncPipelineStep<YarpPipelineState> Build(params SyncPipelineStep<YarpPipelineState>[] steps)
    {
        return Pipeline.Build(
            ctx => ctx.ShouldTerminatePipeline,
            steps);
    }

    /// <summary>
    /// An operator that produces a <see cref="PipelineStep{YarpPipelineState}"/> that executes a <see cref="PipelineStep{YarpPipelineState}"/>
    /// chosen by a <paramref name="selector"/> function.
    /// </summary>
    /// <param name="selector">The selector which takes the input state and chooses a pipeline with which to proceed.</param>
    /// <returns>A <see cref="PipelineStep{YarpPipelineState}"/> which, when executed, will execute the selector to choose the appropriate pipeline,
    /// and execute it.</returns>
    public static PipelineStep<YarpPipelineState> Choose(Func<YarpPipelineState, PipelineStep<YarpPipelineState>> selector)
         => Pipeline.Choose(selector);

    /// <summary>
    /// An operator that produces a <see cref="PipelineStep{YarpPipelineState}"/> that executes a <see cref="PipelineStep{YarpPipelineState}"/>
    /// chosen by a <paramref name="selector"/> function.
    /// </summary>
    /// <param name="selector">The selector which takes the input state and chooses a pipeline with which to proceed.</param>
    /// <returns>A <see cref="PipelineStep{YarpPipelineState}"/> which, when executed, will execute the selector to choose the appropriate pipeline,
    /// and execute it.</returns>
    public static SyncPipelineStep<YarpPipelineState> Choose(Func<YarpPipelineState, SyncPipelineStep<YarpPipelineState>> selector)
         => Pipeline.Choose(selector);

    /// <summary>
    /// An operator that produces a <see cref="PipelineStep{YarpPipelineState}"/> that executes a <see cref="PipelineStep{TState}"/>
    /// chosen by a handler pipeline that takes a <see cref="RequestSignature"/> as input.
    /// </summary>
    /// <param name="notHandled">Invoked if not handled.</param>
    /// <param name="handlers">
    /// The sequence of handlers, the outcome of which determines the pipeline.
    /// </param>
    /// <returns>A <see cref="PipelineStep{YarpPipelineState}"/> which, when executed, will execute the handlers to choose the appropriate pipeline,
    /// and execute it.</returns>
    public static PipelineStep<YarpPipelineState> Choose(
        PipelineStep<YarpPipelineState> notHandled,
        params PipelineStep<HandlerState<RequestSignature, PipelineStep<YarpPipelineState>>>[] handlers)
        => HandlerPipeline.Choose(
            state => state.GetNominalRequestSignature(),
            notHandled,
            handlers);

    /// <summary>
    /// An operator that produces a <see cref="PipelineStep{YarpPipelineState}"/> that executes a <see cref="PipelineStep{TState}"/>
    /// chosen by a handler pipeline that takes a <see cref="RequestSignature"/> as input.
    /// </summary>
    /// <param name="notHandled">Invoked if not handled.</param>
    /// <param name="handlers">
    /// The sequence of handlers, the outcome of which determines the pipeline.
    /// </param>
    /// <returns>A <see cref="PipelineStep{YarpPipelineState}"/> which, when executed, will execute the handlers to choose the appropriate pipeline,
    /// and execute it.</returns>
    public static PipelineStep<YarpPipelineState> Choose(
        PipelineStep<YarpPipelineState> notHandled,
        params SyncPipelineStep<HandlerState<RequestSignature, PipelineStep<YarpPipelineState>>>[] handlers)
        => HandlerPipeline.Choose(
            state => state.GetNominalRequestSignature(),
            notHandled,
            handlers);

    /// <summary>
    /// An operator that produces a <see cref="PipelineStep{YarpPipelineState}"/> that executes a <see cref="PipelineStep{TState}"/>
    /// chosen by a handler pipeline that takes a <see cref="RequestSignature"/> as input.
    /// </summary>
    /// <param name="notHandled">Invoked if not handled.</param>
    /// <param name="handlers">
    /// The sequence of handlers, the outcome of which determines the pipeline.
    /// </param>
    /// <returns>A <see cref="PipelineStep{YarpPipelineState}"/> which, when executed, will execute the handlers to choose the appropriate pipeline,
    /// and execute it.</returns>
    public static PipelineStep<YarpPipelineState> Choose(
        SyncPipelineStep<YarpPipelineState> notHandled,
        params PipelineStep<HandlerState<RequestSignature, SyncPipelineStep<YarpPipelineState>>>[] handlers) =>
        HandlerPipeline.Choose(
            state => state.GetNominalRequestSignature(),
            notHandled,
            handlers);

    /// <summary>
    /// An operator that produces a <see cref="PipelineStep{YarpPipelineState}"/> that executes a <see cref="PipelineStep{TState}"/>
    /// chosen by a handler pipeline that takes a <see cref="RequestSignature"/> as input.
    /// </summary>
    /// <param name="notHandled">Invoked if not handled.</param>
    /// <param name="handlers">
    /// The sequence of handlers, the outcome of which determines the pipeline.
    /// </param>
    /// <returns>A <see cref="PipelineStep{YarpPipelineState}"/> which, when executed, will execute the handlers to choose the appropriate pipeline,
    /// and execute it.</returns>
    public static SyncPipelineStep<YarpPipelineState> Choose(
        SyncPipelineStep<YarpPipelineState> notHandled,
        params SyncPipelineStep<HandlerState<RequestSignature, SyncPipelineStep<YarpPipelineState>>>[] handlers) =>
        HandlerPipeline.Choose(
            state => state.GetNominalRequestSignature(),
            notHandled,
            handlers);
}