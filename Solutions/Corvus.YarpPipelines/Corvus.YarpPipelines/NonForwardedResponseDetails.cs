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
    private readonly string? wwwAuthenticateHeaderValue;

    private NonForwardedResponseDetails(int statusCode)
    {
        this.StatusCode = statusCode;
    }

    private NonForwardedResponseDetails(int statusCode, string wwwAuthenticateHeaderValue)
    {
        this.StatusCode = statusCode;

        // For now, we're handling the WWW-Authenticate header as a special case, because that and cookies are
        // the only scenarios in which we need to add or modify response headers. If more scenarios arise, we
        // will most likely generalise header handling.
        this.wwwAuthenticateHeaderValue = wwwAuthenticateHeaderValue;
    }

    private NonForwardedResponseDetails(
        string redirectLocation,
        ImmutableArray<CookieDetails> cookieDetails,
        bool preserveMethod)
    {
        this.redirectLocation = redirectLocation;
        this.cookieDetails = cookieDetails;
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
    /// user to a login UI (302), and that this response should also set a cookie, and may need to
    /// remove existing cookies.
    /// </summary>
    /// <param name="location">
    /// The redirection location, in a fully escaped form suitable for use in HTTP headers and
    /// other HTTP operations.
    /// </param>
    /// <param name="cookieDetails">The cookies to add and/or remove.</param>
    /// <returns>A <see cref="NonForwardedResponseDetails"/>.</returns>
    public static NonForwardedResponseDetails ForAuthenticationRedirectAdjustingCookies(
        string location,
        IEnumerable<CookieDetails> cookieDetails)
    {
        return new(
            location,
            cookieDetails.ToImmutableArray(),
            preserveMethod: false);
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
        return new(
            location,
            [new(cookiePath, cookieName, default!, DateTimeOffset.UtcNow, CookieAction.EnsureRemoved)],
            preserveMethod: false);
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
    /// <param name="cookieNames">The names of the cookies to remove.</param>
    /// <returns>A <see cref="NonForwardedResponseDetails"/>.</returns>
    public static NonForwardedResponseDetails ForAuthenticationRedirectRemovingCookies(
        string location,
        string cookiePath,
        IEnumerable<string> cookieNames)
    {
        return new(
            location,
            cookieNames
                .Select(cookieName => new CookieDetails(cookiePath, cookieName, default!, DateTimeOffset.UtcNow, CookieAction.EnsureRemoved))
                .ToImmutableArray(),
            preserveMethod: false);
    }

    /// <summary>
    /// Creates a <see cref="NonForwardedResponseDetails"/> indicating that an Unauthorized (401) response should be
    /// produced, along with a WWW-Authenticate header value.
    /// </summary>
    /// <param name="wwwAuthenticateHeaderValue">The value for the WWW-Authenticate header.</param>
    /// <returns>A <see cref="NonForwardedResponseDetails"/>.</returns>
    public static NonForwardedResponseDetails ForUnauthorized(string wwwAuthenticateHeaderValue)
    {
        return new NonForwardedResponseDetails(401, wwwAuthenticateHeaderValue);
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
    /// Gets the number of headers that should be added to the response.
    /// </summary>
    /// <returns>The number of headers.</returns>
    public int GetHeadersToAddCount()
    {
        return this.wwwAuthenticateHeaderValue is not null ? 1 : 0;
    }

    /// <summary>
    /// Reads the headers that should be added to the response into a span, if there is space to do so.
    /// </summary>
    /// <param name="headersToAdd">
    /// The span into which to write the headers. If this is too small to hold all of the headers, then
    /// no headers will be written to it, and the method will return false.
    /// </param>
    /// <param name="headerCount">
    /// The number of headers. This will be set to the total number of headers even if the span was too small.
    /// </param>
    /// <returns>
    /// True if the span was large enough to contain the headers, and was therefore populated. False if it was not.
    /// </returns>
    public bool TryGetHeadersToAdd(Span<HeaderDetails> headersToAdd, out int headerCount)
    {
        if (this.wwwAuthenticateHeaderValue is not null)
        {
            headerCount = 1;
            if (headersToAdd.Length < 1)
            {
                return false;
            }

            headersToAdd[0] = new HeaderDetails("WWW-Authenticate", this.wwwAuthenticateHeaderValue);
            return true;
        }

        headerCount = 0;
        return false;
    }

    /// <summary>
    /// Details returned from <see cref="TryGetRedirect(out RedirectDetails)"/>.
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

    /// <summary>
    /// A header returned from <see cref="TryGetHeadersToAdd(Span{HeaderDetails}, out int)"/>.
    /// </summary>
    /// <param name="Name">The header name.</param>
    /// <param name="Value">The header value.</param>
    public readonly record struct HeaderDetails(string Name, string Value);
}