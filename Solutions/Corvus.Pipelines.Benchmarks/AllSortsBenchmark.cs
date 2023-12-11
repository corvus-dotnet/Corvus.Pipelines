// <copyright file="AllSortsBenchmark.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

#pragma warning disable CA1822 // Mark members as static - not helpful in benchmarks

using BenchmarkDotNet.Attributes;
using Corvus.YarpPipelines;
using Microsoft.AspNetCore.Http;
using PipelineExamples;
using Yarp.ReverseProxy.Transforms;

namespace Corvus.Pipelines.Benchmarks;

/// <summary>
/// Matches tables.
/// </summary>
[MemoryDiagnoser]
public class AllSortsBenchmark
{
    private static readonly RequestTransformContext[] Contexts =
        [
            new() { HttpContext = new DefaultHttpContext() { Request = { Path = "/foo" } }, Path = "/foo" },
            new() { HttpContext = new DefaultHttpContext() { Request = { Path = "/bar" } }, Path = "/bar" },
            new() { HttpContext = new DefaultHttpContext() { Request = { Path = "/fizz" } }, Path = "/fizz" },
            new() { HttpContext = new DefaultHttpContext() { Request = { Path = "/" } }, Path = "/" },
            new() { HttpContext = new DefaultHttpContext() { Request = { Path = "/baz" } }, Path = "/baz" },
        ];

    /// <summary>
    /// Run a pipeline.
    /// </summary>
    /// <returns>
    /// A result, to ensure that the code under test does not get optimized out of existence.
    /// </returns>
    [Benchmark(Baseline = true)]
    public bool RunPipeline()
    {
        bool shouldForward = true;
        foreach (RequestTransformContext context in Contexts)
        {
            YarpRequestPipelineState result = ExampleYarpPipeline.Instance(YarpRequestPipelineState.For(context));
            shouldForward &= result.ShouldForward(out ForwardedRequestDetails? forwardedRequestDetails, out NonForwardedResponseDetails? responseDetails);
        }

        return shouldForward;
    }

    /// <summary>
    /// Run a pipeline.
    /// </summary>
    /// <returns>
    /// A result, to ensure that the code under test does not get optimized out of existence.
    /// </returns>
    [Benchmark]
    public bool RunPipelineWithLoggingAndNullLogger()
    {
        bool shouldForward = true;
        foreach (RequestTransformContext context in Contexts)
        {
            YarpRequestPipelineState result = ExampleYarpPipelineWithLogging.Instance(YarpRequestPipelineState.For(context));
            shouldForward &= result.ShouldForward(out ForwardedRequestDetails? forwardedRequestDetails, out NonForwardedResponseDetails? responseDetails);
        }

        return shouldForward;
    }

    /// <summary>
    /// Run a pipeline.
    /// </summary>
    /// <returns>
    /// A result, to ensure that the code under test does not get optimized out of existence.
    /// </returns>
    [Benchmark]
    public bool RunPipelineWithLoggingAndNopLogger()
    {
        bool shouldForward = true;
        foreach (RequestTransformContext context in Contexts)
        {
            YarpRequestPipelineState result = ExampleYarpPipelineWithLogging.Instance(YarpRequestPipelineState.For(context, NopLogger.Instance));
            shouldForward &= result.ShouldForward(out ForwardedRequestDetails? forwardedRequestDetails, out NonForwardedResponseDetails? responseDetails);
        }

        return shouldForward;
    }

    /// <summary>
    /// Run a pipeline.
    /// </summary>
    /// <returns>
    /// A result, to ensure that the code under test does not get optimized out of existence.
    /// </returns>
    [Benchmark]
    public async Task<bool> RunPipelineAsync()
    {
        bool shouldForward = true;
        foreach (RequestTransformContext context in Contexts)
        {
            YarpRequestPipelineState result = await ExampleYarpPipeline.ForceAsyncInstance(YarpRequestPipelineState.For(context)).ConfigureAwait(false);
            shouldForward &= result.ShouldForward(out ForwardedRequestDetails? forwardedRequestDetails, out NonForwardedResponseDetails? responseDetails);
        }

        return shouldForward;
    }

    /// <summary>
    /// Run a pipeline.
    /// </summary>
    /// <returns>
    /// A result, to ensure that the code under test does not get optimized out of existence.
    /// </returns>
    [Benchmark]
    public async Task<bool> RunPipelineWithLoggingAndNulLoggerAsync()
    {
        bool shouldForward = true;
        foreach (RequestTransformContext context in Contexts)
        {
            YarpRequestPipelineState result = await ExampleYarpPipelineWithLogging.ForceAsyncInstance(YarpRequestPipelineState.For(context)).ConfigureAwait(false);
            shouldForward &= result.ShouldForward(out ForwardedRequestDetails? forwardedRequestDetails, out NonForwardedResponseDetails? responseDetails);
        }

        return shouldForward;
    }

    /// <summary>
    /// Run a pipeline.
    /// </summary>
    /// <returns>
    /// A result, to ensure that the code under test does not get optimized out of existence.
    /// </returns>
    [Benchmark]
    public async Task<bool> RunPipelineWithLoggingAndNopLoggerAsync()
    {
        bool shouldForward = true;
        foreach (RequestTransformContext context in Contexts)
        {
            YarpRequestPipelineState result = await ExampleYarpPipelineWithLogging.ForceAsyncInstance(YarpRequestPipelineState.For(context, NopLogger.Instance)).ConfigureAwait(false);
            shouldForward &= result.ShouldForward(out ForwardedRequestDetails? forwardedRequestDetails, out NonForwardedResponseDetails? responseDetails);
        }

        return shouldForward;
    }
}