// <copyright file="ValueProviderExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Pipelines;

/// <summary>
/// Extension methods for the <see cref="IValueProvider{TSelf, TValue}"/> capability.
/// </summary>
public static class ValueProviderExtensions
{
    /// <summary>
    /// Updates the value of the state.
    /// </summary>
    /// <typeparam name="TState">The type of the <see cref="IValueProvider{TSelf, TValue}"/>.</typeparam>
    /// <typeparam name="TValue">The type of the value in the state.</typeparam>
    /// <param name="state">The state to update.</param>
    /// <param name="value">The new value for the state.</param>
    /// <returns>The value of the state.</returns>
    public static TState WithValue<TState, TValue>(this TState state, TValue value)
        where TState : struct, IValueProvider<TState, TValue>
    {
        return state with { Value = value };
    }
}