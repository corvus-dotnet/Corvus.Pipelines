// <copyright file="ExceptionVersusErrorBenchmark.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

#pragma warning disable CA1822 // Mark members as static - not helpful in benchmarks

using System.Diagnostics.CodeAnalysis;
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
            static state => state.TransientFailure(new("There has been an issue")))
        .Retry<ErrorState, Error>(state => state.FailureCount < 5);

    private static readonly PipelineStep<ErrorState> PipelineWithException =
    Pipeline.Build(
         Pipeline.Current<ErrorState>().Bind(static _ => throw new InvalidOperationException("Some exception is thrown")))
        .Catch<ErrorState, InvalidOperationException>(static (state, ex) => state.TransientFailure(new("There has been an issue", ex)))
        .Retry<ErrorState, Error>(state => state.FailureCount < 5);

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
        return result.TryGetErrorDetails(out Error _);
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
        return result.TryGetErrorDetails(out Error _);
    }

    private readonly struct ErrorState : ICanFail<ErrorState, Error>
    {
        private readonly Error? errorDetails;

        private ErrorState(PipelineStepStatus executionStatus, int failureCount, Error? errorDetails)
        {
            this.ExecutionStatus = executionStatus;
            this.FailureCount = failureCount;
            this.errorDetails = errorDetails;
        }

        public PipelineStepStatus ExecutionStatus { get; }

        public int FailureCount { get; }

        public ErrorState PermanentFailure(Error errorDetails)
        {
            return new(PipelineStepStatus.PermanentFailure, this.FailureCount, errorDetails);
        }

        public ErrorState PrepareToRetry()
        {
            return new(PipelineStepStatus.Success, this.FailureCount + 1, this.errorDetails);
        }

        public ErrorState ResetFailureState()
        {
            return new(PipelineStepStatus.Success, 0, null);
        }

        public ErrorState Success()
        {
            return new(PipelineStepStatus.Success, 0, null);
        }

        public ErrorState TransientFailure(Error errorDetails)
        {
            return new(PipelineStepStatus.TransientFailure, this.FailureCount + 1, errorDetails);
        }

        public bool TryGetErrorDetails([NotNullWhen(true)] out Error errorDetails)
        {
            if (this.errorDetails is Error error)
            {
                errorDetails = error;
                return true;
            }

            errorDetails = default;
            return false;
        }
    }

    private readonly record struct Error(string Message, Exception? InnerException = null);
}