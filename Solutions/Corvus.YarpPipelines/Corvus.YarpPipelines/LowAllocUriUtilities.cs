// <copyright file="LowAllocUriUtilities.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>
// <license>
// Derived from code Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See:
// https://github.com/dotnet/runtime/blob/10ea547d62e30a73e50050d95d44b412ad9a6a7d/src/libraries/System.Private.Uri/src/System/UriHelper.cs#L162
// https://github.com/dotnet/runtime/blob/10ea547d62e30a73e50050d95d44b412ad9a6a7d/src/libraries/Common/src/System/HexConverter.cs#L82
// </license>

using System.Buffers;
using System.Diagnostics;
using System.Text;

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
    /// <param name="path">
    /// The path to append, which is unencoded. If this includes any % symbols, they will be
    /// encoded. For example, <c>"ab%20c"</c> will <em>not</em> be turned into <c>"ab c"</c>
    /// but will instead become <c>"ab%2520c"</c>.
    /// </param>
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

        var builder = new ValueStringBuilder(prefix.Length + path.Length + query.Length);
        builder.Append(prefix);
        int indexOfCharToEncode = path.IndexOfAnyExcept(ValidPathChars);
        bool pathRequiresEncoding = indexOfCharToEncode != -1;
        if (pathRequiresEncoding)
        {
            builder.Append(path[..indexOfCharToEncode]);
            EscapeStringToBuilder(path[indexOfCharToEncode..], ref builder, ValidPathChars, false);
        }
        else
        {
            builder.Append(path);
        }

        builder.Append(query);
        return builder.ToString();
    }

    // Copied from .NET runtime libraries because they don't make their ValueStringBuilder
    // public.
    private static void EscapeStringToBuilder(
        ReadOnlySpan<char> stringToEscape,
        ref ValueStringBuilder vsb,
        SearchValues<char> noEscape,
        bool checkExistingEscaped)
    {
        Debug.Assert(!stringToEscape.IsEmpty && !noEscape.Contains(stringToEscape[0]), "EscapeStringToBuilder requires non-empty input");

        // Allocate enough stack space to hold any Rune's UTF8 encoding.
        Span<byte> utf8Bytes = stackalloc byte[4];

        while (!stringToEscape.IsEmpty)
        {
            char c = stringToEscape[0];

            if (!char.IsAscii(c))
            {
                if (Rune.DecodeFromUtf16(stringToEscape, out Rune r, out int charsConsumed) != OperationStatus.Done)
                {
                    r = Rune.ReplacementChar;
                }

                Debug.Assert(stringToEscape.EnumerateRunes() is { } e && e.MoveNext() && e.Current == r, "Rune enumeration should have produced the same result as DecodeFromUtf16");
                Debug.Assert(charsConsumed is 1 or 2, "DecodeFromUtf16 expected to consume at most 2 characters");

                stringToEscape = stringToEscape[charsConsumed..];

                // The rune is non-ASCII, so encode it as UTF8, and escape each UTF8 byte.
                r.TryEncodeToUtf8(utf8Bytes, out int bytesWritten);
                foreach (byte b in utf8Bytes[..bytesWritten])
                {
                    PercentEncodeByte(b, ref vsb);
                }

                continue;
            }

            if (!noEscape.Contains(c))
            {
                // If we're checking for existing escape sequences, then if this is the beginning of
                // one, check the next two characters in the sequence.
                if (c == '%' && checkExistingEscaped)
                {
                    // If the next two characters are valid escaped ASCII, then just output them as-is.
                    if (stringToEscape.Length > 2 && char.IsAsciiHexDigit(stringToEscape[1]) && char.IsAsciiHexDigit(stringToEscape[2]))
                    {
                        vsb.Append('%');
                        vsb.Append(stringToEscape[1]);
                        vsb.Append(stringToEscape[2]);
                        stringToEscape = stringToEscape[3..];
                        continue;
                    }
                }

                PercentEncodeByte((byte)c, ref vsb);
                stringToEscape = stringToEscape[1..];
                continue;
            }

            // We have a character we don't want to escape. It's likely there are more, do a vectorized search.
            int charsToCopy = stringToEscape.IndexOfAnyExcept(noEscape);
            if (charsToCopy < 0)
            {
                charsToCopy = stringToEscape.Length;
            }

            Debug.Assert(charsToCopy > 0, "Should either have found next escape char or end of text");

            vsb.Append(stringToEscape[..charsToCopy]);
            stringToEscape = stringToEscape[charsToCopy..];
        }
    }

    private static void PercentEncodeByte(byte b, ref ValueStringBuilder to)
    {
        to.Append('%');
        ToCharsBuffer(b, to.AppendSpan(2), 0);
    }

    private static void ToCharsBuffer(byte value, Span<char> buffer, int startingIndex = 0)
    {
        uint difference = ((value & 0xF0U) << 4) + (value & 0x0FU) - 0x8989U;
        uint packedResult = (((uint)(-(int)difference) & 0x7070U) >> 4) + difference + 0xB9B9U;

        buffer[startingIndex + 1] = (char)(packedResult & 0xFF);
        buffer[startingIndex] = (char)(packedResult >> 8);
    }
}