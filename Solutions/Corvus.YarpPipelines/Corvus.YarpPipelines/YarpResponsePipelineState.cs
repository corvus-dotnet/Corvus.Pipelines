// <copyright file="YarpResponsePipelineState.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Corvus.Pipelines;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Yarp.ReverseProxy.Transforms;

namespace Corvus.YarpPipelines;

/// <summary>
/// The state for processing a YARP response transform.
/// </summary>
public readonly struct YarpResponsePipelineState :
    ICancellable<YarpResponsePipelineState>,
    ILoggable<YarpResponsePipelineState>,
    IErrorProvider<YarpResponsePipelineState, YarpPipelineError>
{
    /// <inheritdoc/>
    public CancellationToken CancellationToken { get; init; }

    /// <inheritdoc/>
    public PipelineStepStatus ExecutionStatus { get; init; }

    /// <inheritdoc/>
    public ILogger Logger { get; init; }

    /// <inheritdoc/>
    public YarpPipelineError ErrorDetails { get; init; }

    /// <summary>
    /// Gets the <see cref="IFeatureCollection"/> for the current request.
    /// </summary>
    public IFeatureCollection Features => this.ResponseTransformContext.HttpContext.Features;

    /// <summary>
    /// Gets the YARP <see cref="ResponseTransformContext"/>.
    /// </summary>
    internal ResponseTransformContext ResponseTransformContext { get; init; }

    /// <summary>
    /// Gets an instance of the <see cref="YarpResponsePipelineState"/> for a particular
    /// <see cref="Yarp.ReverseProxy.Transforms.ResponseTransformContext"/>.
    /// </summary>
    /// <param name="responseTransformContext">The <see cref="Yarp.ReverseProxy.Transforms.ResponseTransformContext"/> with which to
    /// initialize the <see cref="YarpResponsePipelineState"/>.</param>
    /// <param name="logger">The logger to use for the context.</param>
    /// <param name="cancellationToken">The cancellation token to use for the context.</param>
    /// <returns>The initialized instance.</returns>
    /// <remarks>
    /// You can explicitly provide a logger; if you don't it will be resolved from the service provider. If
    /// no logger is available in the service provider, it will fall back to the <see cref="NullLogger"/>.
    /// </remarks>
    public static YarpResponsePipelineState For(ResponseTransformContext responseTransformContext, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        return new()
        {
            ResponseTransformContext = responseTransformContext,
            Logger = logger ?? responseTransformContext.HttpContext.RequestServices?.GetService<ILogger>() ?? NullLogger.Instance,
            CancellationToken = cancellationToken,
        };
    }
}