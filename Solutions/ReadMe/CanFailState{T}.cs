// <copyright file="CanFailState{T}.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Corvus.Pipelines;

namespace ReadMe;

/// <summary>
/// An ICanFail state object over a value.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public readonly struct CanFailState<T> :
    IValueProvider<T>,
    ICanFail
{
    private CanFailState(T value, PipelineStepStatus executionStatus)
    {
        this.Value = value;
        this.ExecutionStatus = executionStatus;
    }

    /// <inheritdoc/>
    public T Value { get; init;  }

    /// <inheritdoc/>
    public PipelineStepStatus ExecutionStatus { get; init; }

    internal static CanFailState<T> For(T value)
    {
        return new(value, default);
    }
}

public static class CanFailState
{
    public static CanFailState<T> For<T>(T value)
    {
        return CanFailState<T>.For(value);
    }
}