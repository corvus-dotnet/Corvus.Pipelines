// <copyright file="IValueProvider{TSelf,TValue}.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Pipelines;

/// <summary>
/// Add a single value to a state.
/// </summary>
/// <typeparam name="TSelf">Type implementing the capability.</typeparam>
/// <typeparam name="TValue">The type of the value to provide.</typeparam>
public interface IValueProvider<TSelf, TValue>
    where TSelf : struct, IValueProvider<TSelf, TValue>
{
    /// <summary>
    /// Gets the value.
    /// </summary>
    TValue Value { get; init; }
}