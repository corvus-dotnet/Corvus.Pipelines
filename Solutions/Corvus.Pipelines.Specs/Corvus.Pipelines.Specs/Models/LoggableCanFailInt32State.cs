// <copyright file="LoggableCanFailInt32State.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;

namespace Corvus.Pipelines.Specs.Models;

/// <summary>
/// A loggable state object over an <see langword="int"/>.
/// </summary>
public readonly struct LoggableCanFailInt32State : ICanFail, ILoggable, IEquatable<LoggableCanFailInt32State>
{
    private LoggableCanFailInt32State(int value, PipelineStepStatus executionStatus, ILogger logger)
    {
        this.Value = value;
        this.ExecutionStatus = executionStatus;
        this.Logger = logger;
    }

    /// <summary>
    /// Gets the value of the state.
    /// </summary>
    public int Value { get; init; }

    /// <inheritdoc/>
    public PipelineStepStatus ExecutionStatus { get; init; }

    /// <inheritdoc/>
    public ILogger Logger { get; init; }

    /// <summary>
    /// Conversion to int.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator int(LoggableCanFailInt32State value) => value.Value;

    public static bool operator ==(LoggableCanFailInt32State left, LoggableCanFailInt32State right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(LoggableCanFailInt32State left, LoggableCanFailInt32State right)
    {
        return !(left == right);
    }

    public static LoggableCanFailInt32State For(int value, ILogger logger)
    {
        return new(value, default, logger);
    }

    public LoggableCanFailInt32State PermanentFailure()
    {
        return new(this.Value, PipelineStepStatus.PermanentFailure, this.Logger);
    }

    public LoggableCanFailInt32State TransientFailure()
    {
        return new(this.Value, PipelineStepStatus.TransientFailure, this.Logger);
    }

    public LoggableCanFailInt32State Success()
    {
        return new(this.Value, PipelineStepStatus.Success, this.Logger);
    }

    public LoggableCanFailInt32State WithValue(int value)
    {
        return new(value, this.ExecutionStatus, this.Logger);
    }

    public bool Equals(LoggableCanFailInt32State other)
    {
        return this.Value == other.Value && this.ExecutionStatus == other.ExecutionStatus;
    }

    public override bool Equals(object? obj)
    {
        return obj is LoggableCanFailInt32State cancellableInt32State && this.Equals(cancellableInt32State);
    }

    public override int GetHashCode()
    {
        return this.Value.GetHashCode();
    }

    public override string ToString()
    {
        return $"({this.Value},{Enum.GetName(this.ExecutionStatus)})";
    }
}