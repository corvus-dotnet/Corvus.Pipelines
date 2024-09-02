// <copyright file="ValueStringBuilder.Replace.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>
// <license>
// Derived from code Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See https://github.com/dotnet/runtime/blob/c1049390d5b33483203f058b0e1457d2a1f62bf4/src/libraries/Common/src/System/Text/ValueStringBuilder.cs
// </license>

#pragma warning disable // Currently this is a straight copy of the original code, so we disable warnings to avoid diagnostic problems.

namespace Corvus.YarpPipelines.Internal
{
    public ref partial struct ValueStringBuilder
    {
        public void Replace(
            ReadOnlySpan<char> oldValue,
            ReadOnlySpan<char> newValue,
            int startIndex,
            int count)
        {
            if (_chars[startIndex..(startIndex + count)].IndexOf(oldValue) != -1)
            {
                // If it turns out that our ValueStringBuilderStringableClaimSetFixedList
                // benchmark shows that ValueStringBuilder makes a useful difference, then we
                // should provide a working implementation. (The benchmark should never hit
                // the code path where replacement is required.)
                throw new NotSupportedException("We haven't implemented the case where the replace is actually necessary yet");
            }
        }
    }
}
