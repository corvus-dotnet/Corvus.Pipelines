﻿// <copyright file="RescopeCookieExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Diagnostics;
using System.Net.Http.Headers;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Corvus.YarpPipelines;

/// <summary>
/// Cookie rescoping extension methods for <see cref="YarpRequestPipelineState"/>.
/// and <see cref="YarpResponsePipelineState"/>.
/// </summary>
public static class RescopeCookieExtensions
{
    /// <summary>
    /// Returns a <see cref="YarpRequestPipelineState"/> instance that will continue processing the pipeline,
    /// ensuring that if the incoming request contained a cookie matching one of the entries in
    /// <paramref name="cookieNamePrefixes"/>, the proxied request will contain the same cookie value
    /// but with the key changed to have the <paramref name="scopePrefix"/> removed.
    /// </summary>
    /// <param name="state">The YARP pipeline state.</param>
    /// <param name="cookieNamePrefixes">The cookie names that should have the <paramref name="scopePrefix"/> prepended.</param>
    /// <param name="scopePrefix">The prefix to add to matching cookie names.</param>
    /// <returns>The non-terminating <see cref="YarpRequestPipelineState"/>.</returns>
    public static YarpRequestPipelineState RescopeCookies(
        this YarpRequestPipelineState state,
        IReadOnlyList<string> cookieNamePrefixes,
        string scopePrefix)
    {
        IRequestCookieCollection cookies = state.RequestTransformContext.HttpContext.Request.Cookies;

        state.RequestTransformContext.ProxyRequest.Headers.Remove("Cookie");

        foreach ((string cookieName, string cookieValue) in cookies)
        {
            ReadOnlySpan<char> cookNameRos = cookieName;

            if (cookieName.StartsWith(scopePrefix))
            {
                ReadOnlySpan<char> originalCookieName = cookieName[scopePrefix.Length..].AsSpan();
                foreach (ReadOnlySpan<char> cookieNamePrefix in cookieNamePrefixes)
                {
                    if (originalCookieName.StartsWith(cookieNamePrefix, StringComparison.Ordinal))
                    {
                        cookNameRos = originalCookieName;
                        break;
                    }
                }
            }

            state.RequestTransformContext.ProxyRequest.Headers.Add("Cookie", $"{cookNameRos}={cookieValue}");
        }

        return state;
    }

    /// <summary>
    /// Returns a <see cref="YarpResponsePipelineState"/> instance that will continue processing the pipeline,
    /// ensuring that if the incoming response contained a cookie matching one of the entries in
    /// <paramref name="cookieNamePrefixes"/>, the proxied response will contain the same cookie value
    /// but with the key changed to have the <paramref name="scopePrefix"/> prepended.
    /// </summary>
    /// <param name="state">The YARP pipeline state.</param>
    /// <param name="cookieNamePrefixes">The cookie names that should have the <paramref name="scopePrefix"/> prepended.</param>
    /// <param name="scopePrefix">The prefix to add to matching cookie names.</param>
    /// <returns>The non-terminating <see cref="YarpResponsePipelineState"/>.</returns>
    public static YarpResponsePipelineState RescopeCookies(
        this YarpResponsePipelineState state,
        IReadOnlyList<string> cookieNamePrefixes,
        string scopePrefix)
    {
        if (state.ResponseTransformContext.ProxyResponse?.Headers is HttpResponseHeaders headers)
        {
            IEnumerable<SetCookieHeaderValue> allTheSetCookieHeaders = headers
                .Where(kv => kv.Key == "Set-Cookie")
                .SelectMany(kv => kv.Value)
                .Select(headerValue => SetCookieHeaderValue.Parse(headerValue))
                .ToList();

            // TODO: does this remove all of them if there are several?
            state.ResponseTransformContext.HttpContext.Response.Headers.Remove("Set-Cookie");

            // TODO: perf
            foreach (SetCookieHeaderValue setCookieHeader in allTheSetCookieHeaders)
            {
                StringSegment cookieName = setCookieHeader.Name;
                foreach (ReadOnlySpan<char> cookieNamePrefix in cookieNamePrefixes)
                {
                    if (cookieName.AsSpan().StartsWith(cookieNamePrefix))
                    {
                        setCookieHeader.Name = scopePrefix + cookieName;
                        break;
                    }
                }

                ////headers.Add("Set-Cookie", setCookieHeader.ToString());
                state.ResponseTransformContext.HttpContext.Response.Headers.Append(
                    "Set-Cookie", setCookieHeader.ToString());
            }
        }

        // On entry to this method, the ProxyResponse has any Set-Cookie headers
        // from the back end (because it just represents the back end's response
        // directly).
        // The HttpContext.Response has already been populated - its Cookies
        // property does already include all the cookies from the back end.
        // There apparently isn't any way for us to get either YARP or ASP.NET Core
        // to enumerate the Set-Cookies.

        // There might be multiple cookie headers, and each of those might contain
        // multiple entries. So a single request might have the following:
        //      Set-Cookie: foo=bar; path=/
        //      Set-Cookie: baz=quux; path=/; secure; httponly
        //      Set-Cookie: foo=bar;
        //      Set-Cookie: quux=quuz
        // The problem is we might want to rewrite just of them. The rewritten version
        // might look like this:
        //      Set-Cookie: foo=bar; PREFIXED.baz=quux; irritating=baz
        //      Set-Cookie: quux=quuz
        // So there are a few challenges:
        //  1. we need to be able to discover each individual cookie name/value pair
        //  2. we need to be able to replace specific cookie names selectively
        //  3. we mustn't accidentally tip past the maximum header length
        // The problem we have is that the state.RequestTransformContext.ProxyRequest
        // doesn't present a cookie collection abstraction. It's all just headers,
        // so you need to understand the structure of each cookie header value to
        // find the ones you're interested in.
        // It might be easier to blow away the cookies from the ProxyRequest completely,
        // and rebuild them entirely from scratch.
        // So we might want to turn off YARP's header handling completely, which we
        // could do by setting the relevant context property in our EndjinAppModelTransformProvider's
        // Apply method.
        // NEXT TIME: carry on from here.
        Debug.WriteLine(state.ResponseTransformContext.ProxyResponse?.Headers);
        Debug.WriteLine(state.ResponseTransformContext.HttpContext.Response.Cookies);
        Debug.WriteLine(state.ResponseTransformContext.HttpContext.Request.Cookies);
        return state;
    }
}