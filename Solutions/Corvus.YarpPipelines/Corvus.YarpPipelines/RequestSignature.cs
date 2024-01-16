// <copyright file="RequestSignature.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Corvus.YarpPipelines;

/// <summary>
/// The elements of a request enabling identification for pipeline selection purposes.
/// </summary>
/// <remarks>
/// <para>
/// This is used in the following ways:
/// </para>
/// <list type="bullet">
/// <item>
/// <para>
/// Rewriting paths, e.g., the public-facing endpoint might be /weather/op?x=y but we might
/// rewrite that to just /op?x=y for the back end. In this case, there will be no <see cref="Host"/>.
/// </para>
/// </item>
/// <item>
/// <para>
/// NEXT TIME: look at this.
/// Modifying the query string, e.g. removing parts that are only of interest to the proxy and
/// which the back end will not understand. (TODO: we don't yet have an example of this.)
/// </para>
/// </item>
/// <item>
/// <para>
/// Enables authentication callback handlers to set the nominal request signature to reflect the request
/// that was in progress when we redirected the caller to the authentication provider.
/// </para>
/// <para>
/// Delegates to the <see cref="HttpRequest"/> when handling the request in a straightforward way.
/// </para>
/// </item>
/// </list>
/// </remarks>
public readonly struct RequestSignature
{
    // TODO: we could collapse this into a single object? field, halving the size of this struct.
    private readonly HttpRequest? request;
    private readonly RequestSignatureOverride? requestSignatureOverride;

    /// <summary>
    /// Create an instance of a request signature from its components.
    /// </summary>
    /// <param name="host">The host and, if present, port.</param>
    /// <param name="path">The URL path.</param>
    /// <param name="queryString">The query string.</param>
    /// <param name="method">The HTTP method/verb.</param>
    private RequestSignature(HostString host, ReadOnlyMemory<char> path, QueryString queryString, string method)
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
    public HostString Host => this.request?.Host ?? this.requestSignatureOverride?.Host ?? throw new InvalidOperationException();

    /// <summary>
    /// Gets the URL path.
    /// </summary>
    public ReadOnlyMemory<char> Path => this.request?.Path.Value?.AsMemory() ?? this.requestSignatureOverride?.Path ?? throw new InvalidOperationException();

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
    public static RequestSignature ForPathAndQueryString(ReadOnlyMemory<char> path, QueryString queryString)
        => new(default, path, queryString, string.Empty);

    /// <summary>
    /// Creates a <see cref="RequestSignature"/> representing a particular URL and method.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <param name="method">The <see cref="Method"/>.</param>
    /// <returns>A <see cref="RequestSignature"/>.</returns>
    public static RequestSignature ForUrlAndMethod(string url, string method)
    {
        UriHelper.FromAbsolute(
            url,
            out _,
            out HostString host,
            out PathString path,
            out QueryString queryString,
            out _);
        return new(host, path.Value.AsMemory(), queryString, method);
    }

    /// <summary>
    /// Creates a <see cref="RequestSignature"/> from the elements of an <see cref="HttpRequest"/>.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>A signature.</returns>
    public static RequestSignature From(HttpRequest request)
    {
        return new RequestSignature(request);
    }

    /// <summary>
    /// Holds the elements of a request signature that are not derived from
    /// an <see cref="HttpRequest"/>.
    /// </summary>
    /// <remarks>
    /// Normally we avoid heap-based data, but we discovered that adding the fields
    /// in here to every single <see cref="RequestSignature"/> had a significant
    /// impact on performance, because these things are copied all over the place.
    /// </remarks>
    private record RequestSignatureOverride(HostString Host, ReadOnlyMemory<char> Path, QueryString QueryString, string Method);
}