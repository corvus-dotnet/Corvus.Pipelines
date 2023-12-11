﻿// <copyright file="YarpRequestPipelineStateExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;

namespace Corvus.YarpPipelines;

/// <summary>
/// Extension methods for <see cref="YarpRequestPipelineState"/>.
/// </summary>
public static class YarpRequestPipelineStateExtensions
{
    /// <summary>
    /// Returns a <see cref="YarpRequestPipelineState"/> instance that will continue processing the pipeline,
    /// ensuring that the proxied request will not include the specified header unless a downstream
    /// step adds it back in.
    /// </summary>
    /// <param name="state">The YARP pipeline state.</param>
    /// <param name="headerName">The header to remove if present.</param>
    /// <returns>The non-terminating <see cref="YarpRequestPipelineState"/>.</returns>
    public static YarpRequestPipelineState EnsureHeaderNotPresentAndContinue(
        this YarpRequestPipelineState state,
        string headerName)
    {
        state.RequestTransformContext.ProxyRequest.Headers.Remove(headerName);
        return state.Continue();
    }

    /// <summary>
    /// Returns a <see cref="YarpRequestPipelineState"/> instance that will continue processing the pipeline,
    /// ensuring that the proxied request includes the specified header. The header must not already
    /// be present.
    /// </summary>
    /// <param name="state">The YARP pipeline state.</param>
    /// <param name="headerName">The header to add.</param>
    /// <param name="value">The value for the header.</param>
    /// <returns>The non-terminating <see cref="YarpRequestPipelineState"/>.</returns>
    /// <exception cref="ArgumentException">Thrown if the header is already present.</exception>
    public static YarpRequestPipelineState AddHeaderAndContinue(
        this YarpRequestPipelineState state,
        string headerName,
        string value)
    {
        if (!state.RequestTransformContext.ProxyRequest.Headers.TryAddWithoutValidation(headerName, value))
        {
            throw new ArgumentException($"Unable to add header '{headerName}' to proxy request");
        }

        return state.Continue();
    }

    /// <summary>
    /// Performs an ASP.NET Core sign in for interactive login (using
    /// <see cref="CookieAuthenticationDefaults.AuthenticationScheme"/>) and returns
    /// a state directing the pipeline executor to redirect to the original URL the
    /// user was trying to access when they were redirected to log in.
    /// </summary>
    /// <param name="state">The YARP pipeline state.</param>
    /// <param name="identity">The user details with which to complete the login.</param>
    /// <param name="authenticationProperties">
    /// Authentication properties to associate with the login.
    /// </param>
    /// <param name="returnUrl">The URL to which to redirect the user.</param>
    /// <param name="cookiePath">
    /// The cookie path, typically limited to work only for the login call-back URL.
    /// </param>
    /// <param name="cookieName">
    /// The name of the login cookie to remove. This cookie is used to validate the login.
    /// It incorporates the nonce.
    /// </param>
    /// <returns>
    /// A task producing the pipeline state to terminate the pipeline with.
    /// </returns>
    public static async ValueTask<YarpRequestPipelineState> CompleteInteractiveSignInAndTerminateAsync(
        this YarpRequestPipelineState state,
        ClaimsIdentity identity,
        AuthenticationProperties authenticationProperties,
        string returnUrl,
        string cookiePath,
        string cookieName)
    {
        await state.RequestTransformContext.HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            authenticationProperties);

