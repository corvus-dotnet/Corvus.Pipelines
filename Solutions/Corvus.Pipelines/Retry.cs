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
        where TState : struct, ICanFail
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
        where TState : struct, ICanFail
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
        where TState : struct, ICanFail
    {
        return retryContext => retryContext.FailureCount < count;
    }

    /// <summary>
    /// Gets a pipeline step that can log a retry state.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <returns>A <see cref="SyncPipelineStep{RetryContext}"/> that can log the details of a retry operation.</returns>
    public static PipelineStep<RetryContext<TState>> LogStrategy<TState>()
        where TState : struct, ILoggable, ICanFail
        => static retryContext =>
            {
                if (retryContext.State.Logger.IsEnabled(LogLevel.Information))
                {
                    retryContext.State.Logger.LogInformation(Pipeline.EventIds.Retrying, message: "Retrying: {failureCount}", retryContext.FailureCount);
                }

                return ValueTask.FromResult(retryContext);
            };

    /// <summary>
    /// Gets a pipeline step that can log a retry state.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <returns>A <see cref="SyncPipelineStep{RetryContext}"/> that can log the details of a retry operation.</returns>
    public static SyncPipelineStep<RetryContext<TState>> LogStrategySync<TState>()
        where TState : struct, ILoggable, ICanFail
        => static retryContext =>
        {
            if (retryContext.State.Logger.IsEnabled(LogLevel.Information))
            {
                retryContext.State.Logger.LogInformation(Pipeline.EventIds.Retrying, message: "Retrying: {failureCount}", retryContext.FailureCount);
            }

            return retryContext;
        };

    /// <summary>
    /// Gets a pipeline step that delays for a fixed period.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="duration">The fixed duration to delay before retrying.</param>
    /// <returns>A <see cref="PipelineStep{RetryContext}"/> that will delay before retrying the operation.</returns>
    public static PipelineStep<RetryContext<TState>> FixedDelayStrategy<TState>(TimeSpan duration)
        where TState : struct, ICanFail
        => async retryContext =>
        {
            await Task.Delay(duration).ConfigureAwait(false);
            return retryContext;
        };

    /// <summary>
    /// Gets a pipeline step that delays with a linear backoff.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="initialDuration">The initial duration for the linear retry delay.</param>
    /// <param name="increment">The increment for the linear retry delay.</param>
    /// <param name="maximumDuration">The maximum duration the linear retry delay.</param>
    /// <returns>A <see cref="PipelineStep{RetryContext}"/> that will delay before retrying the operation.</returns>
    public static PipelineStep<RetryContext<TState>> LinearDelayStrategy<TState>(TimeSpan initialDuration, TimeSpan increment, TimeSpan maximumDuration)
        where TState : struct, ICanFail
        => async retryContext =>
        {
            TimeSpan desiredIncrement = initialDuration + ((retryContext.FailureCount - 1) * increment);

            if (desiredIncrement > maximumDuration)
            {
                desiredIncrement = maximumDuration;
            }

            await Task.Delay(desiredIncrement).ConfigureAwait(false);
            return retryContext;
        };

    /// <summary>
    /// Gets a pipeline step that delays with an exponential backoff.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="initialDuration">The initial duration for the linear retry delay.</param>
    /// <param name="increment">The increment for the linear retry delay.</param>
    /// <param name="maximumDuration">The maximum duration the linear retry delay.</param>
    /// <returns>A <see cref="PipelineStep{RetryContext}"/> that will delay before retrying the operation.</returns>
    public static PipelineStep<RetryContext<TState>> ExponentialDelayStrategy<TState>(TimeSpan initialDuration, TimeSpan increment, TimeSpan maximumDuration)
        where TState : struct, ICanFail
        => async retryContext =>
        {
            TimeSpan desiredIncrement = initialDuration + (Math.Pow(2, retryContext.FailureCount - 1) * increment);

            if (desiredIncrement > maximumDuration)
            {
                desiredIncrement = maximumDuration;
            }

            await Task.Delay(desiredIncrement).ConfigureAwait(false);
            return retryContext;
        };

    /// <summary>
    /// A binary operator that produces a predicate that is the logical AND of two retry policy predicates.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="lhs">The lhs of the AND operator.</param>
    /// <param name="rhs">The rhs of the AND operator.</param>
    /// <returns>A <see cref="Predicate{RetryContext}"/> that represents <c>lhs AND rhs</c>.</returns>
    public static Predicate<RetryContext<TState>> And<TState>(this Predicate<RetryContext<TState>> lhs, Predicate<RetryContext<TState>> rhs)
        where TState : struct, ICanFail
        => retryContext => lhs(retryContext) && rhs(retryContext);

    /// <summary>
    /// A binary operator that produces a predicate that is the logical AND of two retry policy predicates.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="lhs">The lhs of the AND operator.</param>
    /// <param name="rhs">The rhs of the AND operator.</param>
    /// <returns>A <see cref="Predicate{RetryContext}"/> that represents <c>lhs AND rhs</c>.</returns>
    public static Predicate<RetryContext<TState>> Or<TState>(this Predicate<RetryContext<TState>> lhs, Predicate<RetryContext<TState>> rhs)
        where TState : struct, ICanFail
        => retryContext => lhs(retryContext) || rhs(retryContext);

    /// <summary>
    /// A unary operator that produces a predicate that is the logical not of a retry policy predicate.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="predicate">The predicate to NOT.</param>
    /// <returns>A <see cref="Predicate{RetryContext}"/> that represents <c>NOT lhs</c>.</returns>
    public static Predicate<RetryContext<TState>> Not<TState>(this Predicate<RetryContext<TState>> predicate)
        where TState : struct, ICanFail
        => retryContext => !predicate(retryContext);


}