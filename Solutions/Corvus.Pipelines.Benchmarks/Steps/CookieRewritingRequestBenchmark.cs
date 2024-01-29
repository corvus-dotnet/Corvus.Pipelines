// <copyright file="CookieRewritingRequestBenchmark.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;

using BenchmarkDotNet.Attributes;

using Corvus.Pipelines;
using Corvus.YarpPipelines;

using Microsoft.AspNetCore.Http;

using Yarp.ReverseProxy.Transforms;

namespace Steps;

/// <summary>
/// Analyzes performance of <see cref="CookieRewriting.RescopeForRequestSync(string[], string)"/>.
/// </summary>
[MemoryDiagnoser]
public class CookieRewritingRequestBenchmark
{
    private static readonly string[] CookiePrefixesToRescope = ["foo"];

    private static readonly SyncPipelineStep<YarpRequestPipelineState> RequestStep =
        CookieRewriting.RescopeForRequestSync(CookiePrefixesToRescope, "AddedPrefix");

    private readonly PipelineAndCookies noIncomingCookiesState;
    private readonly PipelineAndCookies singleNonMatchingCookieState;
    private readonly PipelineAndCookies singleMatchingCookieState;

    // The capacity of the Headers collection in a real request is necessarily large
    // enough to hold however many Cookie headers were present before we started meddling
    // and we'd like the benchmarks to reflect that. But we'd prefer to avoid the
    // capacity being unrepresentatively large, so we're using a separate collection
    // for each benchmark.

    /// <summary>
    /// Setup.
    /// </summary>
    public CookieRewritingRequestBenchmark()
    {
        this.noIncomingCookiesState = new PipelineAndCookies(
            new CookieCollection());
        this.singleNonMatchingCookieState = new PipelineAndCookies(
            new CookieCollection { { "ShouldNotChange", "bar" } });
        this.singleMatchingCookieState = new PipelineAndCookies(
            new CookieCollection { { "AddedPrefixfooShouldChange", "bar" } });
    }

    /// <summary>
    /// Execute the pipeline when the incoming request has no cookies.
    /// </summary>
    /// <returns>The state, to ensure the benchmark isn't optimized into oblivion.</returns>
    [Benchmark]
    public YarpRequestPipelineState NoIncomingCookies()
    {
        return RunAndApply(this.noIncomingCookiesState);
    }

    /// <summary>
    /// Execute the pipeline when the incoming request has a single cookie that does not
    /// match any of the specified prefixes.
    /// </summary>
    /// <returns>The state, to ensure the benchmark isn't optimized into oblivion.</returns>
    [Benchmark]
    public YarpRequestPipelineState SingleIncomingNonMatchingCookie()
    {
        return RunAndApply(this.singleNonMatchingCookieState);
    }

    /// <summary>
    /// Execute the pipeline when the incoming request has a single cookie that does not
    /// match any of the specified prefixes.
    /// </summary>
    /// <returns>The state, to ensure the benchmark isn't optimized into oblivion.</returns>
    [Benchmark]
    public YarpRequestPipelineState SingleIncomingMatchingCookie()
    {
        return RunAndApply(this.singleMatchingCookieState);
    }

    /// <summary>
    /// Oh please stop asking me for docs.
    /// </summary>
    /// <returns>Honestly.</returns>
    [Benchmark]
    public YarpRequestPipelineState JustRequestStep()
    {
        return RequestStep(this.noIncomingCookiesState.PipelineState);
    }

    /// <summary>
    /// Oh please stop asking me for docs.
    /// </summary>
    /// <returns>Honestly.</returns>
    [Benchmark]
    public YarpRequestPipelineState RequestStepAndTerminate()
    {
        YarpRequestPipelineState state = RequestStep(this.noIncomingCookiesState.PipelineState);
        return state.TerminateWithClusterIdAndPathAndQuery("c1", "/test".AsMemory(), ReadOnlyMemory<char>.Empty);
    }

    /// <summary>
    /// Oh please stop asking me for docs.
    /// </summary>
    /// <returns>Honestly.</returns>
    [Benchmark]
    public YarpRequestPipelineState RequestStepTerminateAndCallShouldForward()
    {
        YarpRequestPipelineState state = RequestStep(this.noIncomingCookiesState.PipelineState);
        state = state.TerminateWithClusterIdAndPathAndQuery("c1", "/test".AsMemory(), ReadOnlyMemory<char>.Empty);
        if (!state.ShouldForward(out _, out _))
        {
            throw new InvalidOperationException("Should have forwarded.");
        }

        return state;
    }

    private static YarpRequestPipelineState RunAndApply(PipelineAndCookies pipelineAndCookies)
    {
        YarpRequestPipelineState state = RequestStep(pipelineAndCookies.PipelineState);
        state = state.TerminateWithClusterIdAndPathAndQuery("c1", "/test".AsMemory(), ReadOnlyMemory<char>.Empty);
        if (!state.ShouldForward(out ForwardedRequestDetails? forwardedRequestDetails, out _))
        {
            throw new InvalidOperationException("Should have forwarded.");
        }

        // The first thing to call GetHeaders seems to cause a small (32 byte) allocaion.
        CookieRewriting.ApplyToRequest(forwardedRequestDetails.Value, pipelineAndCookies.GetHeaders());

        return state;
    }

    // TODO:
    // In CookieRewriting.ApplyToRequest, it is unclear which of the two mechanisms for
    // populating the Cookie headers is more efficient. To benchmark both meaningfully,
    // we need benchmarks with variable numbers of cookies in total, and variable numbers
    // of cookies that match.

    // NEXT TIME:
    // Benchmark cases where we do change something.
    // Then add response benchmarks.

    // Other dimensions:
    //  Match:
    //      Only non-matching cookies
    //      Mixture
    //  Header structure
    //      Single cookie
    //      Single header, multiple cookies
    //      Multiple cookies, one header per cookie
    //      Multiple cookies, single header
    //      Multiple cookies, distributed across several headers
    //          All the cookies we want to change are in headers that only contain cookies we want to change
    //          Not the above
    // Awkward matching (e.g., near-matching prefixes)
    private record PipelineAndCookies(
        CookieCollection Cookies)
    {
        private readonly List<string> cookieHeaderValues = Cookies
            .Select(kv => $"{kv.Key}={kv.Value}")
            .ToList();

        private readonly HttpRequestHeaders headers = new HttpRequestMessage().Headers;

        public YarpRequestPipelineState PipelineState { get; } = YarpRequestPipelineState.For(new RequestTransformContext
        {
            HttpContext = new DefaultHttpContext
            {
                Request = { Cookies = Cookies },
            },
            ProxyRequest = new(),
        });

        public HttpRequestHeaders GetHeaders()
        {
            // Previous iterations.
            this.headers.Clear();
            foreach (string cookieValue in this.cookieHeaderValues)
            {
                this.headers.Add("Cookie", cookieValue);
            }

            return this.headers;
        }
    }

    private class CookieCollection : IRequestCookieCollection, IEnumerable
    {
        private readonly Dictionary<string, string> cookies = new();

        public int Count => this.cookies.Count;

        public ICollection<string> Keys => this.cookies.Keys;

        public string? this[string key] => this.cookies[key];

        public void Add(string name, string value) => this.cookies.Add(name, value);

        public bool ContainsKey(string key) => this.cookies.ContainsKey(key);

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => this.cookies.GetEnumerator();

        public bool TryGetValue(string key, [NotNullWhen(true)] out string? value) => this.cookies.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => this.cookies.GetEnumerator();
    }
}