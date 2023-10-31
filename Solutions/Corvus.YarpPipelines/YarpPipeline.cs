// <copyright file="YarpPipeline.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Corvus.Pipelines;
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
    /// Create a named step.
    /// </summary>
    /// <param name="name">The name of the step.</param>
    /// <param name="step">The step.</param>
    /// <returns>A named step.</returns>
    public static NamedSyncPipelineStep<YarpPipelineState> Step(string name, SyncPipelineStep<YarpPipelineState> step) => new(name, step);

    /// <summary>
    /// Create a named step.
    /// </summary>
    /// <param name="name">The name of the step.</param>
    /// <param name="step">The step.</param>
    /// <returns>A named step.</returns>
    public static NamedPipelineStep<YarpPipelineState> Step(string name, PipelineStep<YarpPipelineState> step) => new(name, step);

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
    /// Builds an asynchronous pipeline of <see cref="PipelineStep{YarpPipelineState}"/>.
    /// </summary>
    /// <param name="scopeName">The scope name for the pipeline.</param>
    /// <param name="level">The level at which to surface step entry/exit logging.</param>
    /// <param name="steps">The ordered array of steps in the pipeline.</param>
    /// <returns>A <see cref="PipelineStep{YarpPipelineState}"/> that executes the pipeline.</returns>
    public static PipelineStep<YarpPipelineState> BuildWithStepLogging(string scopeName, LogLevel level, params NamedPipelineStep<YarpPipelineState>[] steps)
    {
        return Pipeline.BuildWithStepLogging(
            ctx => ctx.ShouldTerminatePipeline,
            scopeName,
            level,
            steps);
    }

    /// <summary>
    /// Builds a synchronous pipeline of <see cref="PipelineStep{YarpPipelineState}"/>.
    /// </summary>
    /// <param name="scopeName">The scope name for the pipeline.</param>
    /// <param name="level">The level at which to surface step entry/exit logging.</param>
    /// <param name="steps">The ordered array of steps in the pipeline.</param>
    /// <returns>A <see cref="PipelineStep{YarpPipelineState}"/> that executes the pipeline.</returns>
    public static PipelineStep<YarpPipelineState> BuildWithStepLogging(string scopeName, LogLevel level, params NamedSyncPipelineStep<YarpPipelineState>[] steps)
    {
        return Pipeline.BuildWithStepLogging(
            ctx => ctx.ShouldTerminatePipeline,
            scopeName,
            level,
            steps);
    }
}