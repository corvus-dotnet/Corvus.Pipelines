// <copyright file="ErrorDetailsExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Corvus.Pipelines;

/// <summary>
/// Extensions for the <see cref="IErrorProvider{TSelf, TError}"/> interface.
/// </summary>
public static class ErrorDetailsExtensions
{
    /// <summary>
    /// Try to get the error details for the current state.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TErrorDetails">The error details.</typeparam>
    /// <param name="capability">The <see cref="IErrorProvider{TSelf, TError}"/> capable state.</param>
    /// <param name="errorDetails">The error details, if any.</param>
    /// <returns><see langword="true"/> if error details were available, otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetErrorDetails<TState, TErrorDetails>(this TState capability, [NotNullWhen(true)] out TErrorDetails? errorDetails)
        where TState : struct, IErrorProvider<TState, TErrorDetails>
        where TErrorDetails : notnull
    {
        Debug.Assert(
            capability.ExecutionStatus == PipelineStepStatus.Success || errorDetails is not null,
            "Error provider should not be in a failed state with null error details");
        errorDetails = capability.ErrorDetails;
        return capability.ExecutionStatus != PipelineStepStatus.Success;
    }

    /// <summary>
    /// Create an updated instance of the state for a transient failure with the given error details.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TErrorDetails">The error details.</typeparam>
    /// <param name="capability">The <see cref="IErrorProvider{TSelf, TError}"/> capable state.</param>
    /// <param name="errorDetails">The error details, if any.</param>
    /// <returns>The updated state.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TState TransientFailure<TState, TErrorDetails>(
        this TState capability,
        in TErrorDetails errorDetails)
        where TState : struct, IErrorProvider<TState, TErrorDetails>
        where TErrorDetails : notnull
    {
        // TErrorDetails might be a struct, but in those cases, the JIT does not generate
        // either a box or code that compares this with default(TErrorDetails). Instead, in
        // Tier 0 JIT, it generates code that always skips past this (but at Tier 0 it
        // hasn't optimized it away entirely - oddly, it zeros out a register and then
        // compares it with 1).
        // We're not using ArgumentNullException.ThrowIfNull because that would box.
        if (errorDetails is null)
        {
            throw new ArgumentNullException(nameof(errorDetails));
        }

        return capability with { ErrorDetails = errorDetails, ExecutionStatus = PipelineStepStatus.TransientFailure };
    }

    /// <summary>
    /// Create an updated instance of the state for a permanent failure with the given error details.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TErrorDetails">The error details.</typeparam>
    /// <param name="capability">The <see cref="IErrorProvider{TSelf, TError}"/> capable state.</param>
    /// <param name="errorDetails">The error details, if any.</param>
    /// <returns>The updated state.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TState PermanentFailure<TState, TErrorDetails>(
        this TState capability,
        in TErrorDetails errorDetails)
        where TState : struct, IErrorProvider<TState, TErrorDetails>
        where TErrorDetails : notnull
    {
        if (errorDetails is null)
        {
            throw new ArgumentNullException(nameof(errorDetails));
        }

        return capability with { ErrorDetails = errorDetails, ExecutionStatus = PipelineStepStatus.PermanentFailure };
    }
}