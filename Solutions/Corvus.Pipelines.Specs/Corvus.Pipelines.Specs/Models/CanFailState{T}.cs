// <copyright file="CanFailState{T}.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Pipelines.Specs.Models;

/// <summary>
/// An ICanFail state object over a value.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public readonly struct CanFailState<T> :
    IValueProvider<CanFailState<T>, T>,
    ICanFail<CanFailState<T>>,
    IEquatable<CanFailState<T>>
    where T : notnull, IEquatable<T>
{
    private CanFailState(T value, PipelineStepStatus executionStatus)
    {
        this.Value = value;
        this.ExecutionStatus = executionStatus;
    }

    /// <summary>
    /// Gets the value of the state.
    /// </summary>
    public T Value { get; init; }

    /// <inheritdoc/>
    public PipelineStepStatus ExecutionStatus { get; init; }

    /// <summary>
    /// Conversion to value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator T(CanFailState<T> value) => value.Value;

    /// <summary>
    /// Conversion from value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator CanFailState<T>(T value) => new(value, default);

    public static bool operator ==(CanFailState<T> left, CanFailState<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CanFailState<T> left, CanFailState<T> right)
    {
        return !(left == right);
    }

    public static CanFailState<T> For(T value)
    {
        return new(value, default);
    }

    public bool Equals(CanFailState<T> other)
    {
        return this.Value.Equals(other.Value) && this.ExecutionStatus == other.ExecutionStatus;
    }

    public override bool Equals(object? obj)
    {
        return obj is CanFailState<T> cancellableInt32State && this.Equals(cancellableInt32State);
    }

    public override int GetHashCode()
    {
        return this.Value.GetHashCode();
    }

    public override string? ToString()
    {
        return this.Value.ToString();
    }
}