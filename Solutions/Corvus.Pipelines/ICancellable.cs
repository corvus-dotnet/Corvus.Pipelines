// <copyright file="ICancellable.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Pipelines;

/// <summary>
/// A state entity with cancellability.
/// </summary>
/// <typeparam name="TSelf">The type implementing the capability.</typeparam>
public interface ICancellable<TSelf>
    where TSelf : struct, ICancellable<TSelf>
{
    /// <summary>
    /// Gets the cancellation token for the state.
    /// </summary>
    CancellationToken CancellationToken { get; init; }
}