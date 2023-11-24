// <copyright file="IErrorDetails{TSelf,TError}.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Pipelines;

/// <summary>
/// Adds the ability to provide error details.
/// </summary>
/// <typeparam name="TSelf">The type implementing the capability.</typeparam>
/// <typeparam name="TError">The type of the error details.</typeparam>
public interface IErrorDetails<TSelf, TError> : ICanFail<TSelf>
    where TError : notnull
    where TSelf : struct, IErrorDetails<TSelf, TError>
{
    /// <summary>
    /// Gets the error details if available.
    /// </summary>
    TError ErrorDetails { get; init; }
}