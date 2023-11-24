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
public readonly record struct RetryContext<TState>(TState State, TimeSpan RetryDuration, int FailureCount)
    where TState : struct, ICanFail<TState>;