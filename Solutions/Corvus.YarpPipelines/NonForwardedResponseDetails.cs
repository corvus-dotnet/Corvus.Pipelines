// <copyright file="NonForwardedResponseDetails.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Collections.Immutable;

namespace Corvus.YarpPipelines;

/// <summary>
/// The information required to build a local response for a non-forwarded pipeline result.
/// </summary>
/// <remarks><see cref="YarpPipelineState.ShouldForward(out NonForwardedResponseDetails)"/> for more details.</remarks>
public readonly struct NonForwardedResponseDetails
{
    private readonly string? redirectLocation;
    private readonly ImmutableArray<CookieDetails>? cookieDetails;
    private readonly bool redirectShouldPreserveMethod;

    private NonForwardedResponseDetails(int statusCode)
    {
        this.StatusCode = statusCode;
    }

    private NonForwardedResponseDetails(
        string redirectLocation, bool preserveMethod)
    {
        this.redirectLocation = redirectLocation;
        this.redirectShouldPreserveMethod = preserveMethod;
    }

    private NonForwardedResponseDetails(
        string redirectLocation,
        string cookiePath,
        string cookieName,
        string cookieValue,
        DateTimeOffset cookieExpiresFrom,
        CookieAction cookieAction,
        bool preserveMethod)
    {
        this.redirectLocation = redirectLocation;
        this.cookieDetails =
            [new CookieDetails(cookiePath, cookieName, cookieValue, cookieExpiresFrom, cookieAction)];
        this.redirectShouldPreserveMethod = preserveMethod;
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
        return new(statusCode);
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
        return new(location, preserveMethod: false);
    }

    /// <summary>
    /// Creates a <see cref="NonForwardedResponseDetails"/> indicating that a redirect response should be
    /// produced with a status code suitable for scenarios where we are redirecting the
    /// user to a login UI (302), and that this response should also set a cookie.
    /// </summary>
    /// <param name="location">The redirection location.</param>
    /// <param name="cookiePath">The path for the cookie.</param>
    /// <param name="cookieName">The cookie name.</param>
    /// <param name="cookieValue">The cookie value.</param>
    /// <param name="cookieExpiresFrom">The time after which the cookie expires.</param>
    /// <returns>A <see cref="NonForwardedResponseDetails"/>.</returns>
    public static NonForwardedResponseDetails ForAuthenticationRedirectSettingCookie(
        string location,
        string cookiePath,
        string cookieName,
        string cookieValue,
        DateTimeOffset cookieExpiresFrom)
    {
        return new(location, cookiePath, cookieName, cookieValue, cookieExpiresFrom, cookieAction: CookieAction.Add, preserveMethod: false);
    }

    /// <summary>
    /// Creates a <see cref="NonForwardedResponseDetails"/> indicating that a redirect response should be
    /// produced with a status code suitable for scenarios where we are redirecting the
    /// user to a login UI (302), and that this response should also set a cookie.
    /// </summary>
    /// <param name="location">The redirection location.</param>
    /// <param name="cookiePath">The path for the cookie.</param>
    /// <param name="cookieName">The cookie name.</param>
    /// <returns>A <see cref="NonForwardedResponseDetails"/>.</returns>
    public static NonForwardedResponseDetails ForAuthenticationRedirectRemovingCookie(
        string location,
        string cookiePath,
        string cookieName)
    {
        return new(location, cookiePath, cookieName, default!, DateTimeOffset.UtcNow, cookieAction: CookieAction.EnsureRemoved, preserveMethod: false);
    }

    /// <summary>
    /// Gets the redirect location if there is one.
    /// </summary>
    /// <param name="result">
    /// Set to the redirect details if this value represents a redirect.
    /// </param>
    /// <returns><see langword="true"/> if this was a redirect.</returns>
    public bool TryGetRedirect(out (string Location, bool Permanent, bool PreserveMethod, ImmutableArray<CookieDetails>? CookieDetails) result)
    {
        if (this.redirectLocation is string location)
        {
            result = (location, false, this.redirectShouldPreserveMethod, this.cookieDetails);
            return true;
        }

        result = default;
        return false;
    }
}