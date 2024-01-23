// <copyright file="ForwardedRequestDetails.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Collections.Immutable;

namespace Corvus.YarpPipelines;

/// <summary>
/// Describes how to proxy the request to the back end.
/// </summary>
/// <param name="ClusterId">The cluster that will handle the request.</param>
/// <param name="PathAndQueryOverride">
/// If not null, this determines the signature of the request sent to the back end.
/// If null, the request signature will be derived from nominal request signature.
/// </param>
/// <param name="CookieHeaderValues">
/// The cookie header values to send to the back end.
/// </param>
/// <param name="AtLeastOneCookieHeaderValueIsDifferent">
/// True if this value is being changed, false if it is being copied over without modification.
/// </param>
public readonly record struct ForwardedRequestDetails(
    string ClusterId,
    (ReadOnlyMemory<char> Path, ReadOnlyMemory<char> QueryString)? PathAndQueryOverride,
    string[]? CookieHeaderValues,
    bool AtLeastOneCookieHeaderValueIsDifferent);