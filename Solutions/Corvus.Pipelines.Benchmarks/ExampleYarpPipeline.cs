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
    private static readonly SyncPipelineStep<YarpPipelineState> InnerPipelineInstance =
    YarpPipeline.Build(
        static state => state.RequestTransformContext.Path == "/fizz"
                    ? state.TerminateAndForward()
                    : state.Continue(),
        static state => state.RequestTransformContext.Path == "/buzz"
                    ? throw new InvalidOperationException("Something's gone wrong!")
                    : state.Continue());

    private static readonly SyncPipelineStep<HandlerState<PathString, string?>> MessageHandlerPipelineInstance =
        HandlerPipeline.Build<PathString, string?>(
    static state => state.Input == "/foo"
                ? state.Handled("We're looking at a foo")
                : state.NotHandled(),
    static state => state.Input == "/bar"
                ? state.Handled(null)
                : state.NotHandled());

    private static readonly SyncPipelineStep<YarpPipelineState> AddMessageToHttpContext =
        MessageHandlerPipelineInstance
            .Bind(
                wrap: static (YarpPipelineState state) => HandlerState<PathString, string?>.For(state.RequestTransformContext.Path),
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
                            return state.TerminateWith(NonForwardedResponseDetails.ForStatusCode(400));
                        }
                    }

                    return state.Continue();
                });

    private static readonly Func<YarpPipelineState, SyncPipelineStep<YarpPipelineState>> ChooseMessageContextHandler =
            static state => state.RequestTransformContext.Query.QueryString.HasValue
                                ? state => state.TerminateWith(NonForwardedResponseDetails.ForStatusCode(400))
                                : AddMessageToHttpContext;

    private static readonly Func<YarpPipelineState, Exception, YarpPipelineState> CatchPipelineException =
        static (state, exception) => state.TransientFailure(new("Unable to execute the pipeline.", exception));

    /// <summary>
    /// Gets an instance of an example yarp pipeline handler.
    /// </summary>
    public static PipelineStep<YarpPipelineState> Instance { get; } =
        YarpPipeline.Build(
            static state => state.RequestTransformContext.Path == "/" // You can write in this style where we execute steps directly
                ? state.TerminateAndForward()
                : InnerPipelineInstance(state),
            YarpPipeline.CurrentSync.Choose(ChooseMessageContextHandler), // But we prefer this style where we hide away the state
            static state => state.RequestTransformContext.HttpContext.Items["Message"] is string message
                        ? state.Continue()
                        : state.TerminateWith(NonForwardedResponseDetails.ForStatusCode(404)))
        .Catch(CatchPipelineException).ToAsync()
        .Retry(
            static state => state.ExecutionStatus == PipelineStepStatus.TransientFailure && state.FailureCount < 5, // This is doing a simple count, but you could layer policy based on state.TryGetErrorDetails()
            async state =>
            {
                await Task.Delay(0).ConfigureAwait(false); // You could do a back off using state.FailureCount, or whatever!
                return state;
            })
        .OnError(state => state.TerminateWith(NonForwardedResponseDetails.ForStatusCode(500)));

    /// <summary>
    /// Gets an instance of an example yarp pipeline handler.
    /// </summary>
    public static PipelineStep<YarpPipelineState> ForceAsyncInstance { get; } =
        YarpPipeline.Build(
            static state => state.RequestTransformContext.Path == "/" // You can write in this style where we execute steps directly
                ? ValueTask.FromResult(state.TerminateAndForward())
                : ValueTask.FromResult(InnerPipelineInstance(state)),
            async state =>
            {
                await Task.Delay(0).ConfigureAwait(false);
                return state.Continue();
            },
            YarpPipeline.Current.Choose(ChooseMessageContextHandler), // But we prefer this style where we hide away the state
            static state => ValueTask.FromResult(state.RequestTransformContext.HttpContext.Items["Message"] is string message
                        ? state.Continue()
                        : state.TerminateWith(NonForwardedResponseDetails.ForStatusCode(404))))
        .Catch(CatchPipelineException)
        .Retry(
            static state => state.FailureCount < 5, // This is doing a simple count, but you could layer policy based on state.TryGetErrorDetails()
            async state =>
            {
                await Task.Delay(0).ConfigureAwait(false); // You could do a back off using state.FailureCount, or whatever!
                return state;
            })
        .OnError(state => state.TerminateWith(NonForwardedResponseDetails.ForStatusCode(500)));
}