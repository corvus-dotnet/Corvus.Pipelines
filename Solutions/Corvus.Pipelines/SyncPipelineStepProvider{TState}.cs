// <copyright file="SyncPipelineStepProvider{TState}.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Corvus.Pipelines;

/// <summary>
/// Represents a step with an explicit name.
/// </summary>
/// <typeparam name="TState">The type of the step.</typeparam>
public readonly struct SyncPipelineStepProvider<TState>
    where TState : struct
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SyncPipelineStepProvider{TState}"/> struct.
    /// </summary>
    /// <param name="step">The instance of the step.</param>
    public SyncPipelineStepProvider(in SyncPipelineStep<TState> step)
    {
        this.Step = step;
        this.Features = ImmutableDictionary<string, object>.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncPipelineStepProvider{TState}"/> struct.
    /// </summary>
    /// <param name="step">The instance of the step.</param>
    /// <param name="features">The features for the step.</param>
    private SyncPipelineStepProvider(in SyncPipelineStep<TState> step, in ImmutableDictionary<string, object> features)
    {
        this.Step = step;
        this.Features = features;
    }

    /// <summary>
    /// Gets the step.
    /// </summary>
    public SyncPipelineStep<TState> Step { get; }

    /// <summary>
    /// Gets the features for the step.
    /// </summary>
    internal ImmutableDictionary<string, object> Features { get; }

    /// <summary>
    /// Add a feature to the step.
    /// </summary>
    /// <typeparam name="T">The type of the feature to add.</typeparam>
    /// <param name="name">The name of the feature.</param>
    /// <param name="feature">The feature to add.</param>
    /// <returns>The instance of the requested feature.</returns>
    public SyncPipelineStepProvider<TState> AddFeature<T>(string name, T feature)
        where T : notnull
    {
        return new(this.Step, this.Features.Add(name, feature));
    }

    /// <summary>
    /// Gets a required feature.
    /// </summary>
    /// <typeparam name="T">The type of the feature.</typeparam>
    /// <param name="name">The name of the feature.</param>
    /// <returns>An instance of the required feature.</returns>
    /// <exception cref="ArgumentException">No feature was registered with that name and type.</exception>
    public T GetRequiredFeature<T>(string name)
        where T : notnull
    {
        if (!this.Features.TryGetValue(name, out object? value))
        {
            throw new ArgumentException($"The feature {name} was not available.", nameof(name));
        }

        if (value is T feature)
        {
            return feature;
        }

        throw new ArgumentException($"The feature {name} was not of type {typeof(T)}.", nameof(name));
    }

    /// <summary>
    /// Tries to get a feature.
    /// </summary>
    /// <typeparam name="T">The type of the feature.</typeparam>
    /// <param name="name">The name of the feature.</param>
    /// <param name="feature">The resulting value of the feature.</param>
    /// <returns><see langword="true"/> if the feature of that name and type was available.</returns>
    public bool TryGetFeature<T>(string name, [NotNullWhen(true)] out T? feature)
        where T : notnull
    {
        if (this.Features.TryGetValue(name, out object? value) && value is T v)
        {
            feature = v;
            return true;
        }

        feature = default;
        return false;
    }
}