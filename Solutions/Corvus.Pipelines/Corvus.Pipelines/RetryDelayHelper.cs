// <copyright file="RetryDelayHelper.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>
// <license>
// This code has been derived from code in Polly, licensed under the BSD 3-clause license
// https://github.com/App-vNext/Polly/blob/25a800b210651c26f67972d72452fc129b1dac92/LICENSE
// </license>

namespace Corvus.Pipelines;

/// <summary>
/// Helper functions to generate retry delays.
/// </summary>
public static class RetryDelayHelper
{
    private const double JitterFactor = 0.5;

    private const double ExponentialFactor = 2.0;

    /// <summary>
    /// The backoff type used by the strategies.
    /// </summary>
    public enum DelayBackoffType
    {
        /// <summary>
        /// The constant backoff type.
        /// </summary>
        /// <example>
        /// 200ms, 200ms, 200ms, etc.
        /// </example>
        /// <remarks>
        /// Ensures a constant backoff for each attempt.
        /// </remarks>
        Constant,

        /// <summary>
        /// The linear backoff type.
        /// </summary>
        /// <example>
        /// 100ms, 200ms, 300ms, 400ms, etc.
        /// </example>
        /// <remarks>
        /// Generates backoffs in an linear manner.
        /// In the case randomization introduced by the jitter and exponential growth are not appropriate,
        /// the linear growth allows for more precise control over the backoff intervals.
        /// </remarks>
        Linear,

        /// <summary>
        /// The exponential backoff type with the power of 2.
        /// </summary>
        /// <example>
        /// 200ms, 400ms, 800ms.
        /// </example>
        Exponential,
    }

    /// <summary>
    /// Gets a retry delay of a particular type.
    /// </summary>
    /// <param name="type">The type of the delay.</param>
    /// <param name="jitter">If <see langword="true"/> then de-correlated jitter is applied.</param>
    /// <param name="attempt">The retry attempt number.</param>
    /// <param name="baseDelay">The base delay time.</param>
    /// <param name="maxDelay">The maximum delay time.</param>
    /// <param name="state">The state for the delay acquisition.</param>
    /// <param name="randomizer">A randomization function.</param>
    /// <returns>A delay time.</returns>
    public static TimeSpan GetRetryDelay(
        DelayBackoffType type,
        bool jitter,
        int attempt,
        TimeSpan baseDelay,
        TimeSpan? maxDelay,
        ref double state,
        Func<double> randomizer)
    {
        try
        {
            TimeSpan delay = GetRetryDelayCore(type, jitter, attempt, baseDelay, ref state, randomizer);

            // stryker disable once equality : no means to test this
            if (maxDelay is TimeSpan maxDelayValue && delay > maxDelayValue)
            {
                return maxDelay.Value;
            }

            return delay;
        }
        catch (OverflowException)
        {
            return TimeSpan.MaxValue;
        }
    }

    private static TimeSpan GetRetryDelayCore(DelayBackoffType type, bool jitter, int attempt, TimeSpan baseDelay, ref double state, Func<double> randomizer)
    {
        if (baseDelay == TimeSpan.Zero)
        {
            return baseDelay;
        }

        if (jitter)
        {
            return type switch
            {
                DelayBackoffType.Constant => ApplyJitter(baseDelay, randomizer),
                DelayBackoffType.Linear => ApplyJitter(TimeSpan.FromMilliseconds(attempt * baseDelay.TotalMilliseconds), randomizer),
                DelayBackoffType.Exponential => DecorrelatedJitterBackoffV2(attempt, baseDelay, ref state, randomizer),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "The retry backoff type is not supported."),
            };
        }

        return type switch
        {
            DelayBackoffType.Constant => baseDelay,
            DelayBackoffType.Linear => attempt * baseDelay,
            DelayBackoffType.Exponential => Math.Pow(ExponentialFactor, attempt) * baseDelay,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "The retry backoff type is not supported."),
        };
    }

    /// <summary>
    /// Generates sleep durations in an exponentially backing-off, jittered manner, making sure to mitigate any correlations.
    /// For example: 850ms, 1455ms, 3060ms.
    /// Per discussion in Polly issue https://github.com/App-vNext/Polly/issues/530, the jitter of this implementation exhibits fewer spikes and a smoother distribution than the AWS jitter formula.
    /// </summary>
    /// <param name="attempt">The current attempt.</param>
    /// <param name="baseDelay">The median delay to target before the first retry, call it <c>f (= f * 2^0).</c>
    /// Choose this value both to approximate the first delay, and to scale the remainder of the series.
    /// Subsequent retries will (over a large sample size) have a median approximating retries at time <c>f * 2^1, f * 2^2 ... f * 2^t</c> etc for try t.
    /// The actual amount of delay-before-retry for try t may be distributed between 0 and <c>f * (2^(t+1) - 2^(t-1)) for t >= 2;</c>
    /// or between 0 and <c>f * 2^(t+1)</c>, for t is 0 or 1.</param>
    /// <param name="prev">The previous state value used for calculations.</param>
    /// <param name="randomizer">The generator to use.</param>
    /// <remarks>
    /// This code was adopted from https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry/blob/master/src/Polly.Contrib.WaitAndRetry/Backoff.DecorrelatedJitterV2.cs.
    /// </remarks>
    private static TimeSpan DecorrelatedJitterBackoffV2(int attempt, TimeSpan baseDelay, ref double prev, Func<double> randomizer)
    {
        // The original author/credit for this jitter formula is @george-polevoy .
        // Jitter formula used with permission as described at https://github.com/App-vNext/Polly/issues/530#issuecomment-526555979
        // Minor adaptations (pFactor = 4.0 and rpScalingFactor = 1 / 1.4d) by @reisenberger, to scale the formula output for easier parameterization to users.

        // A factor used within the formula to help smooth the first calculated delay.
        const double PFactor = 4.0;

        // A factor used to scale the median values of the retry times generated by the formula to be _near_ whole seconds, to aid Polly user comprehension.
        // This factor allows the median values to fall approximately at 1, 2, 4 etc seconds, instead of 1.4, 2.8, 5.6, 11.2.
        const double RpScalingFactor = 1 / 1.4d;

        // Upper-bound to prevent overflow beyond TimeSpan.MaxValue. Potential truncation during conversion from double to long
        // (as described at https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/numeric-conversions)
        // is avoided by the arbitrary subtraction of 1000. Validated by unit-test Backoff_should_not_overflow_to_give_negative_timespan.
        double maxTimeSpanDouble = (double)TimeSpan.MaxValue.Ticks - 1000;

        long targetTicksFirstDelay = baseDelay.Ticks;

        double t = attempt + randomizer();
        double next = Math.Pow(ExponentialFactor, t) * Math.Tanh(Math.Sqrt(PFactor * t));

        double formulaIntrinsicValue = next - prev;
        prev = next;

        return TimeSpan.FromTicks((long)Math.Min(formulaIntrinsicValue * RpScalingFactor * targetTicksFirstDelay, maxTimeSpanDouble));
    }

    private static TimeSpan ApplyJitter(TimeSpan delay, Func<double> randomizer)
    {
        double offset = (delay.TotalMilliseconds * JitterFactor) / 2;
        double randomDelay = (delay.TotalMilliseconds * JitterFactor * randomizer()) - offset;
        double newDelay = delay.TotalMilliseconds + randomDelay;

        return TimeSpan.FromMilliseconds(newDelay);
    }
}