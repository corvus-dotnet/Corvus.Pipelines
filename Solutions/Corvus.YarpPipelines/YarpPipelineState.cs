// <copyright file="YarpPipelineState.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Corvus.Pipelines;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Yarp.ReverseProxy.Transforms;

namespace Corvus.YarpPipelines;

/// <summary>
/// The state for processing a YARP transform.
/// </summary>
/// <remarks>
/// The steps in the pipe can inspect and modify the <see cref="Yarp.ReverseProxy.Transforms.RequestTransformContext"/>,
/// then choose to either <see cref="Continue()"/> processing, <see cref="TerminateAndForward()"/> - passing
/// the <see cref="Yarp.ReverseProxy.Transforms.RequestTransformContext"/> on to YARP for forwarding to the appropriate endpoint, or
/// <see cref="TerminateWith(NonForwardedResponseDetails)"/> a specific response code, headers and/or body.
/// </remarks>
public readonly struct YarpPipelineState :
    ICancellable<YarpPipelineState>,
    ILoggable,
    IErrorDetails<YarpPipelineError>
{
    private YarpPipelineState(RequestTransformContext requestTransformContext, in NonForwardedResponseDetails responseDetails, TransformState pipelineState, PipelineStepStatus executionStatus, in YarpPipelineError errorDetails, ILogger logger, CancellationToken cancellationToken)
    {
        this.RequestTransformContext = requestTransformContext;
        this.ResponseDetails = responseDetails;
        this.PipelineState = pipelineState;
        this.ErrorDetails = errorDetails;
        this.ExecutionStatus = executionStatus;
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
    /// Gets the header for the current request.
    /// </summary>
    public IHeaderDictionary Headers => this.RequestTransformContext.HttpContext.Request.Headers;

    /// <summary>
    /// Gets the <see cref="IFeatureCollection"/> for the current request.
    /// </summary>
    public IFeatureCollection Features => this.RequestTransformContext.HttpContext.Features;

    /// <summary>
    /// Gets a <see cref="RequestSignature"/> for the current request.
    /// </summary>
    public RequestSignature RequestSignature => RequestSignature.From(this.RequestTransformContext.HttpContext.Request);

    /// <summary>
    /// Gets a value indicating whether the current request is authenticated.
    /// </summary>
    public bool IsAuthenticated => this.RequestTransformContext.HttpContext.User.Identity?.IsAuthenticated == true;

    /// <summary>
    /// Gets a value indicating whether the current request's content is a form.
    /// </summary>
    public bool RequestHasFormContentType => this.RequestTransformContext.HttpContext.Request.HasFormContentType;

    /// <inheritdoc/>
    public PipelineStepStatus ExecutionStatus { get; init; }

    /// <inheritdoc/>
    public CancellationToken CancellationToken { get; init; }

    /// <inheritdoc/>
    public ILogger Logger { get; init; }

    /// <inheritdoc/>
    public YarpPipelineError ErrorDetails { get; init; }

    /// <summary>
    /// Gets the YARP <see cref="RequestTransformContext"/>.
    /// </summary>
    internal RequestTransformContext RequestTransformContext { get; init; }

    /// <summary>
    /// Gets a value indicating whether the pipeline should be terminated. This is used by the
    /// terminate predicate for the <see cref="YarpPipeline"/>.
    /// </summary>
    internal bool ShouldTerminatePipeline => this.PipelineState != TransformState.Continue || this.CancellationToken.IsCancellationRequested;

    private TransformState PipelineState { get; init; }

    private NonForwardedResponseDetails ResponseDetails { get; init; }

    /// <summary>
    /// Gets an instance of the <see cref="YarpPipelineState"/> for a particular
    /// <see cref="Yarp.ReverseProxy.Transforms.RequestTransformContext"/>.
    /// </summary>
    /// <param name="requestTransformContext">The <see cref="Yarp.ReverseProxy.Transforms.RequestTransformContext"/> with which to
    /// initialize the <see cref="YarpPipelineState"/>.</param>
    /// <param name="logger">The logger to use for the context.</param>
    /// <param name="cancellationToken">The cancellation token to use for the context.</param>
    /// <returns>The initialized instance.</returns>
    /// <remarks>
    /// You can explicitly provide a logger; if you don't it will be resolved from the service provider. If
    /// no logger is available in the service provider, it will fall back to the <see cref="NullLogger"/>.
    /// </remarks>
    public static YarpPipelineState For(RequestTransformContext requestTransformContext, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        return new(requestTransformContext, default, TransformState.Continue, default, default, logger ?? requestTransformContext.HttpContext.RequestServices?.GetService<ILogger>() ?? NullLogger.Instance, cancellationToken);
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
        return this with { PipelineState = TransformState.Terminate, ResponseDetails = responseDetails };
    }

    /// <summary>
    /// Returns a <see cref="YarpPipelineState"/> instance which will terminate the pipeline
    /// and allow the request to be forwarded to the appropriate endpoint.
    /// </summary>
    /// <returns>The terminating <see cref="YarpPipelineState"/>.</returns>
    public YarpPipelineState TerminateAndForward()
    {
        this.Logger.LogInformation(Pipeline.EventIds.Result, "terminate-and-forward");
        return this with { PipelineState = TransformState.TerminateAndForward };
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
        return this with { PipelineState = TransformState.Continue };
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
        if (this.PipelineState == TransformState.Continue || this.PipelineState == TransformState.TerminateAndForward)
        {
            responseDetails = default;
            return true;
        }

        responseDetails = this.ResponseDetails;
        return false;
    }
}