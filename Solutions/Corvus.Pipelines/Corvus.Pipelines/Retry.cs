// <copyright file="Retry.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;

namespace Corvus.Pipelines;

/// <summary>
/// Common retry policies.
/// </summary>
public static class Retry
{
    /// <summary>
    /// Gets a retry policy which allows transient failures.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <returns>A <see cref="Predicate{RetryContext}"/> which returns <see langword="true"/> if the operation can be retried.</returns>
    public static Predicate<RetryContext<TState>> TransientPolicy<TState>()
        where TState : struct, ICanFail<TState>
    {
        return retryContext => retryContext.State.ExecutionStatus == PipelineStepStatus.TransientFailure;
    }

    /// <summary>
    /// Gets a retry policy which will retry for a maximum amount of time.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="maxTotalDuration">The maximum total duration of the retry.</param>
    /// <returns>A <see cref="Predicate{RetryContext}"/> which returns <see langword="true"/> if the operation can be retried.</returns>
    public static Predicate<RetryContext<TState>> DurationPolicy<TState>(TimeSpan maxTotalDuration)
        where TState : struct, ICanFail<TState>
    {
        return retryContext => retryContext.RetryDuration < maxTotalDuration;
    }

    /// <summary>
    /// Gets a retry policy for a given count of failures.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="count">The number of times to retry.</param>
    /// <returns>A <see cref="Predicate{RetryContext}"/> which returns <see langword="true"/> if the operation can be retried.</returns>
    public static Predicate<RetryContext<TState>> CountPolicy<TState>(int count)
        where TState : struct, ICanFail<TState>
    {
        return retryContext => retryContext.FailureCount < count;
    }

    /// <summary>
    /// Gets a pipeline step that can log a retry state.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <returns>A <see cref="SyncPipelineStep{RetryContext}"/> that can log the details of a retry operation.</returns>
    public static PipelineStep<RetryContext<TState>> LogStrategy<TState>()
        where TState : struct, ILoggable<TState>, ICanFail<TState>
        => static retryContext =>
            {
                if (retryContext.State.Logger.IsEnabled(LogLevel.Information))
                {
                    retryContext.State.Logger.LogInformation(Pipeline.EventIds.Retrying, message: "Retrying: {failureCount} {duration}", retryContext.FailureCount, retryContext.RetryDuration);
                }

                return ValueTask.FromResult(retryContext);
            };

    /// <summary>
    /// Gets a pipeline step that can log a retry state.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <returns>A <see cref="SyncPipelineStep{RetryContext}"/> that can log the details of a retry operation.</returns>
    public static SyncPipelineStep<RetryContext<TState>> LogStrategySync<TState>()
        where TState : struct, ILoggable<TState>, ICanFail<TState>
        => static retryContext =>
        {
            if (retryContext.State.Logger.IsEnabled(LogLevel.Information))
            {
                retryContext.State.Logger.LogInformation(Pipeline.EventIds.Retrying, message: "Retrying: {failureCount} {duration}", retryContext.FailureCount, retryContext.RetryDuration);
            }

            return retryContext;
        };

    /// <summary>
    /// Gets a pipeline step that delays for a fixed period.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="duration">The fixed duration to delay before retrying.</param>
    /// <param name="jitter">Include jitter.</param>
    /// <param name="randomGenerator">An optional random number generator for random elements to the delay strategy.</param>
    /// <returns>A <see cref="PipelineStep{RetryContext}"/> that will delay before retrying the operation.</returns>
    public static PipelineStep<RetryContext<TState>> FixedDelayStrategy<TState>(TimeSpan duration, bool jitter = false, Func<double>? randomGenerator = null)
        where TState : struct, ICanFail<TState>
    {
        Func<double> rg = randomGenerator ?? (static () => Random.Shared.NextDouble());

        return async retryContext =>
        {
            double basis = retryContext.CorrelationBase;
            TimeSpan retryDelay = RetryDelayHelper.GetRetryDelay(
                RetryDelayHelper.DelayBackoffType.Constant,
                jitter,
                retryContext.FailureCount,
                duration,
                null,
                ref basis,
                rg);
            await Task.Delay(retryDelay).ConfigureAwait(false);
            return retryContext with { CorrelationBase = basis };
        };
    }

