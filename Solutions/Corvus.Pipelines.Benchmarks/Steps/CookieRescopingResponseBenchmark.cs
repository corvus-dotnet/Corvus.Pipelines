// <copyright file="CookieRescopingResponseBenchmark.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;

using Corvus.Pipelines;
using Corvus.YarpPipelines;

using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

using Yarp.ReverseProxy.Transforms;

namespace Steps;

/// <summary>
/// Analyzes performance of <see cref="CookieRescoping.ForResponseSync(string[], string)"/>.
/// </summary>
[MemoryDiagnoser]
public class CookieRescopingResponseBenchmark
{
#pragma warning disable SA1010 // Opening square brackets should be spaced correctly. This is a bug in the analyzer.
    private static readonly string[] CookiePrefixes = ["foo"];
#pragma warning restore SA1010 // Opening square brackets should be spaced correctly

    private static readonly SyncPipelineStep<YarpResponsePipelineState> ResponseStep =
        CookieRescoping.ForResponseSync(CookiePrefixes, "AddedPrefix");

    private readonly YarpResponsePipelineState noSetCookiesState;
    private readonly YarpResponsePipelineState singleNonMatchingSetCookieState;

    /// <summary>
    /// Setup.
    /// </summary>
    public CookieRescopingResponseBenchmark()
    {
        this.noSetCookiesState = YarpResponsePipelineState.For(CreateResponseTransformContext());
        this.singleNonMatchingSetCookieState = YarpResponsePipelineState.For(
            CreateResponseTransformContext(new KeyValuePair<string, string>("ShouldNotChange", "bar")));

        // NEXT TIME: add a do-something benchmark
    }

    /// <summary>
    /// Execute the pipeline when the back end response sets no cookies.
    /// </summary>
    /// <returns>The state, to ensure the benchmark isn't optimized into oblivion.</returns>
    [Benchmark]
    public YarpResponsePipelineState NoSetCookies()
    {
        return ResponseStep(this.noSetCookiesState);
    }

    /// <summary>
    /// Execute the pipeline when the back end response sets a single cookie that does not
    /// match any of the specified prefixes.
    /// </summary>
    /// <returns>The state, to ensure the benchmark isn't optimized into oblivion.</returns>
    [Benchmark]
    public YarpResponsePipelineState SingleNonMatchingSetCookie()
    {
        return ResponseStep(this.singleNonMatchingSetCookieState);
    }

    private static ResponseTransformContext CreateResponseTransformContext(
        params KeyValuePair<string, string>[] cookies)
    {
        DefaultHttpContext backEndHttpContext = new();
        HttpResponseMessage outputResponse = new();

        foreach ((string cookieName, string cookieValue) in cookies)
        {
            string setCookieHeaderValue = new SetCookieHeaderValue(cookieName, cookieValue).ToString();
            backEndHttpContext.Response.Headers.Append(
                HeaderNames.SetCookie,
                setCookieHeaderValue);
            outputResponse.Headers.Add(
                HeaderNames.SetCookie, setCookieHeaderValue);
        }

        return new()
        {
            HttpContext = backEndHttpContext,
            ProxyResponse = outputResponse,
        };
    }
}