// <copyright file="ErrorDetailsExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Corvus.Pipelines;

/// <summary>
/// Extensions for the <see cref="IErrorDetails{TError}"/> interface.
/// </summary>
public static class ErrorDetailsExtensions
{
    /// <summary>
    /// Try to get the error details for the current state.
    /// </summary>
    /// <typeparam name="TCapability">The type of the capability.</typeparam>
    /// <typeparam name="TErrorDetails">The error details.</typeparam>
    /// <param name="capability">The <see cref="IErrorDetails{TError}"/> capable state.</param>
    /// <param name="errorDetails">The error details, if any.</param>
    /// <returns><see langword="true"/> if error details were available, otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetErrorDetails<TCapability, TErrorDetails>(this TCapability capability, [NotNullWhen(true)] out TErrorDetails errorDetails)
        where TCapability : struct, IErrorDetails<TErrorDetails>
        where TErrorDetails : notnull
    {
        errorDetails = capability.ErrorDetails;
        return capability.ExecutionStatus != PipelineStepStatus.Success;
    }

    /// <summary>
    /// Create an updated instance of the state for a transient failure with the given error details.
    /// </summary>
    /// <typeparam name="TCapability">The type of the capability.</typeparam>
    /// <typeparam name="TErrorDetails">The error details.</typeparam>
    /// <param name="capability">The <see cref="IErrorDetails{TError}"/> capable state.</param>
    /// <param name="errorDetails">The error details, if any.</param>
    /// <returns>The updated state.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TCapability TransientFailure<TCapability, TErrorDetails>(this TCapability capability, TErrorDetails errorDetails)
        where TCapability : struct, IErrorDetails<TErrorDetails>
        where TErrorDetails : notnull
    {
        return capability with { ErrorDetails = errorDetails, ExecutionStatus = PipelineStepStatus.TransientFailure };
    }

    /// <summary>
    /// Create an updated instance of the state for a permanent failure with the given error details.
    /// </summary>
    /// <typeparam name="TCapability">The type of the capability.</typeparam>
    /// <typeparam name="TErrorDetails">The error details.</typeparam>
    /// <param name="capability">The <see cref="IErrorDetails{TError}"/> capable state.</param>
    /// <param name="errorDetails">The error details, if any.</param>
    /// <returns>The updated state.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TCapability PermanentFailure<TCapability, TErrorDetails>(this TCapability capability, TErrorDetails errorDetails)
        where TCapability : struct, IErrorDetails<TErrorDetails>
        where TErrorDetails : notnull
    {
        return capability with { ErrorDetails = errorDetails, ExecutionStatus = PipelineStepStatus.PermanentFailure };
    }
}