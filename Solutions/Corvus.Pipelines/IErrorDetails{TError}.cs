// <copyright file="IErrorDetails{TError}.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Pipelines;

/// <summary>
/// Adds the ability to provide error details.
/// </summary>
/// <typeparam name="TError">The type of the error details.</typeparam>
public interface IErrorDetails<TError> : ICanFail
    where TError : notnull
{
    /// <summary>
    /// Gets the error details if available.
    /// </summary>
    TError ErrorDetails { get; init; }
}