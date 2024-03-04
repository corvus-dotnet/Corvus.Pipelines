// <copyright file="NonForwardedResponseDetails.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Collections.Immutable;

namespace Corvus.YarpPipelines;

/// <summary>
/// The information required to build a local response for a non-forwarded pipeline result.
/// </summary>
/// <remarks><see cref="YarpRequestPipelineState.TerminateWith(NonForwardedResponseDetails)"/> for more details.</remarks>
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
        string redirectLocation,
        CookieDetails cookieDetails,
        bool preserveMethod)
    {
        this.redirectLocation = redirectLocation;
        this.cookieDetails =
            [cookieDetails];
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
    /// user to a login UI (302), and that this response should also set a cookie.
    /// </summary>
    /// <param name="location">
    /// The redirection location, in a fully escaped form suitable for use in HTTP headers and
    /// other HTTP operations.
    /// </param>
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
        return new(location, new(cookiePath, cookieName, cookieValue, cookieExpiresFrom, CookieAction.Add), preserveMethod: false);
    }

    /// <summary>
    /// Creates a <see cref="NonForwardedResponseDetails"/> indicating that a redirect response should be
    /// produced with a status code suitable for scenarios where we are redirecting the
    /// user to a login UI (302), and that this response should also set a cookie.
    /// </summary>
    /// <param name="location">
    /// The redirection location, in a fully escaped form suitable for use in HTTP headers and
    /// other HTTP operations.
    /// </param>
    /// <param name="cookiePath">The path for the cookie.</param>
    /// <param name="cookieName">The cookie name.</param>
    /// <returns>A <see cref="NonForwardedResponseDetails"/>.</returns>
    public static NonForwardedResponseDetails ForAuthenticationRedirectRemovingCookie(
        string location,
        string cookiePath,
        string cookieName)
    {
        return new(location, new(cookiePath, cookieName, default!, DateTimeOffset.UtcNow, CookieAction.EnsureRemoved), preserveMethod: false);
    }

    /// <summary>
    /// Gets the redirect location if there is one.
    /// </summary>
    /// <param name="result">
    /// Set to the redirect details if this value represents a redirect.
    /// </param>
    /// <returns><see langword="true"/> if this was a redirect.</returns>
    public bool TryGetRedirect(out RedirectDetails result)
    {
        if (!string.IsNullOrEmpty(this.redirectLocation))
        {
            // This redirect location will be handed directly to ASP.NET Core's Response.Redirect,
            // and as the docs say:
            // "This must be properly encoded for use in http headers where only ASCII characters are allowed."
            result = new(this.redirectLocation, false, this.redirectShouldPreserveMethod, this.cookieDetails);
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Details returned  from <see cref="TryGetRedirect(out RedirectDetails)"/>.
    /// </summary>
    /// <param name="Location">
    /// The redirection location, in a fully escaped form suitable for use in HTTP headers and
    /// other HTTP operations.
    /// </param>
    /// <param name="Permanent">
    /// Indicates whether the HTTP response should report this as a permanent redirect.
    /// </param>
    /// <param name="PreserveMethod">
    /// If false, the redirect will be a GET. If true, the redirect will be the same method as the
    /// incoming request.
    /// </param>
    /// <param name="CookieDetails">
    /// A list of changes to make to the cookies in the response, or null if no changes are required.
    /// </param>
    public readonly record struct RedirectDetails(string Location, bool Permanent, bool PreserveMethod, ImmutableArray<CookieDetails>? CookieDetails);
}