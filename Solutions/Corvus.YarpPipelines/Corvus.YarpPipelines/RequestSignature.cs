﻿// <copyright file="RequestSignature.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http;

namespace Corvus.YarpPipelines;

/// <summary>
/// The elements of a request enabling identification for pipeline selection purposes.
/// </summary>
/// <remarks>
/// TODO: is this type overloaded? This is both a request signature, but also a vector for feeding
/// information back out of the pipeline.
/// </remarks>
public readonly struct RequestSignature
{
    private readonly HttpRequest? request;
    private readonly RequestSignatureOverride? requestSignatureOverride;

    /// <summary>
    /// Create an instance of a request signature from its components.
    /// </summary>
    /// <param name="host">The host.</param>
    /// <param name="path">The URL path.</param>
    /// <param name="queryString">The query string.</param>
    /// <param name="method">The HTTP method/verb.</param>
    public RequestSignature(string host, PathString path, QueryString queryString, string method)
    {
        this.request = default;

        // TODO: As with our previous Feature-based implementation, this allocates; but it reduces the size of the state for the
        // normal execution path of the pipeline. This was initially only used for OIDC redirects, but it now seems
        // to be affecting cookie rescoping, so we may need to rethink. (But changing it back to a struct makes
        // cookie rescoping handling noticeably slower.)
        this.requestSignatureOverride = new(host, path, queryString, method);
    }

    private RequestSignature(HttpRequest request)
    {
        this.request = request;
    }

    /// <summary>
    /// Gets the host.
    /// </summary>
    public string Host => this.request?.Host.Host ?? this.requestSignatureOverride?.Host ?? throw new InvalidOperationException();

    /// <summary>
    /// Gets the URL path.
    /// </summary>
    public PathString Path => this.request?.Path ?? this.requestSignatureOverride?.Path ?? throw new InvalidOperationException();

    /// <summary>
    /// Gets the query string.
    /// </summary>
    public QueryString QueryString => this.request?.QueryString ?? this.requestSignatureOverride?.QueryString ?? throw new InvalidOperationException();

    /// <summary>
    /// Gets the HTTP method/verb.
    /// </summary>
    public string Method => this.request?.Method ?? this.requestSignatureOverride?.Method ?? throw new InvalidOperationException();

    /// <summary>
    /// Creates a <see cref="RequestSignature"/> representing a specific path, not specifying any particular
    /// host or method.
    /// </summary>
    /// <param name="path">The <see cref="Path"/>.</param>
    /// <param name="queryString">The <see cref="QueryString"/>.</param>
    /// <returns>A <see cref="RequestSignature"/>.</returns>
    public static RequestSignature ForPathAndQueryString(PathString path, QueryString queryString)
        => new(string.Empty, path, queryString, string.Empty);

    /// <summary>
    /// Creates a <see cref="RequestSignature"/> from the elements of an <see cref="HttpRequest"/>.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>A signature.</returns>
    public static RequestSignature From(HttpRequest request)
    {
        // TODO: are we going to cause potentially unnecessary parsing of the query string by
        // retrieving the Query property, or does it not do anything until we ask it questions?
        return new RequestSignature(request);
    }

    /// <summary>
    /// Returns a new <see cref="RequestSignature"/> which copies all but the
    /// <see cref="Path"/> property, replacing that with the specified value.
    /// </summary>
    /// <param name="path">The value for <see cref="Path"/>.</param>
    /// <returns>A <see cref="RequestSignature"/>.</returns>
    public RequestSignature WithPath(PathString path)
        => new(this.Host, path, this.QueryString, this.Method);

    /// <summary>
    /// Returns a new <see cref="RequestSignature"/> which copies all but the
    /// <see cref="QueryString"/> property, replacing that with the specified value.
    /// </summary>
    /// <param name="queryString">The value for <see cref="QueryString"/>.</param>
    /// <returns>A <see cref="RequestSignature"/>.</returns>
    public RequestSignature WithQueryString(QueryString queryString)
        => new(this.Host, this.Path, queryString, this.Method);

    /// <summary>
    /// Holds the elements of a request signature that are not derived from
    /// an <see cref="HttpRequest"/>.
    /// </summary>
    /// <remarks>
    /// Normally we avoid heap-based data, but we discovered that adding the fields
    /// in here to every single <see cref="RequestSignature"/> had a significant
    /// impact on performance, because these things are copied all over the place.
    /// </remarks>
    private record RequestSignatureOverride(string Host, PathString Path, QueryString QueryString, string Method);
}