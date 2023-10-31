// <copyright file="NonForwardedResponseDetails.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http;

namespace Corvus.YarpPipelines;

/// <summary>
/// The information required to build a local response for a non-forwarded pipeline result.
/// </summary>
/// <remarks><see cref="YarpPipelineState.ShouldForward(out NonForwardedResponseDetails)"/> for more details.</remarks>
public readonly struct NonForwardedResponseDetails
{
    private readonly string? redirectLocation;

    private NonForwardedResponseDetails(
        int statusCode, string? redirectLocation)
    {
        this.StatusCode = statusCode;
        this.redirectLocation = redirectLocation;
    }

    /// <summary>
    /// Gets the response status code.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Creates a <see cref="NonForwardedResponseDetails"/> indicating that a particular status code should be returned.
    /// </summary>
    /// <param name="statusCode">The status code.</param>
    /// <returns>A <see cref="NonForwardedResponseDetails"/>.</returns>
    public static NonForwardedResponseDetails ForStatusCode(int statusCode)
    {
        return new(statusCode, null);
    }

    /// <summary>
    /// Creates a <see cref="NonForwardedResponseDetails"/> indicating that a redirect response should be
    /// produced with a status code suitable for scenarios where we are redirecting the
    /// user to a login UI (302).
    /// </summary>
    /// <param name="location">The redirection location.</param>
    /// <returns>A <see cref="NonForwardedResponseDetails"/>.</returns>
    public static NonForwardedResponseDetails ForAuthenticationRedirect(string location)
    {
        return new(StatusCodes.Status302Found, null);
    }

    /// <summary>
    /// Gets the redirect location if there is one.
    /// </summary>
    /// <param name="result">
    /// Set to the redirect details if this value represents a redirect.
    /// </param>
    /// <returns><see langword="true"/> if this was a redirect.</returns>
    public bool TryGetRedirect(out (int StatusCode, string Location) result)
    {
        if (this.redirectLocation is string location)
        {
            result = (this.StatusCode, location);
            return true;
        }

        result = default;
        return false;
    }
}