// <copyright file="NamedPipelineStep.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Pipelines;

/// <summary>
/// Represents a step with an explicit name and ID.
/// </summary>
/// <typeparam name="TState">The type of the step.</typeparam>
/// <param name="Name">The name for the step.</param>
/// <param name="Step">The instance of the step.</param>
public readonly record struct NamedPipelineStep<TState>(in string Name, in PipelineStep<TState> Step)
    where TState : struct;