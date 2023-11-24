// <copyright file="CanFailExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;

namespace Corvus.Pipelines;

/// <summary>
/// Extension methods for <see cref="ICanFail{TSelf}"/>.
/// </summary>
public static class CanFailExtensions
{
    /// <summary>
    /// Get the value with <see cref="ICanFail{TSelf}.ExecutionStatus"/> equal to <see cref="PipelineStepStatus.PermanentFailure"/>.
    /// </summary>
    /// <typeparam name="T">The type of the state.</typeparam>
    /// <param name="canFail">The state with the <see cref="ICanFail{TSelf}"/> capability.</param>
    /// <returns>The updated state.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T PermanentFailure<T>(this T canFail)
        where T : struct, ICanFail<T>
    {
        return canFail with { ExecutionStatus = PipelineStepStatus.PermanentFailure };
    }

    /// <summary>
    /// Get the value with <see cref="ICanFail{TSelf}.ExecutionStatus"/> equal to <see cref="PipelineStepStatus.TransientFailure"/>.
    /// </summary>
    /// <typeparam name="T">The type of the state.</typeparam>
    /// <param name="canFail">The state with the <see cref="ICanFail{TSelf}"/> capability.</param>
    /// <returns>The updated state.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T TransientFailure<T>(this T canFail)
        where T : struct, ICanFail<T>
    {
        return canFail with { ExecutionStatus = PipelineStepStatus.TransientFailure };
    }

    /// <summary>
    /// Get the value with <see cref="ICanFail{TSelf}.ExecutionStatus"/> equal to <see cref="PipelineStepStatus.Success"/>.
    /// </summary>
    /// <typeparam name="T">The type of the state.</typeparam>
    /// <param name="canFail">The state with the <see cref="ICanFail{TSelf}"/> capability.</param>
    /// <returns>The updated state.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Success<T>(this T canFail)
        where T : struct, ICanFail<T>
    {
        return canFail with { ExecutionStatus = PipelineStepStatus.Success };
    }
}