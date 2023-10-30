// <copyright file="YarpPipelineStepExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Corvus.Pipelines;

namespace Corvus.YarpPipelines;

/// <summary>
/// Extensions for <see cref="PipelineStep{YarpPipelineState}"/>.
/// </summary>
public static class YarpPipelineStepExtensions
{
    /// <summary>
    /// An operator which provides the ability to retry a step which might fail.
    /// </summary>
    /// <param name="step">The step to execute.</param>
    /// <param name="shouldRetry">A predicate which determines if the step should be retried.</param>
    /// <param name="beforeRetry">An step to carry out before retrying. This is commonly an asynchronous delay, but can be used to return
    /// an updated version of the state before retyring the action (e.g. incrementing an execution count.</param>
    /// <returns>A <see cref="PipelineStep{YarpPipelineState}"/> which, when executed, will execute the step, choose the appropriate pipeline, based on the result,
    /// and execute it using the result.</returns>
    public static PipelineStep<YarpPipelineState> Retry(this PipelineStep<YarpPipelineState> step, Predicate<YarpPipelineState> shouldRetry, PipelineStep<YarpPipelineState>? beforeRetry = null)
    {
        return step.Retry<YarpPipelineState, YarpPipelineError>(shouldRetry, beforeRetry);
    }

    /// <summary>
    /// An operator which provides the ability to choose a step to run if the bound step fails.
    /// </summary>
    /// <param name="step">The step to execute.</param>
    /// <param name="onError">The step to execute if the step fails.</param>
    /// <returns>A <see cref="PipelineStep{YarpPipelineState}"/> which, when executed, will execute the step, and, if an error occurs,
    /// execute the error step before returning the final result.</returns>
    public static PipelineStep<YarpPipelineState> OnError(
        this PipelineStep<YarpPipelineState> step,
        PipelineStep<YarpPipelineState> onError)
    {
        return step.OnError<YarpPipelineState, YarpPipelineError>(onError);
    }

    /// <summary>
    /// An operator which provides the ability to choose a step to run if the bound step fails.
    /// </summary>
    /// <param name="step">The step to execute.</param>
    /// <param name="onError">The step to execute if the step fails.</param>
    /// <returns>A <see cref="PipelineStep{YarpPipelineState}"/> which, when executed, will execute the step, and, if an error occurs,
    /// execute the error step before returning the final result.</returns>
    public static PipelineStep<YarpPipelineState> OnError(
        this PipelineStep<YarpPipelineState> step,
        SyncPipelineStep<YarpPipelineState> onError)
    {
        return step.OnError<YarpPipelineState, YarpPipelineError>(onError);
    }
}