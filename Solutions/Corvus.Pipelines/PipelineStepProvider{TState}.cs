﻿// <copyright file="PipelineStepProvider{TState}.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Corvus.Pipelines;

/// <summary>
/// Represents a step with an explicit name.
/// </summary>
/// <typeparam name="TState">The type of the step.</typeparam>
public readonly struct PipelineStepProvider<TState>
    where TState : struct
{
    private readonly ImmutableDictionary<string, object> features;

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineStepProvider{TState}"/> struct.
    /// </summary>
    /// <param name="step">The instance of the step.</param>
    public PipelineStepProvider(in PipelineStep<TState> step)
    {
        this.Step = step;
        this.features = ImmutableDictionary<string, object>.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineStepProvider{TState}"/> struct.
    /// </summary>
    /// <param name="step">The instance of the sync step to wrap.</param>
    internal PipelineStepProvider(in SyncPipelineStepProvider<TState> step)
    {
        SyncPipelineStep<TState> syncStep = step.Step;
        this.Step = state => ValueTask.FromResult(syncStep(state));
        this.features = step.Features;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineStepProvider{TState}"/> struct.
    /// </summary>
    /// <param name="step">The instance of the step.</param>
    /// <param name="features">The features for the step.</param>
    private PipelineStepProvider(in PipelineStep<TState> step, in ImmutableDictionary<string, object> features)
    {
        this.Step = step;
        this.features = features;
    }

    /// <summary>
    /// Gets the step.
    /// </summary>
    public PipelineStep<TState> Step { get; }

    /// <summary>
    /// Add a feature to the step.
    /// </summary>
    /// <typeparam name="T">The type of the feature to add.</typeparam>
    /// <param name="name">The name of the feature.</param>
    /// <param name="feature">The feature to add.</param>
    /// <returns>The instance of the requested feature.</returns>
    public PipelineStepProvider<TState> AddFeature<T>(string name, T feature)
        where T : notnull
    {
        return new(this.Step, this.features.Add(name, feature));
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
        if (!this.features.TryGetValue(name, out object? value))
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
        if (this.features.TryGetValue(name, out object? value) && value is T v)
        {
            feature = v;
            return true;
        }

        feature = default;
        return false;
    }
}