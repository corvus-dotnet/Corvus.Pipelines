// <copyright file="StateWithValue{T}.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Corvus.Pipelines;

namespace ReadMe;

public readonly struct StateWithValue<T> :
    IValueProvider<T>
{
    private StateWithValue(T value)
    {
        this.Value = value;
    }

    /// <summary>
    /// Gets the value of the state.
    /// </summary>
    public T Value { get; init; }

    internal static StateWithValue<T> For(T value)
    {
        return new(value);
    }
}

public static class StateWithValue
{
    public static StateWithValue<T> For<T>(T value)
    {
        return StateWithValue<T>.For(value);
    }
}