        return state.TerminateWith(NonForwardedResponseDetails.ForAuthenticationRedirectRemovingCookie(
            returnUrl,
            cookiePath,
            cookieName));
    }

    /// <summary>
    /// Sets the <see cref="HttpContext.User"/> of the request associated with this pipeline state.
    /// </summary>
    /// <param name="state">The pipeline state.</param>
    /// <param name="user">The <see cref="ClaimsPrincipal"/> to set as the request's user.</param>
    public static void SetUser(this YarpRequestPipelineState state, ClaimsPrincipal user)
    {
        state.RequestTransformContext.HttpContext.User = user;
    }

    /// <summary>
    /// Determines whether the request has an authenticated user associated with it, and if so, returns it.
    /// </summary>
    /// <param name="state">The pipeline state.</param>
    /// <param name="user">Receives the user.</param>
    /// <returns><see langword="true"/> if a user was available.</returns>
    public static bool TryGetAuthenticatedUser(this YarpRequestPipelineState state, [NotNullWhen(true)] out ClaimsPrincipal? user)
    {
        user = state.RequestTransformContext.HttpContext.User;
        return user is not null && user.Identity?.IsAuthenticated == true;
    }

    /// <summary>
    /// Finds a cookie from the incoming request that matches a predicate.
    /// </summary>
    /// <param name="state">The YARP pipeline state.</param>
    /// <param name="predicate">Determines the criteria.</param>
    /// <param name="cookie">The matching cookie, if found.</param>
    /// <returns><see langword="true"/> if a match was found.</returns>
    public static bool TryFindCookie(
        this YarpRequestPipelineState state,
        Func<KeyValuePair<string, string>, bool> predicate,
        out KeyValuePair<string, string> cookie)
    {
        cookie = state.RequestTransformContext.HttpContext.Request.Cookies.SingleOrDefault(predicate);
        return cookie.Key is not null;
    }

    /// <summary>
    /// Retrieves the body of the request as an <see cref="IFormCollection"/>.
    /// </summary>
    /// <param name="state">The YARP pipeline state.</param>
    /// <returns>A task producing an <see cref="IFormCollection"/>.</returns>
    public static Task<IFormCollection> ReadFormAsync(this YarpRequestPipelineState state)
        => state.RequestTransformContext.HttpContext.Request.ReadFormAsync();

    /// <summary>
    /// Gets the raw text value of a bearer token in the Authorization header of the request
    /// associated with a <see cref="YarpRequestPipelineState"/>, if such a header is present.
    /// </summary>
    /// <typeparam name="TTokenReceiverState">
    /// The type of value to be passed through to <paramref name="rawTokenReceiver"/>.
    /// </typeparam>
    /// <param name="state">The state representing the request to be inspected.</param>
    /// <param name="tokenReceiverState">A value passed through to the <paramref name="rawTokenReceiver"/>.</param>
    /// <param name="rawTokenReceiver">Call-back that will be invoked with the raw token, if present.</param>
    /// <returns><see langword="true"/> if a bearer token was present.</returns>
    public static bool TryGetRawBearerToken<TTokenReceiverState>(
        this YarpRequestPipelineState state,
        in TTokenReceiverState tokenReceiverState,
        ReadOnlySpanAction<char, TTokenReceiverState> rawTokenReceiver)
    {
        if (TryGetRawBearerToken(state, out ReadOnlyMemory<char> rawToken))
        {
            rawTokenReceiver(rawToken.Span, tokenReceiverState);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the raw text value of a bearer token in the Authorization header of the request
    /// associated with a <see cref="YarpRequestPipelineState"/>, if such a header is present.
    /// </summary>
    /// <param name="state">The state representing the request to be inspected.</param>
    /// <param name="rawToken">The raw token, if present.</param>
    /// <returns><see langword="true"/> if a bearer token was present.</returns>
    public static bool TryGetRawBearerToken(
        this YarpRequestPipelineState state,
        out ReadOnlyMemory<char> rawToken)
    {
        if (state.Headers.Authorization is StringValues authHeader
            && authHeader.Count == 1)
        {
            // This code looks for a Bearer token, which is something we're only going
            // to see on API requests - browser don't send Bearer tokens for HTML, and so
            // on.
            string authHeaderValue = authHeader[0]!;
            const string expectedPrefix = "Bearer ";
            if (authHeaderValue.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
            {
                rawToken = authHeaderValue.AsMemory()[expectedPrefix.Length..].Trim();
                return true;
            }
        }

        rawToken = default;
        return false;
    }

    /// <summary>
    /// Returns the combined components of the request URL in a fully escaped form suitable for use in HTTP headers
    /// and other HTTP operations.
    /// </summary>
    /// <param name="state">The state representing the request to be inspected.</param>
    /// <returns>The URL.</returns>
    public static string GetEncodedUrl(this YarpRequestPipelineState state) => state.RequestTransformContext.HttpContext.Request.GetEncodedUrl();

    /// <summary>
    /// Builds an absolute URL by combining the base URL of the incoming request with a relative path.
    /// </summary>
    /// <param name="state">The state representing the request to be inspected.</param>
    /// <param name="relativePath">The relative path.</param>
    /// <returns>The absolute URL.</returns>
    public static string BuildAbsoluteUrlFromRequestRelativePath(this YarpRequestPipelineState state, string relativePath) => UriHelper.BuildAbsolute(
        state.RequestTransformContext.HttpContext.Request.Scheme,
        state.RequestTransformContext.HttpContext.Request.Host,
        relativePath);
}