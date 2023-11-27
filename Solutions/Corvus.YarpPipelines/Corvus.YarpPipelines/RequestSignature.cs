// <copyright file="RequestSignature.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http;

namespace Corvus.YarpPipelines;

/// <summary>
/// The elements of a request enabling identification for pipeline selection purposes.
/// </summary>
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
    public RequestSignature(string host, string path, QueryString queryString, string method)
    {
        this.request = default;

        // As with our previous Feature-based implementation, this allocates; but it reduces the size of the state for the
        // normal execution path of the pipeline. This is only used for e.g. OIDC redirects.
        this.requestSignatureOverride = new(host, path, queryString, method);
    }

    private RequestSignature(HttpRequest request)
    {
        this.request = request;
    }

    /// <summary>
    /// Gets the host.
    /// </summary>B
    public string Host => this.request?.Host.Host ?? this.requestSignatureOverride?.Host ?? throw new InvalidOperationException();

    /// <summary>
    /// Gets the URL path.
    /// </summary>
    public string Path => this.request?.Path ?? this.requestSignatureOverride?.Path ?? throw new InvalidOperationException();

    /// <summary>
    /// Gets the query string.
    /// </summary>
    public QueryString QueryString => this.request?.QueryString ?? this.requestSignatureOverride?.QueryString ?? throw new InvalidOperationException();

    /// <summary>
    /// Gets the HTTP method/verb.
    /// </summary>
    public string Method => this.request?.Method ?? this.requestSignatureOverride?.Method ?? throw new InvalidOperationException();

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

    private record RequestSignatureOverride(string Host, string Path, QueryString QueryString, string Method);
}