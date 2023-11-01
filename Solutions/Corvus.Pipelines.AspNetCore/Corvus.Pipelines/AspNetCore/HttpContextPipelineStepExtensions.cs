// <copyright file="HttpContextPipelineStepExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Pipelines.AspNetCore;

/// <summary>
/// Extensions for <see cref="PipelineStep{HttpContextPipelineState}"/>.
/// </summary>
public static class HttpContextPipelineStepExtensions
{
    /// <summary>
    /// An operator which provides the ability to retry a step which might fail.
    /// </summary>
    /// <param name="step">The step to execute.</param>
    /// <param name="shouldRetry">A predicate which determines if the step should be retried.</param>
    /// <param name="beforeRetry">An step to carry out before retrying. This is commonly an asynchronous delay, but can be used to return
    /// an updated version of the state before retyring the action (e.g. incrementing an execution count.</param>
    /// <returns>A <see cref="PipelineStep{HttpContextPipelineState}"/> which, when executed, will execute the step, choose the appropriate pipeline, based on the result,
    /// and execute it using the result.</returns>
    public static PipelineStep<HttpContextPipelineState> Retry(this PipelineStep<HttpContextPipelineState> step, Predicate<HttpContextPipelineState> shouldRetry, PipelineStep<HttpContextPipelineState>? beforeRetry = null)
    {
        return step.Retry<HttpContextPipelineState, HttpContextPipelineError>(shouldRetry, beforeRetry);
    }

    /// <summary>
    /// An operator which provides the ability to retry a step which might fail.
    /// </summary>
    /// <param name="step">The step to execute.</param>
    /// <param name="shouldRetry">A predicate which determines if the step should be retried.</param>
    /// <param name="beforeRetry">An step to carry out before retrying. This is commonly an asynchronous delay, but can be used to return
    /// an updated version of the state before retyring the action (e.g. incrementing an execution count.</param>
    /// <returns>A <see cref="PipelineStep{HttpContextPipelineState}"/> which, when executed, will execute the step, choose the appropriate pipeline, based on the result,
    /// and execute it using the result.</returns>
    public static SyncPipelineStep<HttpContextPipelineState> Retry(this SyncPipelineStep<HttpContextPipelineState> step, Predicate<HttpContextPipelineState> shouldRetry, SyncPipelineStep<HttpContextPipelineState>? beforeRetry = null)
    {
        return step.Retry<HttpContextPipelineState, HttpContextPipelineError>(shouldRetry, beforeRetry);
    }

    /// <summary>
    /// An operator which provides the ability to choose a step to run if the bound step fails.
    /// </summary>
    /// <param name="step">The step to execute.</param>
    /// <param name="onError">The step to execute if the step fails.</param>
    /// <returns>A <see cref="PipelineStep{HttpContextPipelineState}"/> which, when executed, will execute the step, and, if an error occurs,
    /// execute the error step before returning the final result.</returns>
    public static PipelineStep<HttpContextPipelineState> OnError(
        this PipelineStep<HttpContextPipelineState> step,
        PipelineStep<HttpContextPipelineState> onError)
    {
        return step.OnError<HttpContextPipelineState, HttpContextPipelineError>(onError);
    }

    /// <summary>
    /// An operator which provides the ability to choose a step to run if the bound step fails.
    /// </summary>
    /// <param name="step">The step to execute.</param>
    /// <param name="onError">The step to execute if the step fails.</param>
    /// <returns>A <see cref="PipelineStep{HttpContextPipelineState}"/> which, when executed, will execute the step, and, if an error occurs,
    /// execute the error step before returning the final result.</returns>
    public static SyncPipelineStep<HttpContextPipelineState> OnError(
        this SyncPipelineStep<HttpContextPipelineState> step,
        SyncPipelineStep<HttpContextPipelineState> onError)
    {
        return step.OnError<HttpContextPipelineState, HttpContextPipelineError>(onError);
    }

    /// <summary>
    /// An operator which provides the ability to choose a step to run if the bound step fails.
    /// </summary>
    /// <param name="step">The step to execute.</param>
    /// <param name="onError">The step to execute if the step fails.</param>
    /// <returns>A <see cref="PipelineStep{HttpContextPipelineState}"/> which, when executed, will execute the step, and, if an error occurs,
    /// execute the error step before returning the final result.</returns>
    public static PipelineStep<HttpContextPipelineState> OnError(
        this PipelineStep<HttpContextPipelineState> step,
        SyncPipelineStep<HttpContextPipelineState> onError)
    {
        return step.OnError<HttpContextPipelineState, HttpContextPipelineError>(onError);
    }
}