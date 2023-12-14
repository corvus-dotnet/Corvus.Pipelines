// <copyright file="DebugInProcessConfigDry.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

namespace Corvus.Pipelines.Benchmarks;

/// <summary>
/// Avoid out-of-proc execution and multiple iterations to enable debugging.
/// </summary>
public class DebugInProcessConfigDry : DebugConfig
{
    /// <inheritdoc/>
    public override IEnumerable<Job> GetJobs()
        => new[]
        {
        Job.Dry // Job.Dry instead of Job.Default
            .WithToolchain(
                new InProcessEmitToolchain(
                    TimeSpan.FromHours(1), // 1h should be enough to debug the benchmark
                    true)),
        };
}