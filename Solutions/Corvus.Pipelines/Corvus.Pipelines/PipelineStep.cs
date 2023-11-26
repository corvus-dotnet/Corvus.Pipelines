// <copyright file="PipelineStep.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Pipelines;

/// <summary>
/// An asynchronous step in a <see cref="Pipeline"/>, that operates on an instance
/// of the <typeparamref name="TState"/> and returns a new instance of that state.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
/// <param name="state">The input state.</param>
/// <returns>A <see cref="ValueTask{TState}"/> which, when resolved, provides the updated state.</returns>
public delegate ValueTask<TState> PipelineStep<TState>(TState state)
    where TState : struct;

/// <summary>
/// An asynchronous step in a <see cref="Pipeline"/>, that operates on an instance
/// of the <typeparamref name="TState"/> and returns a new instance of that state.
/// </summary>
/// <typeparam name="TState">The type of the state.</typeparam>
/// <param name="state">The input state.</param>
/// <returns>A <see cref="ValueTask{TState}"/> which, when resolved, provides the updated state.</returns>
public delegate TState SyncPipelineStep<TState>(TState state)
    where TState : struct;