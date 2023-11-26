// <copyright file="NonNullableString.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Pipelines;

/// <summary>
/// A non-nullable string.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="NonNullableString"/> struct.
/// </remarks>
/// <param name="value">The string from which to create the value.</param>
/// <remarks>
/// The <see cref="PipelineStep{TState}"/> requires the state to be a struct,
/// which precludes the use of a string as the state type. While you are unlikely
/// to want to use a string as the state in a production pipeline, it is a useful
/// thing to be able to do in e.g. testing and exploration.
/// </remarks>
public readonly struct NonNullableString(string value)
        : IComparable<NonNullableString>, IEquatable<NonNullableString>
{
    private readonly string? value = value;

    /// <summary>
    /// Gets a value indicating whether this is a default string, rather than an
    /// explicitly empty string.
    /// </summary>
    public bool IsDefault => this.value is null;

    /// <summary>
    /// Gets the value of the string.
    /// </summary>
    public string Value => this.value ?? string.Empty;

    /// <summary>
    /// Conversion from <see langword="string"/>.
    /// </summary>
    /// <param name="value">The value from which to convert.</param>
    public static implicit operator NonNullableString(string value) => new(value);

    /// <summary>
    /// Conversion to <see langword="string"/>.
    /// </summary>
    /// <param name="value">The value to which to convert.</param>
    public static implicit operator string(NonNullableString value) => value.Value;

    /// <summary>
    /// Equality operator.
    /// </summary>
    /// <param name="left">LHS.</param>
    /// <param name="right">RHS.</param>
    /// <returns><see langword="true"/> if the left is equal to the right.</returns>
    public static bool operator ==(NonNullableString left, NonNullableString right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    /// <param name="left">LHS.</param>
    /// <param name="right">RHS.</param>
    /// <returns><see langword="true"/> if the left is not equal to the right.</returns>
    public static bool operator !=(NonNullableString left, NonNullableString right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Less than operator.
    /// </summary>
    /// <param name="left">LHS.</param>
    /// <param name="right">RHS.</param>
    /// <returns><see langword="true"/> if the left is less than the right.</returns>
    public static bool operator <(NonNullableString left, NonNullableString right)
    {
        return left.CompareTo(right) < 0;
    }

    /// <summary>
    /// Less than or equals operator.
    /// </summary>
    /// <param name="left">LHS.</param>
    /// <param name="right">RHS.</param>
    /// <returns><see langword="true"/> if the left is less than or equal to the right.</returns>
    public static bool operator <=(NonNullableString left, NonNullableString right)
    {
        return left.CompareTo(right) <= 0;
    }

    /// <summary>
    /// Greater than operator.
    /// </summary>
    /// <param name="left">LHS.</param>
    /// <param name="right">RHS.</param>
    /// <returns><see langword="true"/> if the left is greater than the right.</returns>
    public static bool operator >(NonNullableString left, NonNullableString right)
    {
        return left.CompareTo(right) > 0;
    }

    /// <summary>
    /// Greater than or equals operator.
    /// </summary>
    /// <param name="left">LHS.</param>
    /// <param name="right">RHS.</param>
    /// <returns><see langword="true"/> if the left is greater than or equal to the right.</returns>
    public static bool operator >=(NonNullableString left, NonNullableString right)
    {
        return left.CompareTo(right) >= 0;
    }

    /// <summary>
    /// Compare two strings for equality.
    /// </summary>
    /// <param name="x">The first string.</param>
    /// <param name="y">The second string.</param>
    /// <param name="comparisonType">The comparison type.</param>
    /// <returns><see langword="true"/> if the strings are equal.</returns>
    public static bool Equals(NonNullableString x, NonNullableString y, StringComparison comparisonType)
    {
        return string.Equals(x.value, y.value, comparisonType);
    }

    /// <inheritdoc/>
    public bool Equals(NonNullableString other)
    {
        return string.Equals(this.value, other.value);
    }

    /// <inheritdoc/>
    public int CompareTo(NonNullableString other)
    {
        return string.Compare(this.value, other.value);
    }

    /// <summary>
    /// Compare with a string comparison type.
    /// </summary>
    /// <param name="other">The value to which to compare.</param>
    /// <param name="comparisonType">The comparison type.</param>
    /// <returns>-1 if this is less than other, 0 if this equals other, or +1 if this is greater than other.</returns>
    public int CompareTo(NonNullableString other, StringComparison comparisonType)
    {
        return string.Compare(this.value, other.value, comparisonType);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is NonNullableString other)
        {
            return this.Equals(other);
        }

        if (obj is string otherString)
        {
            return otherString.Equals(this.value);
        }

        return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return this.value?.GetHashCode() ?? 0;
    }

    /// <summary>
    /// Get a hash code for the string.
    /// </summary>
    /// <param name="comparisonType">The comparison type.</param>
    /// <returns>The hash code for the string.</returns>
    public int GetHashCode(StringComparison comparisonType)
    {
        return this.value?.GetHashCode(comparisonType) ?? 0;
    }
}