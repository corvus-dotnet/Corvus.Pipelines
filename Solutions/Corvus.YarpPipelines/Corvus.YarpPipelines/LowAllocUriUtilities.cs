// <copyright file="LowAllocUriUtilities.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Buffers;
using System.Diagnostics;

namespace Corvus.YarpPipelines.Internal;

/// <summary>
/// Utilities for building URIs with minimal allocations.
/// </summary>
public static class LowAllocUriUtilities
{
    private static readonly SearchValues<char> ValidPathChars =
        SearchValues.Create("!$&'()*+,-./0123456789:;=@ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz~");

    /// <summary>
    /// Appends the given path and encoded query to the encoded prefix while avoiding duplicate '/'.
    /// </summary>
    /// <param name="prefix">The encoded scheme, host, port, and optional path base for the destination server.
    /// e.g. "http://example.com:80/path/prefix".</param>
    /// <param name="path">The path to append, which may be unencoded.</param>
    /// <param name="query">The encoded query to append.</param>
    /// <returns>The Uri.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the query contains a '#' character.
    /// </exception>
    public static string EncodePathAndAppendEncodedQueryString(
        ReadOnlySpan<char> prefix, ReadOnlySpan<char> path, ReadOnlySpan<char> query)
    {
        Debug.Assert(path[0] == '/', "Path should begin with '/'");

        if (!path.IsEmpty && !prefix.IsEmpty && prefix[^1] == '/')
        {
            // When path has a value it always starts with a '/'. Avoid double slashes when concatenating.
            prefix = prefix[0..^1];
        }

        bool queryRequiresEncoding = query.Contains('#');
        if (queryRequiresEncoding)
        {
            // YARP allows this, rewriting the '#' to '%23'. We don't. You can encode your own '#' you lazy so-and-so.
            throw new NotSupportedException("Query override must not contain '#' character");
        }

        bool pathRequiresEncoding = path.IndexOfAnyExcept(ValidPathChars) != -1;
        if (pathRequiresEncoding)
        {
            // NEXT TIME:
            // Implement!
            throw new NotSupportedException("We haven't yet implemented encoding of forwarded paths that require it");
        }

        var builder = new ValueStringBuilder(prefix.Length + path.Length + query.Length);
        builder.Append(prefix);
        builder.Append(path);
        builder.Append(query);
        return builder.ToString();
    }

////    private static string EncodePath(PathString path)
////    {
////        string? value = path.Value;

////        if (string.IsNullOrEmpty(value))
////        {
////            return string.Empty;
////        }

////        // Check if any escaping is required.
////#if NET8_0_OR_GREATER
////        int indexOfInvalidChar = value.AsSpan().IndexOfAnyExcept(s_validPathChars);
////#else
////        var indexOfInvalidChar = -1;

////        for (var i = 0; i < value.Length; i++)
////        {
////            if (!IsValidPathChar(value[i]))
////            {
////                indexOfInvalidChar = i;
////                break;
////            }
////        }
////#endif

////        return indexOfInvalidChar < 0
////            ? value
////            : EncodePath(value, indexOfInvalidChar);
////    }

////    private static string EncodePath(string value, int i)
////    {
////        var builder = new ValueStringBuilder(stackalloc char[ValueStringBuilder.StackallocThreshold]);

////        int start = 0;
////        int count = i;
////        bool requiresEscaping = false;

////        while (i < value.Length)
////        {
////            if (IsValidPathChar(value[i]))
////            {
////                if (requiresEscaping)
////                {
////                    // the current segment requires escape
////                    builder.Append(Uri.EscapeDataString(value.Substring(start, count)));

////                    requiresEscaping = false;
////                    start = i;
////                    count = 0;
////                }

////                count++;
////                i++;
////            }
////            else
////            {
////                if (!requiresEscaping)
////                {
////                    // the current segment doesn't require escape
////                    builder.Append(value.AsSpan(start, count));

////                    requiresEscaping = true;
////                    start = i;
////                    count = 0;
////                }

////                count++;
////                i++;
////            }
////        }

////        Debug.Assert(count > 0);

////        if (requiresEscaping)
////        {
////            builder.Append(Uri.EscapeDataString(value.Substring(start, count)));
////        }
////        else
////        {
////            builder.Append(value.AsSpan(start, count));
////        }

////        return builder.ToString();
////    }
}