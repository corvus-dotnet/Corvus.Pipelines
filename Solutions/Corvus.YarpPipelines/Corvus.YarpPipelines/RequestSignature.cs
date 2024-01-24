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
/// Modifying the query string, e.g. removing parts that are only of interest to the proxy and
/// which the back end will not understand.
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
    private readonly object requestOrOverride;

    /// <summary>
    /// Create an instance of a request signature from its components.
    /// </summary>
    /// <param name="host">The host and, if present, port.</param>
    /// <param name="path">The URL path.</param>
    /// <param name="queryString">The query string.</param>
    /// <param name="method">The HTTP method/verb.</param>
    /// <remarks>
    /// <para>
    /// There are two cases where this gets used.
    /// </para>
    /// <para>
    /// When a pipeline chooses a different path and/or query for the forwarded request, there
    /// is no underlying <see cref="HttpRequest"/> to use, so we need something else to hold
    /// onto the components. If this were the only scenario, we could move this into the
    /// <see cref="YarpRequestPipelineState.ForwardedRequestDetails"/> to simplify this type.
    /// </para>
    /// <para>
    /// The other scenario in which we can't defer to an <see cref="HttpRequest"/> is when
    /// we need to set the nominal signature to something entirely different from the current
    /// request. This happens in interactive login callbacks: the current request reflects the
    /// callback endpoint, but we need the nominal signature to reflect the request that was
    /// being processed when we redirected the user to the authentication provider.
    /// </para>
    /// </remarks>
    private RequestSignature(HostString host, ReadOnlyMemory<char> path, ReadOnlyMemory<char> queryString, string method)
    {
        this.requestOrOverride = new RequestSignatureOverride(host, path, queryString, method);
    }

    private RequestSignature(HttpRequest request)
    {
        this.requestOrOverride = request;
    }

    /// <summary>
    /// Gets the host.
    /// </summary>
    public HostString Host => this.Request?.Host ?? this.SignatureOverride?.Host ?? throw new InvalidOperationException();

    /// <summary>
    /// Gets the URL path.
    /// </summary>
    public ReadOnlyMemory<char> Path => this.Request?.Path.Value?.AsMemory() ?? this.SignatureOverride?.Path ?? throw new InvalidOperationException();

    /// <summary>
    /// Gets the query string.
    /// </summary>
    public ReadOnlyMemory<char> QueryString => this.Request?.QueryString.Value?.AsMemory() ?? this.SignatureOverride?.QueryString ?? throw new InvalidOperationException();

    /// <summary>
    /// Gets the HTTP method/verb.
    /// </summary>
    public string Method => this.Request?.Method ?? this.SignatureOverride?.Method ?? throw new InvalidOperationException();

    private readonly HttpRequest? Request => this.requestOrOverride as HttpRequest;

    private readonly RequestSignatureOverride? SignatureOverride => this.requestOrOverride as RequestSignatureOverride;

    /// <summary>
    /// Creates a <see cref="RequestSignature"/> representing a specific path, not specifying any particular
    /// host or method.
    /// </summary>
    /// <param name="path">The <see cref="Path"/>.</param>
    /// <param name="queryString">The <see cref="QueryString"/>.</param>
    /// <returns>A <see cref="RequestSignature"/>.</returns>
    public static RequestSignature ForPathAndQueryString(ReadOnlyMemory<char> path, ReadOnlyMemory<char> queryString)
        => new(default, path, queryString, string.Empty);

    /// <summary>
    /// Creates a <see cref="RequestSignature"/> representing a particular URL and method.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <param name="method">The <see cref="Method"/>.</param>
    /// <returns>A <see cref="RequestSignature"/>.</returns>
    public static RequestSignature ForUrlAndMethod(string url, string method)
    {
        // TODO: This could be done more efficient, because we could obtain ReadOnlyMemory<char>s for
        // the various parts, avoiding the need to allocate new strings. Currently this code path
        // is used only in a relatively unusual case (OIDC redirects), so it won't have a huge
        // impact, but we should do it at some point.
        UriHelper.FromAbsolute(
            url,
            out _,
            out HostString host,
            out PathString path,
            out QueryString queryString,
            out _);
        return new(host, path.Value.AsMemory(), queryString.Value.AsMemory(), method);
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
    private record RequestSignatureOverride(HostString Host, ReadOnlyMemory<char> Path, ReadOnlyMemory<char> QueryString, string Method);
}