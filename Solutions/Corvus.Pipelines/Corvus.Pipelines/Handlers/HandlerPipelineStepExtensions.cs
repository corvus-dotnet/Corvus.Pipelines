// <copyright file="HandlerPipelineStepExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Pipelines.Handlers;

/// <summary>
/// Extension methods for working with handler pipeline steps.
/// </summary>
public static class HandlerPipelineStepExtensions
{
    /// <summary>
    /// Evaluates a handler pipeline whose result type is <see langword="bool" />.
    /// </summary>
    /// <typeparam name="TInput">Input type.</typeparam>
    /// <param name="handler">The handler to evaluate.</param>
    /// <param name="input">The input to pass to the handler.</param>
    /// <returns>
    /// <see langword="true"/> if the handler handled the input and returned <see langword="true"/>;
    /// <see langword="false"/> otherwise.
    /// </returns>
    public static bool Evaluate<TInput>(
        this SyncPipelineStep<HandlerState<TInput, bool>> handler,
        TInput input)
    {
        return handler(HandlerState<TInput, bool>.For(input))
            .WasHandled(out bool matched) && matched;
    }
}