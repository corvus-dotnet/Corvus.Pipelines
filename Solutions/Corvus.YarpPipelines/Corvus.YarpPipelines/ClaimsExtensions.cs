// <copyright file="ClaimsExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Security.Claims;
using System.Security.Principal;

namespace Corvus.YarpPipelines;

/// <summary>
/// Low-allocation utilities for working with claims.
/// </summary>
internal static class ClaimsExtensions
{
    /// <summary>
    /// A non-allocating version of <see cref="ClaimsPrincipal.Identity"/>.
    /// </summary>
    /// <param name="claimsPrincipal">
    /// The principal from which to extract the primary identity.
    /// </param>
    /// <returns>The primary identity.</returns>
    /// <remarks>
    /// <see cref="ClaimsPrincipal.Identity"/> allocates an enumerator, because
    /// it executes a foreach over the identities. In most cases, these identities
    /// are in an IList, making zero-allocation enumeration possible, and since
    /// the .NET runtime implementation doesn't do that (at least, not in .NET 8.0)
    /// we have to do it ourselves. (See https://github.com/dotnet/runtime/issues/107861).
    /// </remarks>
    public static IIdentity? GetPrimaryIdentity(this ClaimsPrincipal claimsPrincipal)
    {
        if (claimsPrincipal.Identities is IList<ClaimsIdentity> identities)
        {
            for (int i = 0; i < identities.Count; i++)
            {
                if (identities[i] is not null)
                {
                    return identities[i];
                }
            }
        }

        return claimsPrincipal.Identity;
    }
}