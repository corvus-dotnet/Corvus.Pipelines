// <copyright file="CookieRewritingResponseBenchmark.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using BenchmarkDotNet.Attributes;

using Corvus.Pipelines;
using Corvus.YarpPipelines;

using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

using Yarp.ReverseProxy.Transforms;

namespace Steps;

/// <summary>
/// Analyzes performance of <see cref="CookieRewriting.RescopeForResponseSync(string[], string)"/>.
/// </summary>
[MemoryDiagnoser]
public class CookieRewritingResponseBenchmark
{
    private static readonly string[] CookiePrefixesToRescope = ["foo"];

    private static readonly SyncPipelineStep<YarpResponsePipelineState> ResponseStep =
        CookieRewriting.RescopeForResponseSync(CookiePrefixesToRescope, "AddedPrefix");

    private readonly YarpResponsePipelineState noSetCookiesState;
    private readonly YarpResponsePipelineState singleNonMatchingSetCookieState;
    private readonly YarpResponsePipelineState singleMatchingSetCookieState;

    /// <summary>
    /// Setup.
    /// </summary>
    public CookieRewritingResponseBenchmark()
    {
        this.noSetCookiesState = YarpResponsePipelineState.For(CreateResponseTransformContext());
        this.singleNonMatchingSetCookieState = YarpResponsePipelineState.For(
            CreateResponseTransformContext(new KeyValuePair<string, string>("ShouldNotChange", "bar")));
        this.singleMatchingSetCookieState = YarpResponsePipelineState.For(
            CreateResponseTransformContext(new KeyValuePair<string, string>("fooShouldChange", "bar")));
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

    /// <summary>
    /// Execute the pipeline when the back end response sets a single cookie that
    /// matches one of the cookie prefixes, causing the pipeline to prepend the
    /// scope prefix.
    /// </summary>
    /// <returns>The state, to ensure the benchmark isn't optimized into oblivion.</returns>
    [Benchmark]
    public YarpResponsePipelineState SingleMatchingSetCookie()
    {
        return ResponseStep(this.singleMatchingSetCookieState);
    }

    // TODO:
    // The Request benchmarks come in two forms:
    //  1) those that exercise just the step (like the above)
    //  2) those that also exercise ApplyToRequest
    // We should consider adding benchmarks that also exercise ApplyToResponse.

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