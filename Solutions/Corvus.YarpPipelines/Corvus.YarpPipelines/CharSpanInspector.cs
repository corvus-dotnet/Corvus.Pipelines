// <copyright file="CharSpanInspector.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.YarpPipelines;

/// <summary>
/// A delegate for inspecting a <see cref="ReadOnlySpan{T}"/> of <see cref="char"/>.
/// </summary>
/// <typeparam name="TResult">The type the inspector returns.</typeparam>
/// <typeparam name="TState">The state the inspector uses.</typeparam>
/// <param name="span">The span to inspect.</param>
/// <param name="state">The state passed into the inspection operation.</param>
/// <returns>The value to return from the inspection operation.</returns>
/// <remarks>
/// This is used in cases where we need to be able to inspect a <see cref="ReadOnlySpan{Char}"/>
/// but can't simply expose a property of that type. For example, when we have
/// built a span on the stack, we can only pass it into things, meaning that we
/// need to use a callback-driven approach if other code is to be able to use
/// the span.
/// </remarks>
public delegate TResult CharSpanInspector<TResult, TState>(ReadOnlySpan<char> span, TState state);