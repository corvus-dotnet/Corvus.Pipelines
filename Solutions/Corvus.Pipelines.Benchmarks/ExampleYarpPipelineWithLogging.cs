﻿// <copyright file="ExampleYarpPipelineWithLogging.cs" company="Endjin Limited">
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
    private static readonly SyncPipelineStep<YarpRequestPipelineState> HandleFizz =
        static state => state.GetNominalRequestSignature().Path == "/fizz"
                    ? state.TerminateWith(new ForwardedRequestDetails() { ClusterId = "Nonsense" })
                    : state.Continue();

    private static readonly SyncPipelineStep<YarpRequestPipelineState> HandleBuzz =
        static state => state.GetNominalRequestSignature().Path == "/buzz"
                    ? throw new InvalidOperationException("Something's gone wrong!")
                    : state.Continue();

    private static readonly SyncPipelineStep<YarpRequestPipelineState> InnerPipelineInstance =
        YarpRequestPipeline.Build(
            HandleFizz.Log(),
            HandleBuzz.Log()).Log(name: "InnerPipeline");

    private static readonly SyncPipelineStep<HandlerState<PathString, string?>> HandleFoo =
        static state => state.Input == "/foo"
                            ? state.Handled("We're looking at a foo")
                            : state.NotHandled();

    private static readonly SyncPipelineStep<HandlerState<PathString, string?>> HandleBar =
        static state => state.Input == "/bar"
                    ? state.Handled(null)
                    : state.NotHandled();

    private static readonly SyncPipelineStep<HandlerState<PathString, string?>> MessageHandlerPipelineInstance =
        HandlerPipeline.Build(
            HandleFoo.Log(),
            HandleBar.Log()).Log(name: "MessageHandlerPipeline");

    private static readonly SyncPipelineStep<YarpRequestPipelineState> AddMessageToHttpContext =
        MessageHandlerPipelineInstance
            .Bind(
                wrap: static (YarpRequestPipelineState state) => HandlerState<PathString, string?>.For(state.GetNominalRequestSignature().Path, state.Logger),
                unwrap: static (state, innerState) =>
                {
                    if (innerState.WasHandled(out string? message))
                    {
                        if (message is string msg)
                        {
                            return state.Continue();
                        }
                        else
                        {
                            return state.TerminateWith(NonForwardedResponseDetails.ForStatusCode(400));
                        }
                    }

                    return state.Continue();
                });

    private static readonly Func<YarpRequestPipelineState, SyncPipelineStep<YarpRequestPipelineState>> ChooseMessageContextHandler =
            static state => state.GetNominalRequestSignature().QueryString.HasValue
                                ? state => state.TerminateWith(NonForwardedResponseDetails.ForStatusCode(400))
                                : AddMessageToHttpContext;

    private static readonly Func<YarpRequestPipelineState, Exception, YarpRequestPipelineState> CatchPipelineException =
        static (state, exception) =>
        {
            if (state.Logger.IsEnabled(LogLevel.Warning))
            {
                // If we are logging warnings, we want to tell people about this.
                if (state.Logger.IsEnabled(LogLevel.Debug))
                {
                    // We have debug level diagnostics, so include the exception
                    state.Logger.LogWarning(Pipeline.EventIds.TransientFailure, exception, "Exception during processing produced transient failure.");
                }
                else
                {
                    // Otherwise include just a plain text warning.
                    state.Logger.LogWarning(Pipeline.EventIds.TransientFailure, "Exception during processing produced transient failure.");
                }
            }

            return state.TransientFailure(new YarpPipelineError("Unable to execute the pipeline.", exception));
        };

    private static readonly SyncPipelineStep<YarpRequestPipelineState> HandleRoot =
        static state => state.GetNominalRequestSignature().Path == "/" // You can write in this style where we execute steps directly
                ? state.TerminateWith(new ForwardedRequestDetails() { ClusterId = "Nonsense" })
                : InnerPipelineInstance(state);

    private static readonly PipelineStep<YarpRequestPipelineState> AsyncDelay =
        static async state =>
        {
            await Task.Delay(0).ConfigureAwait(false);
            return state.Continue();
        };

    /// <summary>
    /// Gets an instance of an example yarp pipeline handler.
    /// </summary>
    public static SyncPipelineStep<YarpRequestPipelineState> Instance { get; } =
        YarpRequestPipeline.Build(
            HandleRoot,
            YarpRequestPipeline.CurrentSync.Choose(ChooseMessageContextHandler).Log()).Log(name: "MainPipeline")
        .Catch(CatchPipelineException)
        .Retry(
            YarpRetry.TransientWithCountPolicy(5)) // YarpRetry automatically logs
        .OnError(state => state.TerminateWith(NonForwardedResponseDetails.ForStatusCode(500)));

    /// <summary>
    /// Gets an instance of an example yarp pipeline handler.
    /// </summary>
    public static PipelineStep<YarpRequestPipelineState> ForceAsyncInstance { get; } =
        YarpRequestPipeline.Build(
            HandleRoot.Log().ToAsync(), // You can make the named item async
            AsyncDelay,
            YarpRequestPipeline.CurrentSync.Choose(ChooseMessageContextHandler).ToAsync().Log()).Log(name: "MainAsyncPipeline")
        .Catch(CatchPipelineException)
        .Retry(
            YarpRetry.TransientWithCountPolicy(5),
            YarpRetry.FixedDelayStrategy(TimeSpan.Zero)) // YarpRetry automatically logs
        .OnError(state => state.TerminateWith(NonForwardedResponseDetails.ForStatusCode(500)));
}