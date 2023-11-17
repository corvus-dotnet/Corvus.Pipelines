// <copyright file="RequestSignature.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http;

namespace Corvus.YarpPipelines;

/// <summary>
/// The elements of a request enabling identification for pipeline selection purposes.
/// </summary>
/// <param name="Host">The host.</param>
/// <param name="Path">The URL path.</param>
/// <param name="QueryString">The query string.</param>
/// <param name="Verb">The HTTP verb (method).</param>
public readonly record struct RequestSignature(string Host, string Path, QueryString QueryString, string Verb)
{
    /// <summary>
    /// Creates a <see cref="RequestSignature"/> from the elements of an <see cref="HttpRequest"/>.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>A signature.</returns>
    internal static RequestSignature From(HttpRequest request)
    {
        // TODO: are we going to cause potentially unnecessary parsing of the query string by
        // retrieving the Query property, or does it not do anything until we ask it questions?
        return new RequestSignature(request.Host.Host, request.Path, request.QueryString, request.Method);
    }
}