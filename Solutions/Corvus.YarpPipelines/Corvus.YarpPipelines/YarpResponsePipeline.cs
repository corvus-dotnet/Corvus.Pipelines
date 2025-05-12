// <copyright file="YarpResponsePipeline.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Corvus.Pipelines;

namespace Corvus.YarpPipelines;

/// <summary>
/// A <see cref="Pipeline"/> for handling YARP response transforms.
/// </summary>
public static class YarpResponsePipeline
{
    /// <summary>
    /// Gets the an instance of a <see cref="PipelineStep{YarpResponsePipelineState}"/> that returns
    /// the current pipeline state (the Identity operator).
    /// </summary>
    public static PipelineStep<YarpResponsePipelineState> Current { get; } = Pipeline.Current<YarpResponsePipelineState>();

    /// <summary>
    /// Gets the an instance of a <see cref="PipelineStep{YarpResponsePipelineState}"/> that returns
    /// the current pipeline state (the Identity operator).
    /// </summary>
    public static SyncPipelineStep<YarpResponsePipelineState> CurrentSync { get; } = Pipeline.CurrentSync<YarpResponsePipelineState>();

    /// <summary>
    /// Builds an asynchronous pipeline of <see cref="PipelineStep{YarpResponsePipelineState}"/>.
    /// </summary>
    /// <param name="steps">The ordered array of steps in the pipeline.</param>
    /// <returns>A <see cref="PipelineStep{YarpResponsePipelineState}"/> that executes the pipeline.</returns>
    public static PipelineStep<YarpResponsePipelineState> Build(params PipelineStep<YarpResponsePipelineState>[] steps)
    {
        return Pipeline.Build(
            ctx => ctx.ShouldTerminatePipeline,
            steps);
    }

    /// <summary>
    /// Builds a synchronous pipeline of <see cref="PipelineStep{YarpResponsePipelineState}"/>.
    /// </summary>
    /// <param name="steps">The ordered array of steps in the pipeline.</param>
    /// <returns>A <see cref="PipelineStep{YarpResponsePipelineState}"/> that executes the pipeline.</returns>
    public static SyncPipelineStep<YarpResponsePipelineState> Build(params SyncPipelineStep<YarpResponsePipelineState>[] steps)
    {
        return Pipeline.Build(
            ctx => ctx.ShouldTerminatePipeline,
            steps);
    }

    /// <summary>
    /// An operator that produces a <see cref="PipelineStep{YarpResponsePipelineState}"/> that executes a <see cref="PipelineStep{YarpResponsePipelineState}"/>
    /// chosen by a <paramref name="selector"/> function.
    /// </summary>
    /// <param name="selector">The selector which takes the input state and chooses a pipeline with which to proceed.</param>
    /// <returns>A <see cref="PipelineStep{YarpResponsePipelineState}"/> which, when executed, will execute the selector to choose the appropriate pipeline,
    /// and execute it.</returns>
    public static PipelineStep<YarpResponsePipelineState> Choose(Func<YarpResponsePipelineState, PipelineStep<YarpResponsePipelineState>> selector)
         => Pipeline.Choose(selector);

    /// <summary>
    /// An operator that produces a <see cref="PipelineStep{YarpResponsePipelineState}"/> that executes a <see cref="PipelineStep{YarpResponsePipelineState}"/>
    /// chosen by a <paramref name="selector"/> function.
    /// </summary>
    /// <param name="selector">The selector which takes the input state and chooses a pipeline with which to proceed.</param>
    /// <returns>A <see cref="PipelineStep{YarpResponsePipelineState}"/> which, when executed, will execute the selector to choose the appropriate pipeline,
    /// and execute it.</returns>
    public static SyncPipelineStep<YarpResponsePipelineState> Choose(Func<YarpResponsePipelineState, SyncPipelineStep<YarpResponsePipelineState>> selector)
         => Pipeline.Choose(selector);
}