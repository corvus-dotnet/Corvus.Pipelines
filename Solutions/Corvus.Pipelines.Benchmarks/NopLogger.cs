// <copyright file="NopLogger.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;

namespace Corvus.Pipelines.Benchmarks;

/// <summary>
/// This is a logger that actually does nothing, but is enabled for all
/// log levels, as distinct from the NullLogger which is never enabled.
/// </summary>
public class NopLogger : ILogger, IDisposable
{
    /// <summary>
    /// Gets the static instance of the NOP logger.
    /// </summary>
    public static NopLogger Instance { get; } = new();

    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return null;
    }

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    /// <inheritdoc/>
    public void Dispose()
    {
    }

#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
    }
}