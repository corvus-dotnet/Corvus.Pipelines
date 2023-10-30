// <copyright file="ILoggable.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;

namespace Corvus.Pipelines;

/// <summary>
/// State that supports logging.
/// </summary>
public interface ILoggable
{
    /// <summary>
    /// Gets the logger for the state.
    /// </summary>
    ILogger Logger { get; }
}