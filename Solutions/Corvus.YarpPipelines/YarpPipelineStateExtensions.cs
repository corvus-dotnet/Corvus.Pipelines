// <copyright file="YarpPipelineStateExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Buffers;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Transforms;

namespace Corvus.YarpPipelines;

/// <summary>
/// Extension methods for <see cref="YarpPipelineState"/>.
/// </summary>
public static class YarpPipelineStateExtensions
{
    /// <summary>
    /// Gets the raw text value of a bearer token in the Authorization header of the request
    /// associated with a <see cref="YarpPipelineState"/>, if such a header is present.
    /// </summary>
    /// <typeparam name="TTokenReceiverState">
    /// The type of value to be passed through to <paramref name="rawTokenReceiver"/>.
    /// </typeparam>
    /// <param name="state">The state representing the request to be inspected.</param>
    /// <param name="tokenReceiverState">A value passed through to the <paramref name="rawTokenReceiver"/>.</param>
    /// <param name="rawTokenReceiver">Callback that will be invoked with the raw token, if present.</param>
    /// <returns>True if a bearer token was present.</returns>
    public static bool TryGetRawBearerToken<TTokenReceiverState>(
        this YarpPipelineState state,
        in TTokenReceiverState tokenReceiverState,
        ReadOnlySpanAction<char, TTokenReceiverState> rawTokenReceiver)
    {
        if (TryGetRawBearerToken(state, out ReadOnlyMemory<char> rawToken))
        {
            rawTokenReceiver(rawToken.Span, tokenReceiverState);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the raw text value of a bearer token in the Authorization header of the request
    /// associated with a <see cref="YarpPipelineState"/>, if such a header is present.
    /// </summary>
    /// <param name="state">The state representing the request to be inspected.</param>
    /// <param name="rawToken">The raw token, if present.</param>
    /// <returns>True if a bearer token was present.</returns>
    public static bool TryGetRawBearerToken(
        this YarpPipelineState state,
        out ReadOnlyMemory<char> rawToken)
    {
        if (state.HttpRequest.Headers.Authorization is StringValues authHeader
            && authHeader.Count == 1)
        {
            // This code looks for a Bearer token, which is something we're only going
            // to see on API requests - browser don't send Bearer tokens for HTML, and so
            // on.
            string authHeaderValue = authHeader[0]!;
            const string expectedPrefix = "Bearer ";
            if (authHeaderValue.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
            {
                rawToken = authHeaderValue.AsMemory()[expectedPrefix.Length..].Trim();
                return true;
            }
        }

        rawToken = default;
        return false;
    }
}