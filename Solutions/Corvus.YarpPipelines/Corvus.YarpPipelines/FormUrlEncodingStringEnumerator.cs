// <copyright file="FormUrlEncodingStringEnumerator.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Collections;

namespace Corvus.YarpPipelines;

/// <summary>
/// Enumerates over the key/value pairs in an <c>application/x-www-form-urlencoded</c>-formatted string, such as
/// the query string in a URL.
/// </summary>
/// <remarks>
/// The spec for this format is owned by WHATWG and can be found at
/// <see href="https://url.spec.whatwg.org/#application/x-www-form-urlencoded"/>.
/// </remarks>
public struct FormUrlEncodingStringEnumerator :
    IEnumerable<(ReadOnlyMemory<char> Name, ReadOnlyMemory<char> Value)>,
    IEnumerator<(ReadOnlyMemory<char> Name, ReadOnlyMemory<char> Value)>
{
    private readonly ReadOnlyMemory<char> source;
    private int currentNameIndex;
    private int nameEnd;
    private int valueStart;
    private int valueEnd;

    /// <summary>
    /// Creates a <see cref="FormUrlEncodingStringEnumerator"/>.
    /// </summary>
    /// <param name="source">The string in which to find name/value pairs.</param>
    public FormUrlEncodingStringEnumerator(ReadOnlyMemory<char> source)
    {
        this.source = source;

        this.currentNameIndex = this.nameEnd = this.valueStart = this.valueEnd = 0;
    }

    /// <inheritdoc/>
    public readonly (ReadOnlyMemory<char> Name, ReadOnlyMemory<char> Value) Current =>
        (this.source[this.currentNameIndex..this.nameEnd], this.source[this.valueStart..this.valueEnd]);

    /// <inheritdoc/>
    readonly object IEnumerator.Current => this.Current;

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Reset();
    }

    /// <inheritdoc/>
    readonly IEnumerator<(ReadOnlyMemory<char> Name, ReadOnlyMemory<char> Value)> IEnumerable<(ReadOnlyMemory<char> Name, ReadOnlyMemory<char> Value)>.GetEnumerator()
        => this;

    /// <summary>
    /// Gets an enumerator.
    /// </summary>
    /// <returns>An enumerator.</returns>
    public readonly FormUrlEncodingStringEnumerator GetEnumerator() => this;

    /// <inheritdoc/>
    public bool MoveNext()
    {
        this.currentNameIndex = this.valueEnd + 1;

        // TODO: qss originates from querystring naming
        ReadOnlySpan<char> qss = this.source.Span;
        if (this.currentNameIndex > qss.Length)
        {
            return false;
        }

        this.valueEnd = qss[this.currentNameIndex..].IndexOf('&');
        if (this.valueEnd == -1)
        {
            this.valueEnd = qss.Length;
        }
        else
        {
            this.valueEnd += this.currentNameIndex;
        }

        this.nameEnd = qss[this.currentNameIndex..this.valueEnd].IndexOf('=');
        if (this.nameEnd == -1)
        {
            // There was no '=' after the name, but that is allowed, it just means the value is empty
            this.nameEnd = this.valueEnd;
            this.valueStart = this.valueEnd;
        }
        else
        {
            this.nameEnd += this.currentNameIndex;
            this.valueStart = this.nameEnd + 1;
        }

        return true;
    }

    /// <inheritdoc/>
    public void Reset()
    {
        this.currentNameIndex = this.nameEnd = this.valueStart = this.valueEnd = 0;
    }

    /// <inheritdoc/>
    readonly IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}