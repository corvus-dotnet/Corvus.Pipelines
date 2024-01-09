// <copyright file="YarpResponsePipelineState.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Collections.Immutable;

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
    /// Gets the <see cref="HttpResponseMessage"/> representing the response from the back end.
    /// </summary>
    public HttpResponseMessage ProxyResponse
        => this.ResponseTransformContext.ProxyResponse!; // We already checked that this is not null in the factory method.

    /// <summary>
    /// Gets the YARP <see cref="ResponseTransformContext"/>.
    /// </summary>
    internal ResponseTransformContext ResponseTransformContext { get; init; }

    private ReadOnlyMemory<(string SetCookieHeaderValueToReplace, string ReplacementHeaderValue)> SetCookieHeaderReplacements { get; init; }

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
        if (responseTransformContext.ProxyResponse == null)
        {
            throw new InvalidOperationException("A YarpResponsePipelineState should be created only when the back end responded");
        }

        return new()
        {
            ResponseTransformContext = responseTransformContext,
            Logger = logger ?? responseTransformContext.HttpContext.RequestServices?.GetService<ILogger>() ?? NullLogger.Instance,
            CancellationToken = cancellationToken,
        };
    }

    /// <summary>
    /// Returns a <see cref="YarpResponsePipelineState"/> instance with the given cookie header
    /// value updated.
    /// </summary>
    /// <param name="replacements">
    /// The Set-Cookie header values that should be replaced.
    /// </param>
    /// <returns>The updated state.</returns>
    public YarpResponsePipelineState WithSetCookieHeadersReplaced(
        ReadOnlyMemory<(string SetCookieHeaderValueToReplace, string ReplacementHeaderValue)> replacements)
    {
        return this with
        {
            SetCookieHeaderReplacements = replacements,
        };
    }

    /// <summary>
    /// Determines whether any cookies in the response should be renamed.
    /// </summary>
    /// <param name="cookieRenames">The renaming details.</param>
    /// <returns>True if at least one cookie should be renamed.</returns>
    public bool ShouldAddOrReplaceCookies(out CookiesToAddOrReplace cookieRenames)
    {
        if (this.SetCookieHeaderReplacements.IsEmpty)
        {
            cookieRenames = default;
            return false;
        }

        cookieRenames = new CookiesToAddOrReplace(this.SetCookieHeaderReplacements);
        return true;
    }

    /// <summary>
    /// Describes the <c>Set-Cookie</c> headers to be rewritten or added.
    /// </summary>
    /// <remarks>
    /// Currently, we don't support adding, because we have no scenarios that need it.
    /// </remarks>
    public readonly struct CookiesToAddOrReplace
    {
        private readonly ReadOnlyMemory<(string SetCookieHeaderValueToReplace, string ReplacementHeaderValue)> setCookieHeaderReplacements;

        /// <summary>
        /// Initializes a <see cref="CookiesToAddOrReplace"/>.
        /// </summary>
        /// <param name="setCookieHeaderReplacements">
        /// The Set-Cookie header values that should be replaced.
        /// </param>
        public CookiesToAddOrReplace(ReadOnlyMemory<(string SetCookieHeaderValueToReplace, string ReplacementHeaderValue)> setCookieHeaderReplacements)
        {
            this.setCookieHeaderReplacements = setCookieHeaderReplacements;
        }

        /// <summary>
        /// Determines whether a particular Set-Cookie header should be replaced.
        /// </summary>
        /// <param name="headerValue">The header's current value.</param>
        /// <param name="newHeaderValue">The replacement header value.</param>
        /// <returns>
        /// True if the header should be replaced, false if it should be left as-is.
        /// </returns>
        public bool ShouldReplace(string headerValue, out string? newHeaderValue)
        {
            ReadOnlySpan<(string SetCookieHeaderValueToReplace, string ReplacementHeaderValue)> replacements = this.setCookieHeaderReplacements.Span;
            for (int i = 0; i < replacements.Length; ++i)
            {
                if (replacements[i].SetCookieHeaderValueToReplace == headerValue)
                {
                    newHeaderValue = replacements[i].ReplacementHeaderValue;
                    return true;
                }
            }

            newHeaderValue = default;
            return false;
        }
    }
}