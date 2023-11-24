// <copyright file="CancellableInt32State.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Pipelines.Specs.Models;

/// <summary>
/// A cancellable state object over an <see langword="int"/>.
/// </summary>
public readonly struct CancellableInt32State :
    IValueProvider<CancellableInt32State, int>,
    ICancellable<CancellableInt32State>,
    IEquatable<CancellableInt32State>
{
    private CancellableInt32State(int value, CancellationToken cancellationToken)
    {
        this.Value = value;
        this.CancellationToken = cancellationToken;
    }

    /// <summary>
    /// Gets the value of the state.
    /// </summary>
    public int Value { get; init; }

    public CancellationToken CancellationToken { get; init; }

    /// <summary>
    /// Conversion to int.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator int(CancellableInt32State value) => value.Value;

    /// <summary>
    /// Conversion from int.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator CancellableInt32State(int value) => new(value, default);

    public static bool operator ==(CancellableInt32State left, CancellableInt32State right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CancellableInt32State left, CancellableInt32State right)
    {
        return !(left == right);
    }

    public static CancellableInt32State For(int value, CancellationToken cancellationToken = default)
    {
        return new(value, cancellationToken);
    }

    public bool Equals(CancellableInt32State other)
    {
        return this.Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is CancellableInt32State cancellableInt32State && this.Equals(cancellableInt32State);
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