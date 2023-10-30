// <copyright file="ExampleYarpPipelineWithLogging.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Corvus.Pipelines;
using Corvus.Pipelines.Handlers;
using Corvus.YarpPipelines;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace PipelineExamples;

/// <summary>
/// A sample YARP pipeline illustrating a smorgasbord of capabilities.
/// </summary>
public static class ExampleYarpPipelineWithLogging
{
    private static readonly LogLevel LogLevel = LogLevel.Information;

    private static readonly PipelineStep<YarpPipelineState> InnerPipelineInstance =
    YarpPipeline.BuildWithStepLogging(
        "InnerPipeline",
        LogLevel,
        YarpPipeline.Step("HandleFizz", state => state.RequestTransformContext.Path == "/fizz"
                    ? state.TerminateAndForward()
                    : state.Continue()),
        YarpPipeline.Step("HandleBuzz", state => state.RequestTransformContext.Path == "/buzz"
                    ? throw new InvalidOperationException("Something's gone wrong!")
                    : state.Continue()));

    private static readonly PipelineStep<HandlerState<PathString, string?>> MessageHandlerPipelineInstance =
        HandlerPipeline.BuildWithStepLogging(
            "MessageHandlerPipeline",
            LogLevel,
            HandlerPipeline.Step<PathString, string?>("HandleFoo", static state => state.Input == "/foo"
                    ? state.Handled("We're looking at a foo")
                    : state.NotHandled()),
            HandlerPipeline.Step<PathString, string?>("HandleBar", static state => state.Input == "/bar"
                    ? state.Handled(null)
                    : state.NotHandled()));

    private static readonly PipelineStep<YarpPipelineState> AddMessageToHttpContext =
        MessageHandlerPipelineInstance
            .Bind(
                wrap: static (YarpPipelineState state) => HandlerState<PathString, string?>.For(state.RequestTransformContext.Path, state.Logger),
                unwrap: static (state, innerState) =>
                {
                    if (innerState.WasHandled(out string? message))
                    {
                        if (message is string msg)
                        {
                            state.RequestTransformContext.HttpContext.Items["Message"] = msg;
                            return state.Continue();
                        }
                        else
                        {
                            return state.TerminateWith(new(400));
                        }
                    }

                    return state.Continue();
                });

    private static readonly Func<YarpPipelineState, PipelineStep<YarpPipelineState>> ChooseMessageContextHandler =
            static state => state.RequestTransformContext.Query.QueryString.HasValue
                                ? state => ValueTask.FromResult(state.TerminateWith(new(400)))
                                : AddMessageToHttpContext;

    private static readonly Func<YarpPipelineState, Exception, YarpPipelineState> CatchPipelineException =
        static (state, exception) =>
        {
            if (state.Logger.IsEnabled(LogLevel.Warning))
            {
                // If we are logging warnings, we want to tell people about this.
                if (state.Logger.IsEnabled(LogLevel.Debug))
                {
                    // If we have debug level diagnostics, include the exception
                    state.Logger.LogWarning(Pipeline.EventIds.TransientFailure, exception, "Exception during processing produced transient failure.");
                }
                else
                {
                    // Otherwise include just a plain text warning.
                    state.Logger.LogWarning(Pipeline.EventIds.TransientFailure, "Exception during processing produced transient failure.");
                }
            }

            return state.TransientFailure(new("Unable to execute the pipeline.", exception));
        };

    /// <summary>
    /// Gets an instance of an example yarp pipeline handler.
    /// </summary>
    public static PipelineStep<YarpPipelineState> Instance { get; } =
        YarpPipeline.BuildWithStepLogging(
            "MainPipeline",
            LogLevel,
            YarpPipeline.Step("HandleRoot", static state => state.RequestTransformContext.Path == "/" // You can write in this style where we execute steps directly
                ? ValueTask.FromResult(state.TerminateAndForward())
                : InnerPipelineInstance(state)),
            YarpPipeline.Step("ChooseMessageContextHandler", YarpPipeline.Current.Choose(ChooseMessageContextHandler)), // But we prefer this style where we hide away the state
            YarpPipeline.Step("HandleMessageContextResult", static state => ValueTask.FromResult(state.RequestTransformContext.HttpContext.Items["Message"] is string message
                        ? state.Continue()
                        : state.TerminateWith(new(404)))))
        .Catch(CatchPipelineException)
        .Retry(
            static state => state.ExecutionStatus == PipelineStepStatus.TransientFailure && state.FailureCount < 5, // This is doing a simple count, but you could layer policy based on state.TryGetErrorDetails()
            async state =>
            {
                if (state.Logger.IsEnabled(LogLevel.Information))
                {
                    state.Logger.LogInformation(Pipeline.EventIds.Retrying, message: "Retrying: {failureCount}", state.FailureCount);
                }

                await Task.Delay(0).ConfigureAwait(false); // You could do a back off using state.FailureCount, or whatever!
                return state;
            })
        .OnError(state => state.TerminateWith(new(500)));

    /// <summary>
    /// Gets an instance of an example yarp pipeline handler.
    /// </summary>
    public static PipelineStep<YarpPipelineState> ForceAsyncInstance { get; } =
        YarpPipeline.BuildWithStepLogging(
            "MainAsyncPipeline",
            LogLevel,
            YarpPipeline.Step("HandleRoot", static state => state.RequestTransformContext.Path == "/" // You can write in this style where we execute steps directly
                ? ValueTask.FromResult(state.TerminateAndForward())
                : InnerPipelineInstance(state)),
            YarpPipeline.Step("AsyncDelay", async state =>
            {
                await Task.Delay(0).ConfigureAwait(false);
                return state.Continue();
            }),
            YarpPipeline.Step("ChooseMessageContextHandler", YarpPipeline.Current.Choose(ChooseMessageContextHandler)), // But we prefer this style where we hide away the state
            YarpPipeline.Step("HandleMessageContextResult", static state => ValueTask.FromResult(state.RequestTransformContext.HttpContext.Items["Message"] is string message
                        ? state.Continue()
                        : state.TerminateWith(new(404)))))
        .Catch(CatchPipelineException)
        .Retry(
            static state => state.FailureCount < 5, // This is doing a simple count, but you could layer policy based on state.TryGetErrorDetails()
            async state =>
            {
                if (state.Logger.IsEnabled(LogLevel.Information))
                {
                    state.Logger.LogInformation(Pipeline.EventIds.Retrying, message: "Retrying: {failureCount}", state.FailureCount);
                }

                await Task.Delay(0).ConfigureAwait(false); // You could do a back off using state.FailureCount, or whatever!
                return state;
            })
        .OnError(state => state.TerminateWith(new(500)));
}