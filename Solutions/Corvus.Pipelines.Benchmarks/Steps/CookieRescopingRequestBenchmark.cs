// <copyright file="CookieRescopingRequestBenchmark.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;

using Corvus.Pipelines;
using Corvus.YarpPipelines;

using Microsoft.AspNetCore.Http;

using Yarp.ReverseProxy.Transforms;

namespace Steps;

/// <summary>
/// Analyzes performance of <see cref="CookieRescoping.ForRequestSync(string[], string)"/>.
/// </summary>
[MemoryDiagnoser]
public class CookieRescopingRequestBenchmark
{
#pragma warning disable SA1010 // Opening square brackets should be spaced correctly. This is a bug in the analyzer.
    private static readonly string[] CookiePrefixesToRescope = ["foo"];
#pragma warning restore SA1010 // Opening square brackets should be spaced correctly

    private static readonly SyncPipelineStep<YarpRequestPipelineState> RequestStep =
        CookieRescoping.ForRequestSync(CookiePrefixesToRescope, "AddedPrefix");

    private readonly YarpRequestPipelineState noIncomingCookiesState;
    private readonly YarpRequestPipelineState singleNonMatchingCookieState;
    private readonly YarpRequestPipelineState singleMatchingCookieState;

    /// <summary>
    /// Setup.
    /// </summary>
    public CookieRescopingRequestBenchmark()
    {
        this.noIncomingCookiesState = YarpRequestPipelineState.For(new RequestTransformContext
        {
            HttpContext = new DefaultHttpContext(),
            ProxyRequest = new(),
        });

        this.singleNonMatchingCookieState = YarpRequestPipelineState.For(new RequestTransformContext
        {
            HttpContext = new DefaultHttpContext
            {
                Request = { Cookies = new CookieCollection { { "ShouldNotChange", "bar" } } },
            },
            ProxyRequest = new(),
        });

        this.singleMatchingCookieState = YarpRequestPipelineState.For(new RequestTransformContext
        {
            HttpContext = new DefaultHttpContext
            {
                Request = { Cookies = new CookieCollection { { "AddedPrefixfooShouldChange", "bar" } } },
            },
            ProxyRequest = new(),
        });
    }

    /// <summary>
    /// Execute the pipeline when the incoming request has no cookies.
    /// </summary>
    /// <returns>The state, to ensure the benchmark isn't optimized into oblivion.</returns>
    [Benchmark]
    public YarpRequestPipelineState NoIncomingCookies()
    {
        return RequestStep(this.noIncomingCookiesState);
    }

    /// <summary>
    /// Execute the pipeline when the incoming request has a single cookie that does not
    /// match any of the specified prefixes.
    /// </summary>
    /// <returns>The state, to ensure the benchmark isn't optimized into oblivion.</returns>
    [Benchmark]
    public YarpRequestPipelineState SingleIncomingNonMatchingCookie()
    {
        return RequestStep(this.singleNonMatchingCookieState);
    }

    /// <summary>
    /// Execute the pipeline when the incoming request has a single cookie that does not
    /// match any of the specified prefixes.
    /// </summary>
    /// <returns>The state, to ensure the benchmark isn't optimized into oblivion.</returns>
    [Benchmark]
    public YarpRequestPipelineState SingleIncomingMatchingCookie()
    {
        return RequestStep(this.singleMatchingCookieState);
    }

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