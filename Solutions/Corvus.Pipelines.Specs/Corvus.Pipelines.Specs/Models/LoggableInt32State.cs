// <copyright file="LoggableInt32State.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Corvus.Pipelines.Specs.Models;

/// <summary>
/// A loggable state object over an <see langword="int"/>.
/// </summary>
public readonly struct LoggableInt32State :
    IValueProvider<int>,
    ILoggable,
    IEquatable<LoggableInt32State>
{
    private LoggableInt32State(int value, ILogger logger)
    {
        this.Value = value;
        this.Logger = logger;
    }

    /// <summary>
    /// Gets the value of the state.
    /// </summary>
    public int Value { get; init; }

    /// <inheritdoc/>
    public ILogger Logger { get; init; }

    /// <summary>
    /// Conversion to int.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator int(LoggableInt32State value) => value.Value;

    /// <summary>
    /// Conversion from int.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator LoggableInt32State(int value) => new(value, NullLogger.Instance);

    public static bool operator ==(LoggableInt32State left, LoggableInt32State right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(LoggableInt32State left, LoggableInt32State right)
    {
        return !(left == right);
    }

    public static LoggableInt32State For(int value, ILogger? logger = null)
    {
        return new(value, logger ?? NullLogger.Instance);
    }

    public bool Equals(LoggableInt32State other)
    {
        return this.Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is LoggableInt32State cancellableInt32State && this.Equals(cancellableInt32State);
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