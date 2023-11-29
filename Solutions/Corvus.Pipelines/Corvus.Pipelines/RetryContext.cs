// <copyright file="RetryContext.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Pipelines;

/// <summary>
/// The context of a pipeline retry operation.
/// </summary>
/// <typeparam name="TState">The type of the pipeline step state.</typeparam>
/// <param name="State">The current pipeline step state.</param>
/// <param name="RetryDuration">The total duration of the retried operation.</param>
/// <param name="FailureCount">The total number of failures experienced during the retry operation.</param>
/// <param name="CorrelationBase">The last-used randomization base for decorrelation between attempts.</param>
public readonly record struct RetryContext<TState>(TState State, TimeSpan RetryDuration, int FailureCount, double CorrelationBase)
    where TState : struct, ICanFail<TState>
{
    /// <summary>
    /// Gets and updated context with the given state.
    /// </summary>
    /// <param name="state">The new state.</param>
    /// <returns>The updated context.</returns>
    public RetryContext<TState> WithState(TState state)
    {
        return this with { State = state };
    }
}