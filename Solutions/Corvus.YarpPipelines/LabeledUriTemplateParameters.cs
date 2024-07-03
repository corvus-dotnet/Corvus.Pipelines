// <copyright file="LabeledUriTemplateParameters.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

using Corvus.UriTemplates;

namespace Corvus.YarpPipelines;

/// <summary>
/// One or more labelled URI template parameter sets.
/// </summary>
public readonly struct LabeledUriTemplateParameters
{
    private readonly object? entryOrCollection;

    /// <summary>
    /// Creates a single-entry <see cref="LabeledUriTemplateParameters"/>.
    /// </summary>
    /// <param name="label">The label.</param>
    /// <param name="parameters">The parameters.</param>
    /// <param name="sourceUri">
    /// The URI that was passed when parsing the URI, and which should be used when extracting
    /// parameter values later.
    /// </param>
    private LabeledUriTemplateParameters(
        string label,
        UriTemplateParameters parameters,
        ReadOnlyMemory<char> sourceUri)
    {
        this.entryOrCollection = new Entry(label, parameters, sourceUri);
    }

    private LabeledUriTemplateParameters(Entry[] entries)
    {
        this.entryOrCollection = entries;
    }

    /// <summary>
    /// Reports whether a parameter set with the specified label is present.
    /// </summary>
    /// <param name="label">The parameter set label.</param>
    /// <returns><see langword="true"/> if parameters for the specified label were found.</returns>
    public bool Has(string label) => this.TryGetParameters(label, out _);

    /// <summary>
    /// Tries to get the parameters for the specified label.
    /// </summary>
    /// <param name="label">The parameter set label.</param>
    /// <param name="parameters">The parameters, if found.</param>
    /// <returns><see langword="true"/> if parameters for the specified label were found.</returns>
    public bool TryGetParameters(string label, [NotNullWhen(true)]out UriTemplateParameters? parameters)
    {
        Entry entry = this.FindEntry(label);
        parameters = entry.Parameters;
        return entry.Label is not null;
    }

    /// <summary>
    /// Tries to get the parameters for the specified label.
    /// </summary>
    /// <param name="label">The parameter set label.</param>
    /// <param name="parameterName">The parameter name.</param>
    /// <returns>The parameter value if found, an empty span otherwise.</returns>
    public ReadOnlySpan<char> GetParameter(
        string label,
        ReadOnlySpan<char> parameterName)
    {
        Entry entry = this.FindEntry(label);
        return entry.Label is not null &&
            entry.Parameters.TryGet(parameterName, out ParameterValue value)
            ? value.GetValue(entry.SourceUri.Span) : ReadOnlySpan<char>.Empty;
    }

    /// <summary>
    /// Returns a new <see cref="LabeledUriTemplateParameters"/> with the specified parameters.
    /// </summary>
    /// <param name="label">The label for this paremeter set.</param>
    /// <param name="parameters">The parameter set to add.</param>
    /// <param name="sourceUri">
    /// The URI that was passed when parsing the URI, and which should be used when extracting
    /// parameter values later.
    /// </param>
    /// <returns>
    /// A new <see cref="LabeledUriTemplateParameters"/> with the specified parameters.
    /// </returns>
    internal LabeledUriTemplateParameters With(
        string label,
        UriTemplateParameters parameters,
        ReadOnlyMemory<char> sourceUri)
    {
        return this.entryOrCollection switch
        {
            null => new LabeledUriTemplateParameters(label, parameters, sourceUri),
            Entry existingEntry => CreateCollection(existingEntry, label, parameters, sourceUri),
            Entry[] existingEntries => AddToCollection(existingEntries, label, parameters, sourceUri),
            _ => throw new InvalidOperationException($"Unexpected internal state type: {this.entryOrCollection.GetType()}"),
        };
    }

    private static LabeledUriTemplateParameters AddToCollection(
        Entry[] existingEntries, string label, UriTemplateParameters parameters, ReadOnlyMemory<char> sourceUri)
    {
        // Sadly we have to reallocate every time to preserve immutability of pipeline state.
        // The normal case is that there is exactly one entry, so we have optimized for the
        // one tuple case.
        var entries = new Entry[existingEntries.Length + 1];
        for (int i = 0; i < existingEntries.Length; ++i)
        {
            if (existingEntries[i].Label.Equals(label, StringComparison.Ordinal))
            {
                throw new ArgumentException("Label already exists", nameof(label));
            }

            entries[i] = existingEntries[i];
        }

        entries[^1] = new Entry(label, parameters, sourceUri);
        return new LabeledUriTemplateParameters(entries);
    }

    private static LabeledUriTemplateParameters CreateCollection(
        Entry existingEntry,
        string label,
        UriTemplateParameters parameters,
        ReadOnlyMemory<char> sourceUri)
    {
        if (existingEntry.Label.Equals(label, StringComparison.Ordinal))
        {
            throw new ArgumentException("Label already exists", nameof(label));
        }

        Entry[] entries = [existingEntry, new Entry(label, parameters, sourceUri)];

        return new LabeledUriTemplateParameters(entries);
    }

    private Entry FindEntry(string label)
    {
        return this.entryOrCollection switch
        {
            null => default,
            Entry entry => entry.Label.Equals(label, StringComparison.Ordinal)
                ? entry : default,
            Entry[] entries => FindEntry(label, entries),
            _ => throw new InvalidOperationException($"Unexpected internal state type: {this.entryOrCollection.GetType()}"),
        };

        static Entry FindEntry(string label, Entry[] entries)
        {
            foreach (Entry entry in entries)
            {
                if (entry.Label.Equals(label, StringComparison.Ordinal))
                {
                    return entry;
                }
            }

            return default;
        }
    }

    private readonly record struct Entry(string Label, UriTemplateParameters Parameters, ReadOnlyMemory<char> SourceUri);
}