// <copyright file="YarpRequestPipelineState.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

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
/// then choose to either <see cref="Continue()"/> processing, <see cref="TerminateWith(ForwardedRequestDetails)"/> - passing
/// the <see cref="Yarp.ReverseProxy.Transforms.RequestTransformContext"/> on to YARP for forwarding to the appropriate endpoint, or
/// <see cref="TerminateWith(NonForwardedResponseDetails)"/> a specific response code, headers and/or body.
/// </remarks>
public readonly struct YarpRequestPipelineState :
    ICancellable<YarpRequestPipelineState>,
    ILoggable<YarpRequestPipelineState>,
    IErrorProvider<YarpRequestPipelineState, YarpPipelineError>
{
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
    /// terminate predicate for the <see cref="YarpRequestPipeline"/>.
    /// </summary>
    internal bool ShouldTerminatePipeline => this.PipelineState != TransformState.Continue || this.CancellationToken.IsCancellationRequested;

    // TODO: an experiment. We are wondering whether changing YarpRequestPipelineState
    // to contain a small number of reference type fields, and pooling the memory
    // that those fields point to. If we partition the information carefully,
    // this might minimize the number of times we need to copy data to enable
    // modification while retaining immutability. For example, there are values
    // passed in at startup that are immutable, so if those moved into a separate
    // object, that might reduce the amount of work expended copying the YarpRequestPipelineState
    // every time we want to create a modified version.
    // However, we're not doing that for now...
    // It is an open question as to whether this would make any kind of measurable
    // improvement, because .NET 8 seems to do a pretty good job of minimizing
    // copies when types are immutable.
    // (The motivation for this note was worry over adding "just one more" property
    // into this struct. Specifically, the cluster id and possibly the final request
    // signature.)
    private RequestSignature NominalRequestSignature { get; init; }

    private TransformState PipelineState { get; init; }

    private ForwardedRequestDetails ForwardedRequestDetails { get; init; }

    private NonForwardedResponseDetails NonForwardedResponseDetails { get; init; }

    /// <summary>
    /// Gets an instance of the <see cref="YarpRequestPipelineState"/> for a particular
    /// <see cref="Yarp.ReverseProxy.Transforms.RequestTransformContext"/>.
    /// </summary>
    /// <param name="requestTransformContext">The <see cref="Yarp.ReverseProxy.Transforms.RequestTransformContext"/> with which to
    /// initialize the <see cref="YarpRequestPipelineState"/>.</param>
    /// <param name="logger">The logger to use for the context.</param>
    /// <param name="cancellationToken">The cancellation token to use for the context.</param>
    /// <returns>The initialized instance.</returns>
    /// <remarks>
    /// You can explicitly provide a logger; if you don't it will be resolved from the service provider. If
    /// no logger is available in the service provider, it will fall back to the <see cref="NullLogger"/>.
    /// </remarks>
    public static YarpRequestPipelineState For(RequestTransformContext requestTransformContext, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        return new()
        {
            RequestTransformContext = requestTransformContext,
            NominalRequestSignature = RequestSignature.From(requestTransformContext.HttpContext.Request),
            Logger = logger ?? requestTransformContext.HttpContext.RequestServices?.GetService<ILogger>() ?? NullLogger.Instance,
            CancellationToken = cancellationToken,
        };
    }

    /// <summary>
    /// Returns a <see cref="YarpRequestPipelineState"/> instance which will terminate the pipeline
    /// with the given request forwarding details. The request will be forwarded to the endpoint.
    /// </summary>
    /// <param name="forwardedRequestDetails">The details of the response to return.</param>
    /// <returns>The terminating <see cref="YarpRequestPipelineState"/>.</returns>
    public YarpRequestPipelineState TerminateWith(ForwardedRequestDetails forwardedRequestDetails)
    {
        this.Logger.LogInformation(Pipeline.EventIds.Result, "terminate-with-forward");
        return this with { PipelineState = TransformState.TerminateAndForward, ForwardedRequestDetails = forwardedRequestDetails };
    }

    /// <summary>
    /// Returns a <see cref="YarpRequestPipelineState"/> instance which will terminate the pipeline
    /// with the given response details. The request will not be forwarded to the endpoint.
    /// </summary>
    /// <param name="responseDetails">The details of the response to return.</param>
    /// <returns>The terminating <see cref="YarpRequestPipelineState"/>.</returns>
    public YarpRequestPipelineState TerminateWith(NonForwardedResponseDetails responseDetails)
    {
        this.Logger.LogInformation(Pipeline.EventIds.Result, "terminate-with-nonforward");
        return this with { PipelineState = TransformState.Terminate, NonForwardedResponseDetails = responseDetails };
    }

    /// <summary>
    /// Returns a <see cref="YarpRequestPipelineState"/> instance that will continue processing the pipeline.
    /// </summary>
    /// <returns>The non-terminating <see cref="YarpRequestPipelineState"/>.</returns>
    /// <remarks>
    /// <para>
    /// Note that if this is the last step in the pipeline, it will allow the request to be forwarded to
    /// the appropriate endpoint.
    /// </para>
    /// </remarks>
    public YarpRequestPipelineState Continue()
    {
        this.Logger.LogInformation(Pipeline.EventIds.Result, "continue");
        return this with { PipelineState = TransformState.Continue };
    }

    /// <summary>
    /// Determines whether the result should be forwarded through YARP,
    /// or whether we should build a local response using the resulting response details.
    /// </summary>
    /// <param name="forwardedRequestDetails">The response details to use if the result should be forwarded.</param>
    /// <param name="nonForwardedResponseDetails">The response details to use if the result should not be forwarded.</param>
    /// <returns><see langword="true"/> if the result should be forwarded, and the
    /// <paramref name="forwardedRequestDetails"/> will be set, and will describe how to proxy the request. If <see langword="false"/> then
    /// the <paramref name="nonForwardedResponseDetails"/> will be set and can be used to build a local response.</returns>
    public bool ShouldForward(
        [NotNullWhen(true)] out ForwardedRequestDetails? forwardedRequestDetails,
        [NotNullWhen(false)] out NonForwardedResponseDetails? nonForwardedResponseDetails)
    {
        if (this.PipelineState == TransformState.Continue)
        {
            throw new InvalidOperationException($"The pipeline did not call {nameof(this.TerminateWith)}");
        }

        if (this.PipelineState == TransformState.TerminateAndForward)
        {
            forwardedRequestDetails = this.ForwardedRequestDetails;
            nonForwardedResponseDetails = default;
            return true;
        }

        forwardedRequestDetails = default;
        nonForwardedResponseDetails = this.NonForwardedResponseDetails;
        return false;
    }

    /// <summary>
    /// Sets the nominal request signature something other than the actual one.
    /// </summary>
    /// <param name="requestSignature">The nominal request signature.</param>
    /// <returns>The updated state.</returns>
    /// <remarks>
    /// Used in login call-back scenarios where we have a single call-back endpoint but need
    /// to select endpoint configuration based on the page the user was originally
    /// attempting to access.
    /// </remarks>
    public YarpRequestPipelineState OverrideNominalRequestSignature(RequestSignature requestSignature)
    {
        return this with { NominalRequestSignature = requestSignature };
    }

    /// <summary>
    /// Gets the nominal request signature.
    /// </summary>
    /// <returns>The nominal request signature.</returns>
    /// <remarks>
    /// This is normally the signature for the request being processed, but can
    /// be changed by calling <see cref="OverrideNominalRequestSignature(RequestSignature)"/>.
    /// </remarks>
    public RequestSignature GetNominalRequestSignature()
    {
        return this.NominalRequestSignature;
    }
}