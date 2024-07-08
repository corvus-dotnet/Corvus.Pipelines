// <copyright file="QueryStringExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.WebUtilities;

namespace Corvus.YarpPipelines;

/// <summary>
/// Extension methods for manipulating query strings.
/// </summary>
public static class QueryStringExtensions
{
    /// <summary>
    /// Returns a query string with all instances of the specified parameter removed.
    /// </summary>
    /// <param name="queryStringIn">The existing query string.</param>
    /// <param name="parameterName">The name of the parameter to remove.</param>
    /// <returns>
    /// A query string that will not contain any parameters with the specified name,
    /// but will contain all other parameters if there were any.
    /// </returns>
    public static ReadOnlyMemory<char> Without(
        this ReadOnlyMemory<char> queryStringIn,
        ReadOnlySpan<char> parameterName)
        => Without(queryStringIn, parameterName, StringComparison.Ordinal);

    /// <summary>
    /// Returns a query string with all instances of the specified parameter removed.
    /// </summary>
    /// <param name="queryString">The existing query string.</param>
    /// <param name="parameterName">The name of the parameter to remove.</param>
    /// <param name="parameterNameComparison">The string comparison type to use.</param>
    /// <returns>
    /// A query string that will not contain any parameters with the specified name,
    /// but will contain all other parameters if there were any.
    /// </returns>
    public static ReadOnlyMemory<char> Without(
        this ReadOnlyMemory<char> queryString,
        ReadOnlySpan<char> parameterName,
        StringComparison parameterNameComparison)
    {
        ReadOnlySpan<char> qss = queryString.Span;
        int currentNameIndex = 1;
        Memory<char> mutableBuffer = default;

        while (currentNameIndex < qss.Length)
        {
            int valueEnd = qss[currentNameIndex..].IndexOf('&');
            if (valueEnd == -1)
            {
                valueEnd = qss.Length;
            }
            else
            {
                valueEnd += currentNameIndex;
            }

            int nameEnd = qss[currentNameIndex..valueEnd].IndexOf('=');
            if (nameEnd == -1)
            {
                // The query string is malformed, because there was no '=' after the name.
                nameEnd = valueEnd;
            }
            else
            {
                nameEnd += currentNameIndex;
            }

            if (qss[currentNameIndex..nameEnd].Equals(parameterName, parameterNameComparison))
            {
                // We found a match, so we need to remove this name/value pair
                if (valueEnd == qss.Length)
                {
                    // We're removing the last name/value pair, so we can just truncate the string,
                    // removing the preceding ampersand or question mark.
                    return queryString[..(currentNameIndex - 1)];
                }
                else
                {
                    // We're removing a name/value pair in the middle, so we need to copy the
                    // rest of the string down over the name/value pair we're removing
                    // We need to know what's after name/value pair being removed.
                    int quantityBeingOmitted = valueEnd - currentNameIndex + 1;
                    if (mutableBuffer.IsEmpty)
                    {
                        // We definitely need to allocate because we now need to make
                        // a copy that includes the start and end of the input, missing
                        // a chunk from the middle.
                        mutableBuffer = new char[qss.Length - quantityBeingOmitted];

                        // Copy in everything that came before the part being removed
                        ReadOnlySpan<char> start = qss[..currentNameIndex];
                        start.CopyTo(mutableBuffer.Span);

                        queryString = mutableBuffer;
                    }
                    else
                    {
                        queryString = mutableBuffer[..(mutableBuffer.Length - quantityBeingOmitted)];
                    }

                    ReadOnlySpan<char> everythingElseExceptAmpersand = qss[(valueEnd + 1)..];
                    everythingElseExceptAmpersand.CopyTo(mutableBuffer.Span[currentNameIndex..]);
                    qss = queryString.Span;
                }
            }
            else
            {
                currentNameIndex = valueEnd + 1;
            }
        }

        return queryString;
    }

    /// <summary>
    /// Determines whether the query string contains the specified parameter, and if so, returns its value.
    /// </summary>
    /// <param name="queryString">The query string to inspect.</param>
    /// <param name="parameterName">The name of the parameter to look for.</param>
    /// <param name="result">The parameter's value, or null if not found.</param>
    /// <returns>True if the parameter was found, false if not.</returns>
    public static bool TryGetSingleValue(this ReadOnlyMemory<char> queryString, string parameterName, out ReadOnlyMemory<char> result)
        => queryString.TryGetSingleValue(parameterName, out result, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether the query string contains the specified parameter, and if so, returns its value.
    /// </summary>
    /// <param name="queryString">The query string to inspect.</param>
    /// <param name="parameterName">The name of the parameter to look for.</param>
    /// <param name="result">The parameter's value, or null if not found.</param>
    /// <param name="parameterNameComparison">The string comparison type to use.</param>
    /// <returns>True if the parameter was found, false if not.</returns>
    public static bool TryGetSingleValue(this ReadOnlyMemory<char> queryString, string parameterName, out ReadOnlyMemory<char> result, StringComparison parameterNameComparison)
    {
        // TODO[optimization]: if key doesn't need to be encoded, we could compare against the encoded name
        foreach (QueryStringEnumerable.EncodedNameValuePair x in new QueryStringEnumerable(queryString))
        {
            if (x.DecodeName().Span.Equals(parameterName, parameterNameComparison))
            {
                result = x.DecodeValue();
                return true;
            }
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Determines whether the query string contains the specified parameter.
    /// </summary>
    /// <param name="queryString">The query string to inspect.</param>
    /// <param name="parameterName">The name of the parameter to look for.</param>
    /// <returns>True if the parameter was found, false if not.</returns>
    public static bool HasKey(this ReadOnlyMemory<char> queryString, string parameterName)
        => queryString.HasKey(parameterName, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether the query string contains the specified parameter.
    /// </summary>
    /// <param name="queryString">The query string to inspect.</param>
    /// <param name="parameterName">The name of the parameter to look for.</param>
    /// <param name="keyComparison">The string comparison type to use.</param>
    /// <returns>True if the parameter was found, false if not.</returns>
    public static bool HasKey(this ReadOnlyMemory<char> queryString, string parameterName, StringComparison keyComparison)
    {
        foreach (QueryStringEnumerable.EncodedNameValuePair x in new QueryStringEnumerable(queryString))
        {
            if (x.DecodeName().Span.Equals(parameterName, keyComparison))
            {
                return true;
            }
        }

        return false;
    }
}