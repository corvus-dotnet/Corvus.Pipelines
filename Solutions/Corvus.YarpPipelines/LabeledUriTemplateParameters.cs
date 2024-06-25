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
    private LabeledUriTemplateParameters(string label, UriTemplateParameters parameters)
    {
        this.entryOrCollection = (label, parameters);
    }

    private LabeledUriTemplateParameters(Entry[] entries)
    {
        this.entryOrCollection = entries;
    }

    /// <summary>
    /// Tries to get the parameters for the specified label.
    /// </summary>
    /// <param name="label">The parameter set label.</param>
    /// <param name="parameters">The parameters, if found.</param>
    /// <returns><see langword="true"/> if parameters for the specified label were found.</returns>
    public bool TryGet(string label, [NotNullWhen(true)]out UriTemplateParameters? parameters)
    {
        parameters = this.entryOrCollection switch
        {
            null => null,
            Entry entry => entry.Label.Equals(label, StringComparison.Ordinal)
                ? entry.Parameters : null,
            Entry[] entries => FindEntry(label, entries),
            _ => throw new InvalidOperationException($"Unexpected internal state type: {this.entryOrCollection.GetType()}"),
        };

        return parameters is not null;

        static UriTemplateParameters? FindEntry(string label, Entry[] entries)
        {
            foreach (Entry entry in entries)
            {
                if (entry.Label.Equals(label, StringComparison.Ordinal))
                {
                    return entry.Parameters;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Returns a new <see cref="LabeledUriTemplateParameters"/> with the specified parameters.
    /// </summary>
    /// <param name="label">The label for this paremeter set.</param>
    /// <param name="parameters">The parameter set to add.</param>
    /// <returns>
    /// A new <see cref="LabeledUriTemplateParameters"/> with the specified parameters.
    /// </returns>
    internal LabeledUriTemplateParameters With(string label, UriTemplateParameters parameters)
    {
        return this.entryOrCollection switch
        {
            null => new LabeledUriTemplateParameters(label, parameters),
            Entry existingEntry => CreateCollection(existingEntry, label, parameters),
            Entry[] existingEntries => AddToCollection(existingEntries, label, parameters),
            _ => throw new InvalidOperationException($"Unexpected internal state type: {this.entryOrCollection.GetType()}"),
        };
    }

    private static LabeledUriTemplateParameters AddToCollection(
        Entry[] existingEntries, string label, UriTemplateParameters parameters)
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

        entries[^1] = new Entry(label, parameters);
        return new LabeledUriTemplateParameters(entries);
    }

    private static LabeledUriTemplateParameters CreateCollection(
        Entry existingEntry,
        string label,
        UriTemplateParameters parameters)
    {
        if (existingEntry.Label.Equals(label, StringComparison.Ordinal))
        {
            throw new ArgumentException("Label already exists", nameof(label));
        }

        Entry[] entries = [existingEntry, new Entry(label, parameters)];

        return new LabeledUriTemplateParameters(entries);
    }

    private readonly record struct Entry(string Label, UriTemplateParameters Parameters);
}