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
    IErrorDetails<HttpContextPipelineError>,
    ICancellable<HttpContextPipelineState>,
    ILoggable
{
    private HttpContextPipelineState(HttpContext httpContext, RequestState pipelineState, PipelineStepStatus executionStatus, HttpContextPipelineError errorDetails, ILogger logger, CancellationToken cancellationToken)
    {
        this.HttpContext = httpContext;
        this.PipelineState = pipelineState;
        this.ErrorDetails = errorDetails;
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
    public HttpContext HttpContext { get; init; }

    /// <inheritdoc/>
    public PipelineStepStatus ExecutionStatus { get; init; }

    /// <inheritdoc/>
    public CancellationToken CancellationToken { get; init; }

    /// <inheritdoc/>
    public ILogger Logger { get; init; }

    /// <inheritdoc/>
    public HttpContextPipelineError ErrorDetails { get; init; }

    /// <summary>
    /// Gets a value indicating whether the pipeline should be terminated. This is used by the
    /// terminate predicate for the <see cref="HttpContextPipeline"/>.
    /// </summary>
    internal bool ShouldTerminatePipeline => this.PipelineState != RequestState.Continue || this.CancellationToken.IsCancellationRequested;

    private RequestState PipelineState { get; init; }

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
        return this with { PipelineState = RequestState.Terminate };
    }

    /// <summary>
    /// Returns a <see cref="HttpContextPipelineState"/> instance that will continue processing the pipeline.
    /// </summary>
    /// <returns>The non-terminating <see cref="HttpContextPipelineState"/>.</returns>
    public HttpContextPipelineState Continue()
    {
        this.Logger.LogInformation(Pipeline.EventIds.Result, "continue");
        return this with { PipelineState = RequestState.Continue };
    }
}