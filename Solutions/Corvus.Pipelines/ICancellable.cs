// <copyright file="ICancellable.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Pipelines;

/// <summary>
/// A state entity with cancellability.
/// </summary>
/// <typeparam name="TState">The type of the cancellable state.</typeparam>
public interface ICancellable<TState>
    where TState : struct, ICancellable<TState>
{
    /// <summary>
    /// Gets the cancellation token for the state.
    /// </summary>
    CancellationToken CancellationToken { get; init; }
}