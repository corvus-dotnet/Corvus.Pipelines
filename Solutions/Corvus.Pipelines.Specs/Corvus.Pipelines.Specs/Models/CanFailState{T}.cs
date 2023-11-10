// <copyright file="CanFailState{T}.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Pipelines.Specs.Models;

/// <summary>
/// A loggable state object over a value.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public readonly struct CanFailState<T> : ICanFail, IEquatable<CanFailState<T>>
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
    public T Value { get; }

    /// <inheritdoc/>
    public PipelineStepStatus ExecutionStatus { get; }

    /// <summary>
    /// Conversion to int.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator T(CanFailState<T> value) => value.Value;

    /// <summary>
    /// Conversion from int.
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

    public CanFailState<T> PermanentFailure()
    {
        return new(this.Value, PipelineStepStatus.PermanentFailure);
    }

    public CanFailState<T> TransientFailure()
    {
        return new(this.Value, PipelineStepStatus.TransientFailure);
    }

    public CanFailState<T> Success()
    {
        return new(this.Value, PipelineStepStatus.Success);
    }

    public CanFailState<T> WithValue(T value)
    {
        return new(value, this.ExecutionStatus);
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