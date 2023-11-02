// <copyright file="ExceptionVersusErrorBenchmark.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

#pragma warning disable CA1822 // Mark members as static - not helpful in benchmarks

using BenchmarkDotNet.Attributes;

namespace Corvus.Pipelines.Benchmarks;

/// <summary>
/// Matches tables.
/// </summary>
[MemoryDiagnoser]
public class ExceptionVersusErrorBenchmark
{
    private static readonly PipelineStep<ErrorState> PipelineWithError =
        Pipeline.Build<ErrorState>(
            static state => state.TransientFailure())
        .Retry<ErrorState>(state => state.FailureCount < 5);

    private static readonly PipelineStep<ErrorState> PipelineWithException =
    Pipeline.Build(
         Pipeline.Current<ErrorState>().Bind(static _ => throw new InvalidOperationException("Some exception is thrown")))
        .Catch<ErrorState, InvalidOperationException>(static (state, _) => state.TransientFailure())
        .Retry(state => state.FailureCount < 5);

    /// <summary>
    /// Extract parameters from a URI template using the Corvus implementation of the Tavis API.
    /// </summary>
    /// <returns>
    /// A result, to ensure that the code under test does not get optimized out of existence.
    /// </returns>
    [Benchmark(Baseline = true)]
    public async Task<bool> RunPipelineWithError()
    {
        ErrorState result = await PipelineWithError(default);
        return result.ExecutionStatus == PipelineStepStatus.Success;
    }

    /// <summary>
    /// Extract parameters from a URI template using the Corvus implementation of the Tavis API.
    /// </summary>
    /// <returns>
    /// A result, to ensure that the code under test does not get optimized out of existence.
    /// </returns>
    [Benchmark]
    public async Task<bool> RunPipelineWithException()
    {
        ErrorState result = await PipelineWithException(default);
        return result.ExecutionStatus == PipelineStepStatus.Success;
    }

    private readonly struct ErrorState : ICanFail
    {
        private ErrorState(PipelineStepStatus executionStatus)
        {
            this.ExecutionStatus = executionStatus;
        }

        public PipelineStepStatus ExecutionStatus { get; }

        public ErrorState PermanentFailure()
        {
            return new(PipelineStepStatus.PermanentFailure);
        }

        public ErrorState Success()
        {
            return new(PipelineStepStatus.Success);
        }

        public ErrorState TransientFailure()
        {
            return new(PipelineStepStatus.TransientFailure);
        }
    }
}