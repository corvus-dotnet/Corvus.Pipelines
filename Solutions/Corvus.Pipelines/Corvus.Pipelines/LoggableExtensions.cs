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
    /// Log entry and exit to the step.
    /// </summary>
    /// <typeparam name="TState">The type of the step to log.</typeparam>
    /// <param name="step">The step to log.</param>
    /// <param name="logLevel">The log level at which to write the log entry. (Defaults to <see cref="LogLevel.Debug"/>.</param>
    /// <param name="name">The name of the step. (Defaults to the name of the caller argument expression.)</param>
    /// <returns>The step wrapped with logging.</returns>
    public static PipelineStep<TState> Log<TState>(this PipelineStep<TState> step, LogLevel logLevel = LogLevel.Debug, [CallerArgumentExpression(nameof(step))] string? name = null)
        where TState : struct, ILoggable<TState>
    {
        return async state =>
        {
            if (state.Logger.IsEnabled(logLevel))
            {
                using IDisposable? scope = name is string scopeName ? state.Logger.BeginScope(scopeName) : null;
                LogEntry(logLevel, state);
                TState result = await step(state).ConfigureAwait(false);
                LogExit(logLevel, result);
                return result;
            }
            else
            {
                return await step(state).ConfigureAwait(false);
            }
        };
    }

    /// <summary>
    /// Log entry and exit to the step.
    /// </summary>
    /// <typeparam name="TState">The type of the step to log.</typeparam>
    /// <param name="step">The step to log.</param>
    /// <param name="logLevel">The log level at which to write the log entry. (Defaults to <see cref="LogLevel.Debug"/>.</param>
    /// <param name="name">The name of the step. (Defaults to the name of the caller argument expression.)</param>
    /// <returns>The step wrapped with logging.</returns>
    public static SyncPipelineStep<TState> Log<TState>(this SyncPipelineStep<TState> step, LogLevel logLevel = LogLevel.Debug, [CallerArgumentExpression(nameof(step))] string? name = null)
        where TState : struct, ILoggable<TState>
    {
        return state =>
        {
            if (state.Logger.IsEnabled(logLevel))
            {
                using IDisposable? scope = name is string scopeName ? state.Logger.BeginScope(scopeName) : null;
                LogEntry(logLevel, state);
                TState result = step(state);
                LogExit(logLevel, state);
                return result;
            }
            else
            {
                return step(state);
            }
        };
    }

    /// <summary>
    /// Log an event for the state instance, at the configured log level.
    /// </summary>
    /// <typeparam name="TState">The type of the state for which to log an event.</typeparam>
    /// <param name="state">The instance of the state for which to log an event.</param>
    /// <param name="level">The level at which to log the event.</param>
    /// <param name="eventId">The ID of the event to log.</param>
    /// <param name="message">The message format for the log event.</param>
    /// <remarks>
    /// We discourage the use of event args to format the message as it causes allocations.
    /// Using the scope obviates this problem.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Log<TState>(this TState state, LogLevel level, in EventId eventId, string message)
        where TState : struct, ILoggable<TState>
    {
#pragma warning disable CA2254 // Template should be a static expression
        switch (level)
        {
            case LogLevel.Debug:
                state.Logger.LogDebug(eventId, message);
                break;
            case LogLevel.Trace:
                state.Logger.LogTrace(eventId, message);
                break;
            case LogLevel.Information:
                state.Logger.LogInformation(eventId, message);
                break;
            case LogLevel.Warning:
                state.Logger.LogWarning(eventId, message);
                break;
            case LogLevel.Error:
                state.Logger.LogError(eventId, message);
                break;
            case LogLevel.Critical:
                state.Logger.LogCritical(eventId, message);
                break;
        }
#pragma warning restore CA2254 // Template should be a static expression
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void LogEntry<TState>(LogLevel level, in TState state)
    where TState : struct, ILoggable<TState>
    {
        state.Logger.Log(level, Pipeline.EventIds.EnteredStep, "entered");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void LogExit<TState>(LogLevel level, in TState state)
        where TState : struct, ILoggable<TState>
    {
        state.Log(level, Pipeline.EventIds.ExitedStep, "exited");
    }
}