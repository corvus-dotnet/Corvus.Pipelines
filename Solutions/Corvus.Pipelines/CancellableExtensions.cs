// <copyright file="CancellableExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;

namespace Corvus.Pipelines;

/// <summary>
/// Extension methods for <see cref="ICancellable{TState}"/>.
/// </summary>
public static class CancellableExtensions
{
    /// <summary>
    /// Returns a version of the state augmented with a cancellation token.
    /// </summary>
    /// <typeparam name="TCapability">The type of the <see cref="ICancellable{TState}"/> capability.</typeparam>
    /// <param name="capability">The cancellable state.</param>
    /// <param name="cancellationToken">The cancellation token which will be used to signal cancellation.</param>
    /// <returns>The state, with the cancellation token set.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TCapability WithCancellationToken<TCapability>(this TCapability capability, CancellationToken cancellationToken)
        where TCapability : struct, ICancellable<TCapability>
    {
        return capability with { CancellationToken = cancellationToken };
    }
}