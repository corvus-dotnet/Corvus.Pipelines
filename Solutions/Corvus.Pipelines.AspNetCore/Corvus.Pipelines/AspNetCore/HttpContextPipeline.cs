// <copyright file="HttpContextPipeline.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Corvus.Pipelines.AspNetCore;

/// <summary>
/// A <see cref="Pipeline"/> for handling YARP transforms.
/// </summary>
public static class HttpContextPipeline
{
    /// <summary>
    /// Gets the an instance of a <see cref="PipelineStep{HttpContextPipelineState}"/> that returns
    /// the current pipeline state (the Identity operator).
    /// </summary>
    public static PipelineStep<HttpContextPipelineState> Current { get; } = Pipeline.Current<HttpContextPipelineState>();

    /// <summary>
    /// Gets the an instance of a <see cref="PipelineStep{HttpContextPipelineState}"/> that returns
    /// the current pipeline state (the Identity operator).
    /// </summary>
    public static SyncPipelineStep<HttpContextPipelineState> CurrentSync { get; } = Pipeline.CurrentSync<HttpContextPipelineState>();

    /// <summary>
    /// Builds an asynchronous pipeline of <see cref="PipelineStep{HttpContextPipelineState}"/>.
    /// </summary>
    /// <param name="steps">The ordered array of steps in the pipeline.</param>
    /// <returns>A <see cref="PipelineStep{HttpContextPipelineState}"/> that executes the pipeline.</returns>
    public static PipelineStep<HttpContextPipelineState> Build(params PipelineStep<HttpContextPipelineState>[] steps)
    {
        return Pipeline.Build(
            ctx => ctx.ShouldTerminatePipeline,
            steps);
    }

    /// <summary>
    /// Builds a synchronous pipeline of <see cref="PipelineStep{HttpContextPipelineState}"/>.
    /// </summary>
    /// <param name="steps">The ordered array of steps in the pipeline.</param>
    /// <returns>A <see cref="PipelineStep{HttpContextPipelineState}"/> that executes the pipeline.</returns>
    public static SyncPipelineStep<HttpContextPipelineState> Build(params SyncPipelineStep<HttpContextPipelineState>[] steps)
    {
        return Pipeline.Build(
            ctx => ctx.ShouldTerminatePipeline,
            steps);
    }

    /// <summary>
    /// An operator that produces a <see cref="PipelineStep{HttpContextPipelineState}"/> that executes a <see cref="PipelineStep{HttpContextPipelineState}"/>
    /// chosen by a <paramref name="selector"/> function.
    /// </summary>
    /// <param name="selector">The selector which takes the input state and chooses a pipeline with which to proceed.</param>
    /// <returns>A <see cref="PipelineStep{HttpContextPipelineState}"/> which, when executed, will execute the selector to choose the appropriate pipeline,
    /// and execute it.</returns>
    public static PipelineStep<HttpContextPipelineState> Choose(Func<HttpContextPipelineState, PipelineStep<HttpContextPipelineState>> selector)
         => Pipeline.Choose(selector);

    /// <summary>
    /// An operator that produces a <see cref="PipelineStep{HttpContextPipelineState}"/> that executes a <see cref="PipelineStep{HttpContextPipelineState}"/>
    /// chosen by a <paramref name="selector"/> function.
    /// </summary>
    /// <param name="selector">The selector which takes the input state and chooses a pipeline with which to proceed.</param>
    /// <returns>A <see cref="PipelineStep{HttpContextPipelineState}"/> which, when executed, will execute the selector to choose the appropriate pipeline,
    /// and execute it.</returns>
    public static SyncPipelineStep<HttpContextPipelineState> Choose(Func<HttpContextPipelineState, SyncPipelineStep<HttpContextPipelineState>> selector)
         => Pipeline.Choose(selector);
}