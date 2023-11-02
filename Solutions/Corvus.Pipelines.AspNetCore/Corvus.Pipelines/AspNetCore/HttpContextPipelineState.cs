// <copyright file="HttpContextPipelineState.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Corvus.Pipelines.AspNetCore;

/// <summary>
/// The state for processing a YARP transform.
/// </summary>
/// <remarks>
/// The steps in the pipe can inspect and modify the <see cref="Microsoft.AspNetCore.Http.HttpContext"/>,
/// then choose to either <see cref="Continue()"/> processing, or <see cref="Complete()"/> the request.
/// </remarks>
public readonly struct HttpContextPipelineState :
    ICanFail,
    ICancellable<HttpContextPipelineState>,
    ILoggable
{
    private readonly RequestState pipelineState;
    private readonly HttpContextPipelineError errorDetails;

    private HttpContextPipelineState(HttpContext httpContext, RequestState pipelineState, PipelineStepStatus executionStatus, HttpContextPipelineError errorDetails, ILogger logger, CancellationToken cancellationToken)
    {
        this.HttpContext = httpContext;
        this.pipelineState = pipelineState;
        this.errorDetails = errorDetails;
        this.ExecutionStatus = executionStatus;
        this.Logger = logger;
        this.CancellationToken = cancellationToken;
    }

    private enum RequestState
    {
        Continue,
        Terminate,
    }

    /// <summary>
    /// Gets the <see cref="HttpContext"/> for the current request.
    /// </summary>
    public HttpContext HttpContext { get; }

    /// <inheritdoc/>
    public PipelineStepStatus ExecutionStatus { get; }

    /// <inheritdoc/>
    public CancellationToken CancellationToken { get; }

    /// <inheritdoc/>
    public ILogger Logger { get; }

    /// <summary>
    /// Gets a value indicating whether the pipeline should be terminated. This is used by the
    /// terminate predicate for the <see cref="HttpContextPipeline"/>.
    /// </summary>
    internal bool ShouldTerminatePipeline => this.pipelineState != RequestState.Continue || this.CancellationToken.IsCancellationRequested;

    /// <summary>
    /// Gets an instance of the <see cref="HttpContextPipelineState"/> for a particular
    /// <see cref="HttpContext"/>.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> with which to
    /// initialize the <see cref="HttpContextPipelineState"/>.</param>
    /// <param name="logger">The logger to use for the context.</param>
    /// <param name="cancellationToken">The cancellation token to use for the context.</param>
    /// <returns>The initialized instance.</returns>
    /// <remarks>
    /// You can explicitly provide a logger; if you don't it will be resolved from the service provider. If
    /// no logger is available in the service provider, it will fall back to the <see cref="NullLogger"/>.
    /// </remarks>
    public static HttpContextPipelineState For(HttpContext httpContext, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        return new(httpContext, RequestState.Continue, default, default, logger ?? httpContext.RequestServices?.GetService<ILogger>() ?? NullLogger.Instance, cancellationToken);
    }

    /// <summary>
    /// Returns a <see cref="HttpContextPipelineState"/> instance which will terminate the pipeline.
    /// </summary>
    /// <returns>The terminating <see cref="HttpContextPipelineState"/>.</returns>
    public HttpContextPipelineState Complete()
    {
        this.Logger.LogInformation(Pipeline.EventIds.Result, "complete");
        return new(this.HttpContext, RequestState.Terminate, this.ExecutionStatus, this.errorDetails, this.Logger, this.CancellationToken);
    }

    /// <summary>
    /// Returns a <see cref="HttpContextPipelineState"/> instance that will continue processing the pipeline.
    /// </summary>
    /// <returns>The non-terminating <see cref="HttpContextPipelineState"/>.</returns>
    public HttpContextPipelineState Continue()
    {
        this.Logger.LogInformation(Pipeline.EventIds.Result, "continue");
        return new(this.HttpContext, RequestState.Continue, this.ExecutionStatus, this.errorDetails, this.Logger, this.CancellationToken);
    }

    /// <summary>
    /// Try to get the error details for the current state.
    /// </summary>
    /// <param name="errorDetails">The error details, if any.</param>
    /// <returns><see langword="true"/> if error details were available, otherwise false.</returns>
    public bool TryGetErrorDetails([NotNullWhen(true)] out HttpContextPipelineError errorDetails)
    {
        errorDetails = this.errorDetails;
        return this.ExecutionStatus != PipelineStepStatus.Success;
    }

    /// <summary>
    /// Update the state with a permanent failure.
    /// </summary>
    /// <param name="errorDetails">The error details associated with the failure.</param>
    /// <returns>The updated state.</returns>
    public HttpContextPipelineState PermanentFailure(HttpContextPipelineError errorDetails)
    {
        return new HttpContextPipelineState(
            this.HttpContext,
            this.pipelineState,
            PipelineStepStatus.PermanentFailure,
            errorDetails,
            this.Logger,
            this.CancellationToken);
    }

    /// <summary>
    /// Update the state with a transient failure.
    /// </summary>
    /// <param name="errorDetails">The error details associated with the failure.</param>
    /// <returns>The updated state.</returns>
    public HttpContextPipelineState TransientFailure(HttpContextPipelineError errorDetails)
    {
        return new HttpContextPipelineState(
            this.HttpContext,
            this.pipelineState,
            PipelineStepStatus.TransientFailure,
            errorDetails,
            this.Logger,
            this.CancellationToken);
    }

    /// <summary>
    /// Update the state for a successful execution.
    /// </summary>
    /// <returns>The updated state.</returns>
    public HttpContextPipelineState Success()
    {
        return new HttpContextPipelineState(
            this.HttpContext,
            this.pipelineState,
            PipelineStepStatus.Success,
            default,
            this.Logger,
            this.CancellationToken);
    }

    /// <inheritdoc/>
    public HttpContextPipelineState WithCancellationToken(CancellationToken cancellationToken)
    {
        return new HttpContextPipelineState(
            this.HttpContext,
            this.pipelineState,
            this.ExecutionStatus,
            this.errorDetails,
            this.Logger,
            cancellationToken);
    }
}