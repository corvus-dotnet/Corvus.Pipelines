// <copyright file="CookieDetails.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.YarpPipelines;

/// <summary>
/// An action for a cookie.
/// </summary>
public enum CookieAction
{
    /// <summary>
    /// Explicitly add the cookie.
    /// </summary>
    Add,

    /// <summary>
    /// Ensures the cookie is not present, unless added back later.
    /// </summary>
    EnsureRemoved,
}

/// <summary>
/// Details of a cookie operation for <see cref="NonForwardedResponseDetails"/>.
/// </summary>
/// <param name="Path">The path for the cookie.</param>
/// <param name="Name">The name of the cookie.</param>
/// <param name="Value">The value of the cookie.</param>
/// <param name="ExpiresFrom">The date from which the cookie should expire.</param>
/// <param name="Action">The action to take with the cookie.</param>
public readonly record struct CookieDetails(string Path, string Name, string Value, DateTimeOffset ExpiresFrom, CookieAction Action);