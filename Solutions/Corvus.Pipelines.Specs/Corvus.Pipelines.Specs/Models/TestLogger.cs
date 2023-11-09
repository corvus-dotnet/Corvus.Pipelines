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

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return this;
    }

    public void Dispose()
    {
    }

    public void Validate(params (LogLevel LogLevel, string Message)[] expectedEntries)
    {
        var loggedItems = this.Entries.ToList();

        Assert.AreEqual(expectedEntries.Length, loggedItems.Count);

        for (int i = 0; i < loggedItems.Count; ++i)
        {
            Assert.AreEqual(expectedEntries[i].LogLevel, loggedItems[i].LogLevel);
            Assert.AreEqual(expectedEntries[i].Message, loggedItems[i].Message);
        }
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        this.Entries.Enqueue(new(logLevel, eventId, exception, formatter(state, exception)));
    }

    public readonly record struct LogEntry(LogLevel LogLevel, EventId EventId, Exception? Exception, string Message);
}