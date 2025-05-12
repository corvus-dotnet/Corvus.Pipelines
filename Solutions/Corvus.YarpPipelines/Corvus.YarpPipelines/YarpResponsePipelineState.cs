// <copyright file="YarpResponsePipelineState.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Diagnostics;
using System.Runtime.CompilerServices;

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
    private enum TransformState
    {
        /// <summary>
        /// The pipeline should continue processing. If we remain in this state when there are no more steps to run,
        /// then the pipeline has failed to produce a result.
        /// </summary>
        Continue,

        /// <summary>
        /// The pipeline has determined that the response should not be forwarded to the client, and no more
        /// steps should be run.
        /// </summary>
        Terminate,

        /// <summary>
        /// The pipeline has determined that the response should be forwarded to the client, and no more
        /// steps should be run.
        /// </summary>
        TerminateAndForward,
    }

    /// <inheritdoc/>
    public CancellationToken CancellationToken { get; init; }

    /// <inheritdoc/>
    public PipelineStepStatus ExecutionStatus { get; init; }

    /// <inheritdoc/>
    public ILogger Logger { get; init; }

    /// <inheritdoc/>
    public YarpPipelineError? ErrorDetails { get; init; }

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

    /// <summary>
    /// Gets a value indicating whether the pipeline should be terminated. This is used by the
    /// terminate predicate for the <see cref="YarpRequestPipeline"/>.
    /// </summary>
    internal bool ShouldTerminatePipeline => this.PipelineState != TransformState.Continue || this.CancellationToken.IsCancellationRequested;

    /// <summary>
    /// Gets the current pipeline state.
    /// </summary>
    private TransformState PipelineState { get; init; }

    private CookieHeaderChanges SetCookieHeaderReplacements { get; init; }

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
    /// <param name="cookieHeaderChanges">
    /// A <see cref="CookieHeaderChanges"/> tracking whether any Set-Cookie header values should be replaced,
    /// and if so with what.
    /// </param>
    /// <returns>The updated state.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public YarpResponsePipelineState WithSetCookieHeadersReplaced(
        ref readonly CookieHeaderChanges cookieHeaderChanges)
    {
        return this with
        {
            SetCookieHeaderReplacements = cookieHeaderChanges,
        };
    }

    /// <summary>
    /// Determines whether any cookies in the response should be renamed.
    /// </summary>
    /// <param name="cookieRenames">The renaming details.</param>
    /// <returns>True if at least one cookie should be renamed.</returns>
    public bool ShouldAddOrReplaceCookies(out CookieHeaderChanges cookieRenames)
    {
        cookieRenames = this.SetCookieHeaderReplacements;
        return !this.SetCookieHeaderReplacements.IsEmpty;
    }

    /// <summary>
    /// Accumulates the changes to the Set-Cookie headers.
    /// </summary>
    public struct CookieHeaderChanges
    {
        private (string SetCookieHeaderValueToReplace, string ReplacementHeaderValue)[]? replacements;
        private int replacementsCount;

        /// <summary>
        /// Gets a value indicating whether there are any replacements.
        /// </summary>
        public readonly bool IsEmpty => this.replacementsCount == 0;

        /// <summary>
        /// Adds a replacement.
        /// </summary>
        /// <param name="setCookieHeaderValueToReplace">
        /// The Set-Cookie header value that should be replaced.
        /// </param>
        /// <param name="replacementHeaderValue">
        /// The value with which the Set-Cookie header should be replaced.
        /// </param>
        /// <param name="maxHeaders">
        /// The number of as yet unprocessed Set-Cookie headers. (This is used to size the array on the
        /// first call.)
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddReplacement(
            string setCookieHeaderValueToReplace,
            string replacementHeaderValue,
            int maxHeaders)
        {
            if (this.replacements is null)
            {
                // We tried using ArrayPool here to amortize the allocations. Although this did reduce
                // the per-request allocations, it's not possible to get those to zero in the code paths
                // that end up allocating this array, because we have to allocate a string with the
                // modified Set-Cookie header. That string is usually going to be much larger than this
                // array, so this offers at best a marginal reduction in allocation. And it comes at a
                // significant cost: the CPU usage was about 47% higher when using ArrayPool. Since
                // it's impossible to hit zero allocation for this code path, the extra CPU usage
                // seems more important. Moreover, the extra checks required by the use of ArrayPool
                // slow down the cases where we don't need to allocate.
                this.replacements = new (string SetCookieHeaderValueToReplace, string ReplacementHeaderValue)[maxHeaders];
            }
            else
            {
                Debug.Assert(this.replacements.Length >= maxHeaders, "maxHeaders is larger than on an earlier call");
            }

            this.replacements[this.replacementsCount++] = (setCookieHeaderValueToReplace, replacementHeaderValue);
        }

        /// <summary>
        /// Determines whether a particular Set-Cookie header should be replaced.
        /// </summary>
        /// <param name="headerValue">The header's current value.</param>
        /// <param name="newHeaderValue">The replacement header value.</param>
        /// <returns>
        /// True if the header should be replaced, false if it should be left as-is.
        /// </returns>
        public readonly bool ShouldReplace(string headerValue, out string? newHeaderValue)
        {
            if (this.replacements is not null)
            {
                foreach ((string setCookieHeaderValueToReplace, string replacementHeaderValue) in this.replacements)
                {
                    if (setCookieHeaderValueToReplace == headerValue)
                    {
                        newHeaderValue = replacementHeaderValue;
                        return true;
                    }
                }
            }

            newHeaderValue = default;
            return false;
        }
    }
}