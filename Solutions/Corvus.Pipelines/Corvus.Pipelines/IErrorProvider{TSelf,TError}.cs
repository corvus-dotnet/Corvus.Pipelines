// <copyright file="IErrorProvider{TSelf,TError}.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Pipelines;

/// <summary>
/// Adds the ability to provide error details.
/// </summary>
/// <typeparam name="TSelf">The type implementing the capability.</typeparam>
/// <typeparam name="TError">The type of the error details.</typeparam>
public interface IErrorProvider<TSelf, TError> : ICanFail<TSelf>
    where TSelf : struct, IErrorProvider<TSelf, TError>
    where TError : notnull
{
    /// <summary>
    /// Gets the error details if available.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Be aware of the oddness of using <c>?</c> against the unconstrained type
    /// parameter <typeparamref name="TError"/>. Older versions of C# didn't let you
    /// do this at all because it was not possible for the compiler to generate
    /// one output that worked for both reference and values types. (A
    /// <c>string?</c> is quite different from an <c>int?</c>.) But in response
    /// to popular demand, they decided to allow this in C# 12.0. If
    /// <typeparamref name="TError"/> is a reference type, it means exactly what
    /// it looks like. But if <typeparamref name="TError"/> is a value type, it
    /// is as though the <c>?</c> was not present (i.e. it will not in fact be
    /// nullable).
    /// </para>
    /// </remarks>
    TError? ErrorDetails { get; init; }
}