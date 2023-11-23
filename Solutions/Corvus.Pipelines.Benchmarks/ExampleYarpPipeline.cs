// <copyright file="ExampleYarpPipeline.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using Corvus.Pipelines;
using Corvus.Pipelines.Handlers;
using Corvus.YarpPipelines;
using Microsoft.AspNetCore.Http;

namespace PipelineExamples;

/// <summary>
/// A sample YARP pipeline illustrating a smorgasbord of capabilities.
/// </summary>
public static class ExampleYarpPipeline
{
    private static readonly SyncPipelineStep<YarpPipelineState> HandleFizz =
        static state => state.RequestSignature.Path == "/fizz"
                    ? state.TerminateAndForward()
                    : state.Continue();

    private static readonly SyncPipelineStep<YarpPipelineState> HandleBuzz =
        static state => state.RequestSignature.Path == "/buzz"
                    ? throw new InvalidOperationException("Something's gone wrong!")
                    : state.Continue();

    private static readonly SyncPipelineStep<YarpPipelineState> InnerPipelineInstance =
        YarpPipeline.Build(
            HandleFizz,
            HandleBuzz);

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
            HandleFoo,
            HandleBar);

    private static readonly SyncPipelineStep<YarpPipelineState> AddMessageToHttpContext =
        MessageHandlerPipelineInstance
            .Bind(
                wrap: static (YarpPipelineState state) => HandlerState<PathString, string?>.For(state.RequestSignature.Path, state.Logger),
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

    private static readonly Func<YarpPipelineState, SyncPipelineStep<YarpPipelineState>> ChooseMessageContextHandler =
            static state => state.RequestSignature.QueryString.HasValue
                                ? state => state.TerminateWith(NonForwardedResponseDetails.ForStatusCode(400))
                                : AddMessageToHttpContext;

    private static readonly Func<YarpPipelineState, Exception, YarpPipelineState> CatchPipelineException =
        static (state, exception) => state.TransientFailure(new YarpPipelineError("Unable to execute the pipeline.", exception));

    private static readonly SyncPipelineStep<YarpPipelineState> HandleRoot =
        static state => state.RequestSignature.Path == "/" // You can write in this style where we execute steps directly
                ? state.TerminateAndForward()
                : InnerPipelineInstance(state);

    private static readonly PipelineStep<YarpPipelineState> AsyncDelay =
        static async state =>
        {
            await Task.Delay(0).ConfigureAwait(false);
            return state.Continue();
        };

    /// <summary>
    /// Gets an instance of an example yarp pipeline handler.
    /// </summary>
    public static SyncPipelineStep<YarpPipelineState> Instance { get; } =
        YarpPipeline.Build(
            HandleRoot,
            YarpPipeline.CurrentSync.Choose(ChooseMessageContextHandler))
        .Catch(CatchPipelineException)
        .Retry(
            YarpRetry.TransientWithCountPolicy(5)) // YarpRetry automatically logs
        .OnError(state => state.TerminateWith(NonForwardedResponseDetails.ForStatusCode(500)));

    /// <summary>
    /// Gets an instance of an example yarp pipeline handler.
    /// </summary>
    public static PipelineStep<YarpPipelineState> ForceAsyncInstance { get; } =
        YarpPipeline.Build(
            HandleRoot.ToAsync(), // You can make the named item async
            AsyncDelay,
            YarpPipeline.CurrentSync.Choose(ChooseMessageContextHandler).ToAsync())
        .Catch(CatchPipelineException)
        .Retry(
            YarpRetry.TransientWithCountPolicy(5),
            YarpRetry.FixedDelayStrategy(TimeSpan.Zero)) // YarpRetry automatically logs
        .OnError(state => state.TerminateWith(NonForwardedResponseDetails.ForStatusCode(500)));
}