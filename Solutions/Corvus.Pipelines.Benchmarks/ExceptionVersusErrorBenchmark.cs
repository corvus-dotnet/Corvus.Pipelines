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
    private static readonly SyncPipelineStep<ErrorState> PipelineWithError =
        Pipeline.Build<ErrorState>(
            static state => state.TransientFailure())
        .Retry(state => state.FailureCount < 5);

    private static readonly SyncPipelineStep<ErrorState> PipelineWithException =
    Pipeline.Build(
         Pipeline.CurrentSync<ErrorState>().Bind(static _ => throw new InvalidOperationException("Some exception is thrown")))
        .Catch<ErrorState, InvalidOperationException>(static (state, _) => state.TransientFailure())
        .Retry(state => state.FailureCount < 5);

    private static readonly PipelineStep<ErrorState> PipelineWithErrorAsync =
    Pipeline.Build<ErrorState>(
        static state => ValueTask.FromResult(state.TransientFailure()))
    .Retry(state => state.FailureCount < 5);

    private static readonly PipelineStep<ErrorState> PipelineWithExceptionAsync =
    Pipeline.Build(
         Pipeline.Current<ErrorState>().Bind(static _ => throw new InvalidOperationException("Some exception is thrown")))
        .Catch<ErrorState, InvalidOperationException>(static (state, _) => state.TransientFailure())
        .Retry(state => state.FailureCount < 5);

    /// <summary>
    /// Run a pipeline.
    /// </summary>
    /// <returns>
    /// A result, to ensure that the code under test does not get optimized out of existence.
    /// </returns>
    [Benchmark(Baseline = true)]
    public bool RunPipelineWithError()
    {
        ErrorState result = PipelineWithError(default);
        return result.ExecutionStatus == PipelineStepStatus.Success;
    }

    /// <summary>
    /// Run a pipeline.
    /// </summary>
    /// <returns>
    /// A result, to ensure that the code under test does not get optimized out of existence.
    /// </returns>
    [Benchmark]
    public async Task<bool> RunPipelineWithErrorAsync()
    {
        ErrorState result = await PipelineWithErrorAsync(default).ConfigureAwait(false);
        return result.ExecutionStatus == PipelineStepStatus.Success;
    }

    /// <summary>
    /// Run a pipeline.
    /// </summary>
    /// <returns>
    /// A result, to ensure that the code under test does not get optimized out of existence.
    /// </returns>
    [Benchmark]
    public bool RunPipelineWithException()
    {
        ErrorState result = PipelineWithException(default);
        return result.ExecutionStatus == PipelineStepStatus.Success;
    }

    /// <summary>
    /// Run a pipeline.
    /// </summary>
    /// <returns>
    /// A result, to ensure that the code under test does not get optimized out of existence.
    /// </returns>
    [Benchmark]
    public async Task<bool> RunPipelineWithExceptionAsync()
    {
        ErrorState result = await PipelineWithExceptionAsync(default).ConfigureAwait(false);
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