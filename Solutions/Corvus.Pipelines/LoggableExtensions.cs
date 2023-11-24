// <copyright file="LoggableExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Corvus.Pipelines;

/// <summary>
/// Extensions for <see cref="ILoggable{TSelf}"/>.
/// </summary>
public static class LoggableExtensions
{
    /// <summary>
    /// Log an event for the state instance, at the configured log level.
    /// </summary>
    /// <typeparam name="TState">The type of the state for which to log an event.</typeparam>
    /// <param name="state">The instance of the state for which to log an event.</param>
    /// <param name="level">The level at which to log the event.</param>
    /// <param name="eventId">The ID of the event to log.</param>
    /// <param name="message">The message format for the log event.</param>
    /// <param name="args">The arguments to format with the message.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Log<TState>(this TState state, LogLevel level, EventId eventId, string message, params object?[] args)
        where TState : struct, ILoggable<TState>
    {
#pragma warning disable CA2254 // Template should be a static expression
        if (state.Logger.IsEnabled(level))
        {
            switch (level)
            {
                case LogLevel.Debug:
                    state.Logger.LogDebug(eventId, message, args);
                    break;
                case LogLevel.Trace:
                    state.Logger.LogTrace(eventId, message, args);
                    break;
                case LogLevel.Information:
                    state.Logger.LogInformation(eventId, message, args);
                    break;
                case LogLevel.Warning:
                    state.Logger.LogWarning(eventId, message, args);
                    break;
                case LogLevel.Error:
                    state.Logger.LogError(eventId, message, args);
                    break;
                case LogLevel.Critical:
                    state.Logger.LogCritical(eventId, message, args);
                    break;
            }
        }
#pragma warning restore CA2254 // Template should be a static expression
    }
}