// <copyright file="CookieRescoping.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Diagnostics;
using System.Net.Http.Headers;

using Corvus.Pipelines;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Corvus.YarpPipelines;

/// <summary>
/// Cookie rescoping extension methods for <see cref="YarpRequestPipelineState"/>.
/// and <see cref="YarpResponsePipelineState"/>.
/// </summary>
public static class CookieRescoping
{
    /// <summary>
    /// Returns a <see cref="PipelineStep{YarpRequestPipelineState}"/> that ensures that if the
    /// incoming request contains a cookie matching one of the entries in
    /// <paramref name="cookieNamePrefixes"/>, the proxied request will contain the same cookie value
    /// but with the key changed to have the <paramref name="scopePrefix"/> removed.
    /// </summary>
    /// <param name="cookieNamePrefixes">The cookie names that should have the <paramref name="scopePrefix"/> prepended.</param>
    /// <param name="scopePrefix">The prefix to add to matching cookie names.</param>
    /// <returns>The pipeline.</returns>
    public static PipelineStep<YarpRequestPipelineState> ForRequest(
        string[] cookieNamePrefixes,
        string scopePrefix)
        => ForRequestSync(cookieNamePrefixes, scopePrefix).ToAsync();

    /// <summary>
    /// Returns a <see cref="SyncPipelineStep{YarpRequestPipelineState}"/> that ensures that if the
    /// incoming request contains a cookie matching one of the entries in
    /// <paramref name="cookieNamePrefixes"/>, the proxied request will contain the same cookie value
    /// but with the key changed to have the <paramref name="scopePrefix"/> removed.
    /// </summary>
    /// <param name="cookieNamePrefixes">The cookie names that should have the <paramref name="scopePrefix"/> prepended.</param>
    /// <param name="scopePrefix">The prefix to add to matching cookie names.</param>
    /// <returns>The pipeline.</returns>
    public static SyncPipelineStep<YarpRequestPipelineState> ForRequestSync(
        string[] cookieNamePrefixes,
        string scopePrefix)
        => (YarpRequestPipelineState state) =>
        {
            IRequestCookieCollection cookies = state.RequestTransformContext.HttpContext.Request.Cookies;

            // TODO: detect whether we need to change anything - no sense rebuilding all the cookies if we didn't.
            // (Probably. We will end up scanning some of the cookies twice.)
            // The expected normal use case is that you'll always need this if you ask for it, so maybe it's
            // fine as it is.
            state.RequestTransformContext.ProxyRequest.Headers.Remove("Cookie");

            foreach ((string cookieName, string cookieValue) in cookies)
            {
                ReadOnlyMemory<char> cookNameRos = cookieName.AsMemory();

                if (cookieName.StartsWith(scopePrefix))
                {
                    ReadOnlySpan<char> originalCookieName = cookieName.AsSpan()[scopePrefix.Length..];
                    foreach (ReadOnlySpan<char> cookieNamePrefix in cookieNamePrefixes)
                    {
                        if (originalCookieName.StartsWith(cookieNamePrefix, StringComparison.Ordinal))
                        {
                            cookNameRos = cookNameRos[scopePrefix.Length..];
                            break;
                        }
                    }
                }

                string cookieHeaderValue = string.Create(
                    cookNameRos.Length + cookieValue.Length + 1,
                    (cookNameRos, cookieValue),
                    static (span, state) =>
                    {
                        state.cookNameRos.Span.CopyTo(span);
                        span[state.cookNameRos.Length] = '=';
                        state.cookieValue.CopyTo(span[(state.cookNameRos.Length + 1)..]);
                    });
                state.RequestTransformContext.ProxyRequest.Headers.Add("Cookie", cookieHeaderValue);
            }

            return state;
        };

    /// <summary>
    /// Returns a <see cref="PipelineStep{YarpResponsePipelineState}"/> that ensures that if the
    /// back end response sets a cookie matching one of the entries in
    /// <paramref name="cookieNamePrefixes"/>, the proxied response will contain the same cookie value
    /// but with the key changed to have the <paramref name="scopePrefix"/> prepended.
    /// </summary>
    /// <param name="cookieNamePrefixes">The cookie names that should have the <paramref name="scopePrefix"/> prepended.</param>
    /// <param name="scopePrefix">The prefix to add to matching cookie names.</param>
    /// <returns>The non-terminating <see cref="YarpResponsePipelineState"/>.</returns>
    public static PipelineStep<YarpResponsePipelineState> ForResponse(
        string[] cookieNamePrefixes,
        string scopePrefix)
        => ForResponseSync(cookieNamePrefixes, scopePrefix).ToAsync();

    /// <summary>
    /// Returns a <see cref="SyncPipelineStep{YarpResponsePipelineState}"/> that ensures that if the
    /// back end response sets a cookie matching one of the entries in
    /// <paramref name="cookieNamePrefixes"/>, the proxied response will contain the same cookie value
    /// but with the key changed to have the <paramref name="scopePrefix"/> prepended.
    /// </summary>
    /// <param name="cookieNamePrefixes">The cookie names that should have the <paramref name="scopePrefix"/> prepended.</param>
    /// <param name="scopePrefix">The prefix to add to matching cookie names.</param>
    /// <returns>The non-terminating <see cref="YarpResponsePipelineState"/>.</returns>
    public static SyncPipelineStep<YarpResponsePipelineState> ForResponseSync(
        string[] cookieNamePrefixes,
        string scopePrefix)
        => (YarpResponsePipelineState state) =>
        {
            // On entry to this method, the ProxyResponse has any Set-Cookie headers
            // from the back end (because it just represents the back end's response
            // directly).
            // YARP has already populated the HttpContext.Response, so its Headers
            // property also includes all the Set-Cookies from the back end.
            // So we're free to use either property to determine which cookies have
            // been set. As it happens, the HttpContext.Response (representing the
            // response we're going to send back to the external client) offers the
            // higher-performance way because:
            //  1) we can discover what Set-Cookie headers were present and remove
            //      them with a single call
            //  2) it provides us with the headers as a StringValues, which can
            //      be enumerated more efficiently than the IEnumerable<string>
            //      we get from ProxyResponse.Headers
            //  3) if we Remove the Set-Cookie headers and subsequently re-add
            //      them without modification, this appears not to cause any
            //      additional allocations.
            if (state.ResponseTransformContext.HttpContext.Response.Headers.Remove("Set-Cookie", out StringValues headerValues))
            {
                foreach (string? headerValue in headerValues)
                {
                    ReadOnlySpan<char> headerValueRos = headerValue.AsSpan();
                    SetCookieHeaderValue? setCookieHeaderValue = null;

                    foreach (ReadOnlySpan<char> cookieNamePrefix in cookieNamePrefixes)
                    {
                        // RFC 6265 says that the Set-Cookie header value always starts with
                        //      cookie-name "=" cookie-value
                        // so we don't need to fully parse the thing just to work out
                        // whether it's one we want to change. (There's a cost to using
                        // SetCookieHeaderValue.Parse, so we don't want to pay that unless
                        // we're absolutely certain that we have to.)
                        if (headerValueRos.StartsWith(cookieNamePrefix))
                        {
                            setCookieHeaderValue = SetCookieHeaderValue.Parse(headerValue);
                            setCookieHeaderValue.Name = scopePrefix + setCookieHeaderValue.Name;
                            break;
                        }
                    }

                    state.ResponseTransformContext.HttpContext.Response.Headers.Append(
                        "Set-Cookie",
                        setCookieHeaderValue?.ToString() ?? headerValue);
                }
            }

            return state;
        };
}