    /// <summary>
    /// Gets a pipeline step that delays with a linear backoff.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="baseDuration">The base duration for the linear retry delay.</param>
    /// <param name="maximumDuration">The maximum duration the linear retry delay.</param>
    /// <param name="jitter">Include jitter.</param>
    /// <param name="randomGenerator">An optional random number generator for random elements to the delay strategy.</param>
    /// <returns>A <see cref="PipelineStep{RetryContext}"/> that will delay before retrying the operation.</returns>
    public static PipelineStep<RetryContext<TState>> LinearDelayStrategy<TState>(TimeSpan baseDuration, TimeSpan maximumDuration, bool jitter = false, Func<double>? randomGenerator = null)
        where TState : struct, ICanFail<TState>
    {
        Func<double> rg = randomGenerator ?? (static () => Random.Shared.NextDouble());

        return async retryContext =>
         {
             double basis = retryContext.CorrelationBase;
             TimeSpan retryDelay = RetryDelayHelper.GetRetryDelay(
                 RetryDelayHelper.DelayBackoffType.Linear,
                 jitter,
                 retryContext.FailureCount,
                 baseDuration,
                 maximumDuration,
                 ref basis,
                 rg);

             await Task.Delay(retryDelay).ConfigureAwait(false);
             return retryContext with { CorrelationBase = basis };
         };
    }

    /// <summary>
    /// Gets a pipeline step that delays with an exponential backoff.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="baseDuration">The initial duration for the linear retry delay.</param>
    /// <param name="maximumDuration">The maximum duration the linear retry delay.</param>
    /// <param name="jitter">Include jitter.</param>
    /// <param name="randomGenerator">An optional random number generator for random elements to the delay strategy.</param>
    /// <returns>A <see cref="PipelineStep{RetryContext}"/> that will delay before retrying the operation.</returns>
    public static PipelineStep<RetryContext<TState>> ExponentialDelayStrategy<TState>(TimeSpan baseDuration, TimeSpan maximumDuration, bool jitter = false, Func<double>? randomGenerator = null)
        where TState : struct, ICanFail<TState>
    {
        Func<double> rg = randomGenerator ?? (static () => Random.Shared.NextDouble());

        return async retryContext =>
        {
            double basis = retryContext.CorrelationBase;
            TimeSpan retryDelay = RetryDelayHelper.GetRetryDelay(
                RetryDelayHelper.DelayBackoffType.Exponential,
                jitter,
                retryContext.FailureCount,
                baseDuration,
                maximumDuration,
                ref basis,
                rg);
            await Task.Delay(retryDelay).ConfigureAwait(false);
            return retryContext with { CorrelationBase = basis };
        };
    }

    /// <summary>
    /// A binary operator that produces a predicate that is the logical AND of two retry policy predicates.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="lhs">The lhs of the AND operator.</param>
    /// <param name="rhs">The rhs of the AND operator.</param>
    /// <returns>A <see cref="Predicate{RetryContext}"/> that represents <c>lhs AND rhs</c>.</returns>
    public static Predicate<RetryContext<TState>> And<TState>(this Predicate<RetryContext<TState>> lhs, Predicate<RetryContext<TState>> rhs)
        where TState : struct, ICanFail<TState>
        => retryContext => lhs(retryContext) && rhs(retryContext);

    /// <summary>
    /// A binary operator that produces a predicate that is the logical AND of two retry policy predicates.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="lhs">The lhs of the AND operator.</param>
    /// <param name="rhs">The rhs of the AND operator.</param>
    /// <returns>A <see cref="Predicate{RetryContext}"/> that represents <c>lhs AND rhs</c>.</returns>
    public static Predicate<RetryContext<TState>> Or<TState>(this Predicate<RetryContext<TState>> lhs, Predicate<RetryContext<TState>> rhs)
        where TState : struct, ICanFail<TState>
        => retryContext => lhs(retryContext) || rhs(retryContext);

    /// <summary>
    /// A unary operator that produces a predicate that is the logical not of a retry policy predicate.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="predicate">The predicate to NOT.</param>
    /// <returns>A <see cref="Predicate{RetryContext}"/> that represents <c>NOT lhs</c>.</returns>
    public static Predicate<RetryContext<TState>> Not<TState>(this Predicate<RetryContext<TState>> predicate)
        where TState : struct, ICanFail<TState>
        => retryContext => !predicate(retryContext);
}