// <copyright file="YarpPipelineState.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using Corvus.Pipelines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Yarp.ReverseProxy.Transforms;

namespace Corvus.YarpPipelines;

/// <summary>
/// The state for processing a YARP transform.
/// </summary>
/// <remarks>
/// The steps in the pipe can inspect and modify the <see cref="RequestTransformContext"/>,
/// then choose to either <see cref="Continue()"/> processing, <see cref="TerminateAndForward()"/> - passing
/// the <see cref="RequestTransformContext"/> on to YARP for forwarding to the appropriate endpoint, or
/// <see cref="TerminateWith(NonForwardedResponseDetails)"/> a specific response code, headers and/or body.
/// </remarks>
public readonly struct YarpPipelineState :
    ICanFail<YarpPipelineState, YarpPipelineError>,
    ICancellable<YarpPipelineState>,
    ILoggable
{
    private readonly NonForwardedResponseDetails responseDetails;
    private readonly TransformState pipelineState;
    private readonly YarpPipelineError errorDetails;

    private YarpPipelineState(RequestTransformContext requestTransformContext, NonForwardedResponseDetails responseDetails, TransformState pipelineState, PipelineStepStatus executionStatus, YarpPipelineError errorDetails, int failureCount, ILogger logger, CancellationToken cancellationToken)
    {
        this.RequestTransformContext = requestTransformContext;
        this.responseDetails = responseDetails;
        this.pipelineState = pipelineState;
        this.errorDetails = errorDetails;
        this.ExecutionStatus = executionStatus;
        this.FailureCount = failureCount;
        this.Logger = logger;
        this.CancellationToken = cancellationToken;
    }

    private enum TransformState
    {
        Continue,
        Terminate,
        TerminateAndForward,
    }

    /// <summary>
    /// Gets the <see cref="RequestTransformContext"/> for the current request.
    /// </summary>
    public RequestTransformContext RequestTransformContext { get; }

    /// <inheritdoc/>
    public PipelineStepStatus ExecutionStatus { get; }

    /// <inheritdoc/>
    public int FailureCount { get; }

    /// <inheritdoc/>
    public CancellationToken CancellationToken { get; }

    /// <inheritdoc/>
    public ILogger Logger { get; }

    /// <summary>
    /// Gets a value indicating whether the pipeline should be terminated. This is used by the
    /// terminate predicate for the <see cref="YarpPipeline"/>.
    /// </summary>
    internal bool ShouldTerminatePipeline => this.pipelineState != TransformState.Continue;

    /// <summary>
    /// Gets an instance of the <see cref="YarpPipelineState"/> for a particular
    /// <see cref="RequestTransformContext"/>.
    /// </summary>
    /// <param name="requestTransformContext">The <see cref="RequestTransformContext"/> with which to
    /// initialize the <see cref="YarpPipelineState"/>.</param>
    /// <param name="logger">The logger to use for the context.</param>
    /// <returns>The initialized instance.</returns>
    /// <remarks>
    /// You can explicitly provide a logger; if you don't it will be resolved from the service provider. If
    /// no logger is available in the service provider, it will fall back to the <see cref="NullLogger{YarpPipelineState}"/>.
    /// </remarks>
    public static YarpPipelineState For(RequestTransformContext requestTransformContext, ILogger? logger = null)
    {
        return new(requestTransformContext, default, TransformState.Continue, default, default, 0, logger ?? requestTransformContext.HttpContext.RequestServices?.GetService<ILogger<YarpPipelineState>>() ?? NullLogger<YarpPipelineState>.Instance, default);
    }

    /// <summary>
    /// Returns a <see cref="YarpPipelineState"/> instance which will terminate the pipeline
    /// with the given response details. The request will not be forwarded to the endpoint.
    /// </summary>
    /// <param name="responseDetails">The details of the response to return.</param>
    /// <returns>The terminating <see cref="YarpPipelineState"/>.</returns>
    public YarpPipelineState TerminateWith(NonForwardedResponseDetails responseDetails)
    {
        this.Logger.LogInformation(Pipeline.EventIds.Result, "terminate-with");
        return new(this.RequestTransformContext, responseDetails, TransformState.Terminate, this.ExecutionStatus, this.errorDetails, this.FailureCount, this.Logger, this.CancellationToken);
    }

    /// <summary>
    /// Returns a <see cref="YarpPipelineState"/> instance which will terminate the pipeline
    /// and allow the request to be forwarded to the appropriate endpoint.
    /// </summary>
    /// <returns>The terminating <see cref="YarpPipelineState"/>.</returns>
    public YarpPipelineState TerminateAndForward()
    {
        this.Logger.LogInformation(Pipeline.EventIds.Result, "terminate-and-forward");
        return new(this.RequestTransformContext, default, TransformState.TerminateAndForward, this.ExecutionStatus, this.errorDetails, this.FailureCount, this.Logger, this.CancellationToken);
    }

    /// <summary>
    /// Returns a <see cref="YarpPipelineState"/> instance that will continue processing the pipeline.
    /// </summary>
    /// <returns>The non-terminating <see cref="YarpPipelineState"/>.</returns>
    /// <remarks>
    /// <para>
    /// Note that if this is the last step in the pipeline, it will allow the request to be forwarded to
    /// the appropriate endpoint.
    /// </para>
    /// </remarks>
    public YarpPipelineState Continue()
    {
        this.Logger.LogInformation(Pipeline.EventIds.Result, "continue");
        return new(this.RequestTransformContext, default, TransformState.Continue, this.ExecutionStatus, this.errorDetails, this.FailureCount, this.Logger, this.CancellationToken);
    }

    /// <summary>
    /// Determines whether the result should be forwarded through YARP,
    /// or whether we should build a local response using the resulting response details.
    /// </summary>
    /// <param name="responseDetails">The response details to use if the result should not be forwarded.</param>
    /// <returns><see langword="true"/> if the result should be forwarded. If <see langword="false"/> then
    /// the <paramref name="responseDetails"/> will be set an can be used to build a local response.</returns>
    public bool ShouldForward(out NonForwardedResponseDetails responseDetails)
    {
        if (this.pipelineState == TransformState.Continue || this.pipelineState == TransformState.TerminateAndForward)
        {
            responseDetails = default;
            return true;
        }

        responseDetails = this.responseDetails;
        return false;
    }

    /// <inheritdoc/>
    public bool TryGetErrorDetails([NotNullWhen(true)] out YarpPipelineError errorDetails)
    {
        errorDetails = this.errorDetails;
        return this.ExecutionStatus != PipelineStepStatus.Success;
    }

    /// <inheritdoc/>
    public YarpPipelineState PermanentFailure(YarpPipelineError errorDetails)
    {
        return new YarpPipelineState(
            this.RequestTransformContext,
            this.responseDetails,
            this.pipelineState,
            PipelineStepStatus.PermanentFailure,
            errorDetails,
            this.FailureCount + 1,
            this.Logger,
            this.CancellationToken);
    }

    /// <inheritdoc/>
    public YarpPipelineState TransientFailure(YarpPipelineError errorDetails)
    {
        return new YarpPipelineState(
            this.RequestTransformContext,
            this.responseDetails,
            this.pipelineState,
            PipelineStepStatus.TransientFailure,
            errorDetails,
            this.FailureCount + 1,
            this.Logger,
            this.CancellationToken);
    }

    /// <inheritdoc/>
    public YarpPipelineState Success()
    {
        return new YarpPipelineState(
            this.RequestTransformContext,
            this.responseDetails,
            this.pipelineState,
            PipelineStepStatus.Success,
            default,
            this.FailureCount,
            this.Logger,
            this.CancellationToken);
    }

    /// <inheritdoc/>
    public YarpPipelineState ResetFailureState()
    {
        return new YarpPipelineState(
            this.RequestTransformContext,
            this.responseDetails,
            this.pipelineState,
            PipelineStepStatus.Success,
            default,
            0,
            this.Logger,
            this.CancellationToken);
    }

    /// <inheritdoc/>
    public YarpPipelineState PrepareToRetry()
    {
        return new YarpPipelineState(
            this.RequestTransformContext,
            this.responseDetails,
            this.pipelineState,
            PipelineStepStatus.Success,
            default,
            this.FailureCount,
            this.Logger,
            this.CancellationToken);
    }

    /// <inheritdoc/>
    public YarpPipelineState WithCancellationToken(CancellationToken cancellationToken)
    {
        return new YarpPipelineState(
            this.RequestTransformContext,
            this.responseDetails,
            this.pipelineState,
            this.ExecutionStatus,
            this.errorDetails,
            this.FailureCount,
            this.Logger,
            cancellationToken);
    }
}