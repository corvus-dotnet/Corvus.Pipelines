// <copyright file="ForwardedRequestDetails.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.YarpPipelines;

/// <summary>
/// Describes how to proxy the request to the back end.
/// </summary>
/// <param name="ClusterId">The cluster that will handle the request.</param>
/// <param name="PathOverride">
/// If not null, this determines the path of the request sent to the back end.
/// If null, the request path will be derived from nominal request signature.
/// </param>
/// <param name="QueryStringOverride">
/// If not null, this determines the query string of the request sent to the back end.
/// If null, the request query string will be derived from nominal request signature.
/// </param>
/// <param name="CookieHeaderValues">
/// The cookie header values to send to the back end, or null if they are not to be changed.
/// </param>
public readonly record struct ForwardedRequestDetails(
    string ClusterId,
    ReadOnlyMemory<char>? PathOverride,
    ReadOnlyMemory<char>? QueryStringOverride,
    string[]? CookieHeaderValues);