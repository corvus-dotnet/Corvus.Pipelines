// <copyright file="HttpContextRetry.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Pipelines.AspNetCore;

/// <summary>
/// Common retry policies.
/// </summary>
public static class HttpContextRetry
{
    /// <summary>
    /// Gets a retry policy which allows transient failures.
    /// </summary>
    /// <returns>A <see cref="Predicate{TRetryContext}"/> which returns <see langword="true"/> if the operation can be retried.</returns>
    public static Predicate<RetryContext<HttpContextPipelineState>> TransientPolicy()
        => Retry.TransientPolicy<HttpContextPipelineState>();

    /// <summary>
    /// Gets a retry policy which will retry for a maximum amount of time.
    /// </summary>
    /// <param name="maxTotalDuration">The maximum total duration of the retry.</param>
    /// <returns>A <see cref="Predicate{TRetryContext}"/> which returns <see langword="true"/> if the operation can be retried.</returns>
    public static Predicate<RetryContext<HttpContextPipelineState>> DurationPolicy(TimeSpan maxTotalDuration)
        => Retry.DurationPolicy<HttpContextPipelineState>(maxTotalDuration);

    /// <summary>
    /// Gets a retry policy which allows up to a given count of failures.
    /// </summary>
    /// <param name="count">The number of times to retry.</param>
    /// <returns>A <see cref="Predicate{TRetryContext}"/> which returns <see langword="true"/> if the operation can be retried.</returns>
    public static Predicate<RetryContext<HttpContextPipelineState>> CountPolicy(int count)
        => Retry.CountPolicy<HttpContextPipelineState>(count);

    /// <summary>
    /// Gets a retry policy which allows transient failures up to a given count of failures.
    /// </summary>
    /// <param name="count">The number of times to retry.</param>
    /// <returns>A <see cref="Predicate{TRetryContext}"/> which returns <see langword="true"/> if the operation can be retried.</returns>
    public static Predicate<RetryContext<HttpContextPipelineState>> TransientWithCountPolicy(int count)
        => TransientPolicy().And(CountPolicy(count));

    /// <summary>
    /// Gets a pipeline step that can log a retry state.
    /// </summary>
    /// <returns>A <see cref="PipelineStep{RetryContext}"/> that can log the details of a retry operation.</returns>
    public static PipelineStep<RetryContext<HttpContextPipelineState>> LogStrategy() => Retry.LogStrategy<HttpContextPipelineState>();

    /// <summary>
    /// Gets a pipeline step that can log a retry state.
    /// </summary>
    /// <returns>A <see cref="SyncPipelineStep{RetryContext}"/> that can log the details of a retry operation.</returns>
    public static SyncPipelineStep<RetryContext<HttpContextPipelineState>> LogStrategySync() => Retry.LogStrategySync<HttpContextPipelineState>();

    /// <summary>
    /// Gets a pipeline step that delays for a fixed period.
    /// </summary>
    /// <param name="duration">The fixed duration to delay before retrying.</param>
    /// <param name="jitter">Include jitter.</param>
    /// <param name="randomGenerator">An optional random number generator for random elements to the delay strategy.</param>
    /// <returns>A <see cref="PipelineStep{HttpContextPipelineState}"/> that will delay before retrying the operation.</returns>
    public static PipelineStep<RetryContext<HttpContextPipelineState>> FixedDelayStrategy(TimeSpan duration, bool jitter = false, Func<double>? randomGenerator = null)
        => LogStrategy().Bind(Retry.FixedDelayStrategy<HttpContextPipelineState>(duration, jitter, randomGenerator));

    /// <summary>
    /// Gets a pipeline step that delays with a linear backoff.
    /// </summary>
    /// <param name="baseDuration">The initial duration for the linear retry delay.</param>
    /// <param name="maximumDuration">The maximum duration the linear retry delay.</param>
    /// <param name="jitter">Include jitter.</param>
    /// <param name="randomGenerator">An optional random number generator for random elements to the delay strategy.</param>
    /// <returns>A <see cref="PipelineStep{HttpContextPipelineState}"/> that will delay before retrying the operation.</returns>
    public static PipelineStep<RetryContext<HttpContextPipelineState>> LinearDelayStrategy(TimeSpan baseDuration, TimeSpan maximumDuration, bool jitter = false, Func<double>? randomGenerator = null)
        => LogStrategy().Bind(Retry.LinearDelayStrategy<HttpContextPipelineState>(baseDuration, maximumDuration, jitter, randomGenerator));

    /// <summary>
    /// Gets a pipeline step that delays with an exponential backoff.
    /// </summary>
    /// <param name="baseDuration">The initial duration for the linear retry delay.</param>
    /// <param name="maximumDuration">The maximum duration the linear retry delay.</param>
    /// <param name="jitter">Include jitter.</param>
    /// <param name="randomGenerator">An optional random number generator for random elements to the delay strategy.</param>
    /// <returns>A <see cref="PipelineStep{TState}"/> that will delay before retrying the operation.</returns>
    public static PipelineStep<RetryContext<HttpContextPipelineState>> ExponentialDelayStrategy(TimeSpan baseDuration, TimeSpan maximumDuration, bool jitter = false, Func<double>? randomGenerator = null)
        => LogStrategy().Bind(Retry.ExponentialDelayStrategy<HttpContextPipelineState>(baseDuration, maximumDuration, jitter, randomGenerator));
}