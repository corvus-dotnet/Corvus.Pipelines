﻿// <copyright file="CanFailInt32State.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Pipelines.Specs.Models;

/// <summary>
/// A loggable state object over an <see langword="int"/>.
/// </summary>
public readonly struct CanFailInt32State : ICanFail, IEquatable<CanFailInt32State>
{
    private CanFailInt32State(int value, PipelineStepStatus executionStatus)
    {
        this.Value = value;
        this.ExecutionStatus = executionStatus;
    }

    /// <summary>
    /// Gets the value of the state.
    /// </summary>
    public int Value { get; }

    public PipelineStepStatus ExecutionStatus { get; }

    /// <summary>
    /// Conversion to int.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator int(CanFailInt32State value) => value.Value;

    /// <summary>
    /// Conversion from int.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator CanFailInt32State(int value) => new(value, default);

    public static bool operator ==(CanFailInt32State left, CanFailInt32State right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CanFailInt32State left, CanFailInt32State right)
    {
        return !(left == right);
    }

    public static CanFailInt32State For(int value)
    {
        return new(value, default);
    }

    public CanFailInt32State PermanentFailure()
    {
        return new(this.Value, PipelineStepStatus.PermanentFailure);
    }

    public CanFailInt32State TransientFailure()
    {
        return new(this.Value, PipelineStepStatus.TransientFailure);
    }

    public CanFailInt32State Success()
    {
        return new(this.Value, PipelineStepStatus.Success);
    }

    public CanFailInt32State WithValue(int value)
    {
        return new(value, this.ExecutionStatus);
    }

    public bool Equals(CanFailInt32State other)
    {
        return this.Value == other.Value && this.ExecutionStatus == other.ExecutionStatus;
    }

    public override bool Equals(object? obj)
    {
        return obj is CanFailInt32State cancellableInt32State && this.Equals(cancellableInt32State);
    }

    public override int GetHashCode()
    {
        return this.Value.GetHashCode();
    }

    public override string ToString()
    {
        return this.Value.ToString();
    }
}