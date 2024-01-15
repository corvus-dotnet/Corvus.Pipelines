// <copyright file="CookieRewriting.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Collections.Immutable;
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
public static class CookieRewriting
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
    public static PipelineStep<YarpRequestPipelineState> RescopeForRequest(
        string[] cookieNamePrefixes,
        string scopePrefix)
        => RescopeForRequestSync(cookieNamePrefixes, scopePrefix).ToAsync();

    /// <summary>
    /// Returns a <see cref="SyncPipelineStep{YarpRequestPipelineState}"/> that ensures that if the
    /// incoming request contains a cookie matching one of the entries in
    /// <paramref name="cookieNamePrefixes"/>, the proxied request will contain the same cookie value
    /// but with the key changed to have the <paramref name="scopePrefix"/> removed.
    /// </summary>
    /// <param name="cookieNamePrefixes">The cookie names that should have the <paramref name="scopePrefix"/> prepended.</param>
    /// <param name="scopePrefix">The prefix to add to matching cookie names.</param>
    /// <returns>The pipeline.</returns>
    public static SyncPipelineStep<YarpRequestPipelineState> RescopeForRequestSync(
        string[] cookieNamePrefixes,
        string scopePrefix)
        => (YarpRequestPipelineState state) =>
        {
            IRequestCookieCollection cookies = state.RequestTransformContext.HttpContext.Request.Cookies;
            if (cookies.Count > 0)
            {
                foreach ((string cookieName, string cookieValue) in cookies)
                {
                    ReadOnlyMemory<char> cookNameRos = cookieName.AsMemory();
                    bool thisCookieWasChanged = false;

                    if (cookieName.StartsWith(scopePrefix))
                    {
                        ReadOnlySpan<char> originalCookieName = cookieName.AsSpan()[scopePrefix.Length..];
                        foreach (ReadOnlySpan<char> cookieNamePrefix in cookieNamePrefixes)
                        {
                            if (originalCookieName.StartsWith(cookieNamePrefix, StringComparison.Ordinal))
                            {
                                cookNameRos = cookNameRos[scopePrefix.Length..];
                                thisCookieWasChanged = true;
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

                    state = state.WithCookieHeader(cookieHeaderValue, thisCookieWasChanged);
                }
            }

            return state;
        };

    /// <summary>
    /// Applies the cookie header values from <paramref name="forwardedRequestDetails"/> to
    /// a <see cref="HttpRequestHeaders"/> instance.
    /// </summary>
    /// <param name="forwardedRequestDetails">
    /// Description of how the request should be proxied to the back end.
    /// </param>
    /// <param name="headers">
    /// The outgoing request's headers.
    /// </param>
    public static void ApplyToRequest(
        ForwardedRequestDetails forwardedRequestDetails,
        HttpRequestHeaders headers)
    {
        if (forwardedRequestDetails.AtLeastOneCookieHeaderValueIsDifferent &&
            forwardedRequestDetails.CookieHeaderValues is ImmutableList<string> cookieHeaderValues)
        {
            headers.Remove("Cookie");

            // Although headers.Add offers an overload that accepts an IEnumerable<string?>
            // we're going to enumerate the headers ourselves so that we can avoid allocating
            // an enumerator.
            foreach (string cookieHeaderValue in cookieHeaderValues)
            {
                headers.Add("Cookie", cookieHeaderValue);
            }

            // ...but it's going to look up the Cookie header info every time, so it's
            // possible that this will in fact be faster.
            // Only one way to find out!
            ////headers.Add("Cookie", forwardedRequestDetails.CookieHeaderValues);
        }
    }

    /// <summary>
    /// Returns a <see cref="PipelineStep{YarpResponsePipelineState}"/> that ensures that if the
    /// back end response sets a cookie matching one of the entries in
    /// <paramref name="cookieNamePrefixes"/>, the proxied response will contain the same cookie value
    /// but with the key changed to have the <paramref name="scopePrefix"/> prepended.
    /// </summary>
    /// <param name="cookieNamePrefixes">The cookie names that should have the <paramref name="scopePrefix"/> prepended.</param>
    /// <param name="scopePrefix">The prefix to add to matching cookie names.</param>
    /// <returns>The non-terminating <see cref="YarpResponsePipelineState"/>.</returns>
    public static PipelineStep<YarpResponsePipelineState> RescopeForResponse(
        string[] cookieNamePrefixes,
        string scopePrefix)
        => RescopeForResponseSync(cookieNamePrefixes, scopePrefix).ToAsync();

    /// <summary>
    /// Returns a <see cref="SyncPipelineStep{YarpResponsePipelineState}"/> that ensures that if the
    /// back end response sets a cookie matching one of the entries in
    /// <paramref name="cookieNamePrefixes"/>, the proxied response will contain the same cookie value
    /// but with the key changed to have the <paramref name="scopePrefix"/> prepended.
    /// </summary>
    /// <param name="cookieNamePrefixes">The cookie names that should have the <paramref name="scopePrefix"/> prepended.</param>
    /// <param name="scopePrefix">The prefix to add to matching cookie names.</param>
    /// <returns>The non-terminating <see cref="YarpResponsePipelineState"/>.</returns>
    public static SyncPipelineStep<YarpResponsePipelineState> RescopeForResponseSync(
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
            if (state.ResponseTransformContext.HttpContext.Response.Headers.TryGetValue("Set-Cookie", out StringValues headerValues))
            {
                YarpResponsePipelineState.CookieHeaderChanges cookieHeaderChanges = default;
                int headersRemaining = 0;

                foreach (string? headerValue in headerValues)
                {
                    // Although StringValues supports nulls (in two ways: it may be null as
                    // a whole, but it's also possible for individual elements to be null when
                    // it's in multi-value mode), we should never see any nulls when we're
                    // enumerating the value returned by Remove. Either that will be a singular
                    // null or an empty list, in which case enumeration returns no values, or
                    // it will be a list of values populated from the Set-Cookie headers in the
                    // response, in which case each value will be a non-null string.
                    Debug.Assert(headerValue is not null, "Enumeration from IHeaderDictionary contained null");

                    // Why did we make this a ROS? It's the whole of the string.
                    ReadOnlySpan<char> headerValueRos = headerValue.AsSpan();
                    string? rescopedSetCookieHeaderValue = null;

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
                            // Although SetCookieHeaderValue would make it more obvious that we're
                            // just changing the name here, that type turns out to be very allocatey.
                            // This was a few hundred bytes cheaper, and over twice as fast.
                            int requiredHeaderValueLength = headerValueRos.Length + scopePrefix.Length;
                            rescopedSetCookieHeaderValue = string.Create(
                                requiredHeaderValueLength,
                                (scopePrefix, headerValue),
                                static (span, state) =>
                                {
                                    state.scopePrefix.CopyTo(span);
                                    state.headerValue.CopyTo(span[state.scopePrefix.Length..]);
                                });

                            if (headersRemaining <= 0)
                            {
                                headersRemaining += headerValues.Count;
                            }

                            cookieHeaderChanges.AddReplacement(headerValue, rescopedSetCookieHeaderValue, headersRemaining);
                            break;
                        }
                    }

                    headersRemaining -= 1;
                }

                if (!cookieHeaderChanges.IsEmpty)
                {
                    state = state.WithSetCookieHeadersReplaced(ref cookieHeaderChanges);
                }
            }

            return state;
        };

    /// <summary>
    /// Applies the Set-Cookie header changes from <paramref name="state"/> to
    /// the state's <see cref="HttpResponse"/>.
    /// </summary>
    /// <param name="state">
    /// The outcome of the response pipeline.
    /// </param>
    public static void ApplyToResponse(
        YarpResponsePipelineState state)
    {
        // Did the pipeline ask us to do anything?
        if (state.ShouldAddOrReplaceCookies(out YarpResponsePipelineState.CookieHeaderChanges cookieMap))
        {
            if (state.ResponseTransformContext.HttpContext.Response.Headers.Remove("Set-Cookie", out StringValues headerValues))
            {
                foreach (string? headerValue in headerValues)
                {
                    // See similar assert in RescopeForResponseSync.
                    Debug.Assert(headerValue is not null, "Enumeration from IHeaderDictionary contained null");

                    if (cookieMap.ShouldReplace(headerValue, out string? renamedValue))
                    {
                        state.ResponseTransformContext.HttpContext.Response.Headers.Append(
                            "Set-Cookie",
                            renamedValue);
                    }
                    else
                    {
                        // Put it back.
                        state.ResponseTransformContext.HttpContext.Response.Headers.Append(
                            "Set-Cookie",
                            headerValue);
                    }
                }
            }
        }
    }
}