// <copyright file="NamedSyncPipelineStep.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Pipelines;

/// <summary>
/// Represents a synchronous step with an explicit name and ID.
/// </summary>
/// <typeparam name="TState">The type of the step.</typeparam>
/// <param name="Name">The Name for the step.</param>
/// <param name="Step">The instance of the step.</param>
public readonly record struct NamedSyncPipelineStep<TState>(in string Name, in SyncPipelineStep<TState> Step)
    where TState : struct;