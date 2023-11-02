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
    /// Create a named step.
    /// </summary>
    /// <param name="step">The step.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>A named step.</returns>
    public static NamedSyncPipelineStep<HttpContextPipelineState> Name(this SyncPipelineStep<HttpContextPipelineState> step, [CallerArgumentExpression(nameof(step))] string? name = null) => new(name!, step);

    /// <summary>
    /// Create a named step.
    /// </summary>
    /// <param name="step">The step.</param>
    /// <param name="name">The name of the step.</param>
    /// <returns>A named step.</returns>
    public static NamedPipelineStep<HttpContextPipelineState> Name(this PipelineStep<HttpContextPipelineState> step, [CallerArgumentExpression(nameof(step))] string? name = null) => new(name!, step);

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
    /// Builds an asynchronous pipeline of <see cref="PipelineStep{HttpContextPipelineState}"/>.
    /// </summary>
    /// <param name="scopeName">The scope name for the pipeline.</param>
    /// <param name="level">The level at which to surface step entry/exit logging.</param>
    /// <param name="steps">The ordered array of steps in the pipeline.</param>
    /// <returns>A <see cref="PipelineStep{HttpContextPipelineState}"/> that executes the pipeline.</returns>
    public static PipelineStep<HttpContextPipelineState> Build(string scopeName, LogLevel level, params NamedPipelineStep<HttpContextPipelineState>[] steps)
    {
        return Pipeline.Build(
            ctx => ctx.ShouldTerminatePipeline,
            scopeName,
            level,
            steps);
    }

    /// <summary>
    /// Builds a synchronous pipeline of <see cref="PipelineStep{HttpContextPipelineState}"/>.
    /// </summary>
    /// <param name="scopeName">The scope name for the pipeline.</param>
    /// <param name="level">The level at which to surface step entry/exit logging.</param>
    /// <param name="steps">The ordered array of steps in the pipeline.</param>
    /// <returns>A <see cref="PipelineStep{HttpContextPipelineState}"/> that executes the pipeline.</returns>
    public static SyncPipelineStep<HttpContextPipelineState> Build(string scopeName, LogLevel level, params NamedSyncPipelineStep<HttpContextPipelineState>[] steps)
    {
        return Pipeline.Build(
            ctx => ctx.ShouldTerminatePipeline,
            scopeName,
            level,
            steps);
    }
}