// <copyright file="TestLogger.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Corvus.Pipelines.Specs.Models;

public class TestLogger : ILogger, IDisposable
{
    public ConcurrentQueue<LogEntry> Entries { get; } = new();

    private ConcurrentStack<string> Scopes { get; } = new();

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        this.Scopes.Push(state.ToString() ?? string.Empty);
        return this;
    }

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    public void Dispose()
    {
        this.Scopes.TryPop(out _);
    }

#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize

    public void Validate(params (LogLevel LogLevel, string Message, string Scope)[] expectedEntries)
    {
        var loggedItems = this.Entries.ToList();

        Assert.AreEqual(expectedEntries.Length, loggedItems.Count);

        for (int i = 0; i < loggedItems.Count; ++i)
        {
            Assert.AreEqual(expectedEntries[i].LogLevel, loggedItems[i].LogLevel);
            Assert.AreEqual(expectedEntries[i].Message, loggedItems[i].Message);
            Assert.AreEqual(expectedEntries[i].Scope, loggedItems[i].Scope);
        }
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        string scope = string.Empty;
        if (this.Scopes.TryPeek(out string? currentScope))
        {
            scope = currentScope;
        }

        this.Entries.Enqueue(new(logLevel, eventId, exception, formatter(state, exception), scope));
    }

    public readonly record struct LogEntry(LogLevel LogLevel, EventId EventId, Exception? Exception, string? Message, string? Scope);
}