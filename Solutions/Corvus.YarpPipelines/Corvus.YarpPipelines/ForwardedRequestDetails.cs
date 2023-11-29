// <copyright file="ForwardedRequestDetails.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.YarpPipelines;

/// <summary>
/// Describes how to proxy the request to the back end.
/// </summary>
/// <param name="ClusterId">The cluster that will handle the request.</param>
/// <param name="RequestSignature">
/// If not null, this determines the signature of the request sent to the back end.
/// If null, the request signature will be derived from nominal request signature.
/// </param>
public readonly record struct ForwardedRequestDetails(
    string ClusterId,
    RequestSignature? RequestSignature = null);