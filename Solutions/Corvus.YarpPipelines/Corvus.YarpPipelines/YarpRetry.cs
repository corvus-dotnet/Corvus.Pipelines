// <copyright file="YarpRetry.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Corvus.Pipelines;

namespace Corvus.YarpPipelines;

/// <summary>
/// Common retry policies.
/// </summary>
public static class YarpRetry
{
    /// <summary>
    /// Gets a retry policy which allows transient failures.
    /// </summary>
    /// <returns>A <see cref="Predicate{TRetryContext}"/> which returns <see langword="true"/> if the operation can be retried.</returns>
    public static Predicate<RetryContext<YarpRequestPipelineState>> TransientPolicy()
        => Retry.TransientPolicy<YarpRequestPipelineState>();

    /// <summary>
    /// Gets a retry policy which will retry for a maximum amount of time.
    /// </summary>
    /// <param name="maxTotalDuration">The maximum total duration of the retry.</param>
    /// <returns>A <see cref="Predicate{TRetryContext}"/> which returns <see langword="true"/> if the operation can be retried.</returns>
    public static Predicate<RetryContext<YarpRequestPipelineState>> DurationPolicy(TimeSpan maxTotalDuration)
        => Retry.DurationPolicy<YarpRequestPipelineState>(maxTotalDuration);

    /// <summary>
    /// Gets a retry policy which allows up to a given count of failures.
    /// </summary>
    /// <param name="count">The number of times to retry.</param>
    /// <returns>A <see cref="Predicate{TRetryContext}"/> which returns <see langword="true"/> if the operation can be retried.</returns>
    public static Predicate<RetryContext<YarpRequestPipelineState>> CountPolicy(int count)
        => Retry.CountPolicy<YarpRequestPipelineState>(count);

    /// <summary>
    /// Gets a retry policy which allows transient failures up to a given count of failures.
    /// </summary>
    /// <param name="count">The number of times to retry.</param>
    /// <returns>A <see cref="Predicate{TRetryContext}"/> which returns <see langword="true"/> if the operation can be retried.</returns>
    public static Predicate<RetryContext<YarpRequestPipelineState>> TransientWithCountPolicy(int count)
        => TransientPolicy().And(CountPolicy(count));

    /// <summary>
    /// Gets a pipeline step that can log a retry state.
    /// </summary>
    /// <returns>A <see cref="PipelineStep{RetryContext}"/> that can log the details of a retry operation.</returns>
    public static PipelineStep<RetryContext<YarpRequestPipelineState>> LogStrategy() => Retry.LogStrategy<YarpRequestPipelineState>();

    /// <summary>
    /// Gets a pipeline step that can log a retry state.
    /// </summary>
    /// <returns>A <see cref="SyncPipelineStep{RetryContext}"/> that can log the details of a retry operation.</returns>
    public static SyncPipelineStep<RetryContext<YarpRequestPipelineState>> LogStrategySync() => Retry.LogStrategySync<YarpRequestPipelineState>();

    /// <summary>
    /// Gets a pipeline step that delays for a fixed period.
    /// </summary>
    /// <param name="duration">The fixed duration to delay before retrying.</param>
    /// <param name="jitter">Include jitter.</param>
    /// <param name="randomGenerator">An optional random number generator for random elements to the delay strategy.</param>
    /// <param name="timeProvider">An optional time provider.</param>
    /// <returns>A <see cref="PipelineStep{YarpRequestPipelineState}"/> that will delay before retrying the operation.</returns>
    public static PipelineStep<RetryContext<YarpRequestPipelineState>> FixedDelayStrategy(TimeSpan duration, bool jitter = false, Func<double>? randomGenerator = null, TimeProvider? timeProvider = null)
        => LogStrategy().Bind(Retry.FixedDelayStrategy<YarpRequestPipelineState>(duration, jitter, randomGenerator, timeProvider));

    /// <summary>
    /// Gets a pipeline step that delays with a linear backoff.
    /// </summary>
    /// <param name="baseDuration">The initial duration for the linear retry delay.</param>
    /// <param name="maximumDuration">The maximum duration the linear retry delay.</param>
    /// <param name="jitter">Include jitter.</param>
    /// <param name="randomGenerator">An optional random number generator for random elements to the delay strategy.</param>
    /// <param name="timeProvider">An optional time provider.</param>
    /// <returns>A <see cref="PipelineStep{YarpRequestPipelineState}"/> that will delay before retrying the operation.</returns>
    public static PipelineStep<RetryContext<YarpRequestPipelineState>> LinearDelayStrategy(TimeSpan baseDuration, TimeSpan maximumDuration, bool jitter = false, Func<double>? randomGenerator = null, TimeProvider? timeProvider = null)
        => LogStrategy().Bind(Retry.LinearDelayStrategy<YarpRequestPipelineState>(baseDuration, maximumDuration, jitter, randomGenerator, timeProvider));

    /// <summary>
    /// Gets a pipeline step that delays with an exponential backoff.
    /// </summary>
    /// <param name="baseDuration">The initial duration for the linear retry delay.</param>
    /// <param name="maximumDuration">The maximum duration the linear retry delay.</param>
    /// <param name="jitter">Include jitter.</param>
    /// <param name="randomGenerator">An optional random number generator for random elements to the delay strategy.</param>
    /// <param name="timeProvider">An optional time provider.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> that will delay before retrying the operation.</returns>
    public static PipelineStep<RetryContext<YarpRequestPipelineState>> ExponentialDelayStrategy(TimeSpan baseDuration, TimeSpan maximumDuration, bool jitter = false, Func<double>? randomGenerator = null, TimeProvider? timeProvider = null)
        => LogStrategy().Bind(Retry.ExponentialDelayStrategy<YarpRequestPipelineState>(baseDuration, maximumDuration, jitter, randomGenerator, timeProvider));
}