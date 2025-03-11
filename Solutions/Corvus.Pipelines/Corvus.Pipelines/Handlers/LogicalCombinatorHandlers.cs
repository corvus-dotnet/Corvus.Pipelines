// <copyright file="LogicalCombinatorHandlers.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Pipelines.Handlers;

/// <summary>
/// Combinators for building handlers that evaluate logical expressions, including short-circuiting evaluation
/// for <c>AND</c> and <c>OR</c> operations.
/// </summary>
/// <remarks>
/// <para>
/// The short-circuiting methods in this class operate in pairs. If you have a series of handlers that produce a
/// <see langword="bool" />, and want to combine them into a single handler that produces
/// <see langword="true" /> if and only if all of the individual handlers return <see langword="true" />,
/// you wrap each individual step with <see cref="BindPredicateForAnd{TInput}"/>, and pass all of these into
/// <see cref="And{TInput}(Corvus.Pipelines.SyncPipelineStep{Corvus.Pipelines.Handlers.HandlerState{TInput, bool}}[])"/>.
/// This is the handler equivalent of the <c>&amp;&amp;</c> operator in C# (in that it immediately stops evaluation if
/// a step returns <see langword="false"/>. Similarly, the operation equivalent to
/// <c>||</c> is provided by <see cref="BindPredicateForOr{TInput}"/> and
/// <see cref="Or{TInput}(Corvus.Pipelines.SyncPipelineStep{Corvus.Pipelines.Handlers.HandlerState{TInput, bool}}[])"/>.
/// </para>
/// </remarks>
public static class LogicalCombinatorHandlers
{
    /// <summary>
    /// Wraps a single <see langword="bool"/>-producing handler in a form suitable for passing
    /// to <see cref="And{TInput}(SyncPipelineStep{HandlerState{TInput, bool}}[])"/>.
    /// </summary>
    /// <typeparam name="TInput">The handler input type.</typeparam>
    /// <param name="predicate">
    /// The handler to wrap. This must handle the input: the returned wrapper will throw an
    /// <see cref="InvalidOperationException"/> when invoked if this predicate does not
    /// return a handled state.
    /// </param>
    /// <returns>
    /// The handler, wrapped in a form suitable for passing to
    /// <see cref="And{TInput}(SyncPipelineStep{HandlerState{TInput, bool}}[])"/>.
    /// </returns>
    public static SyncPipelineStep<HandlerState<TInput, bool>> BindPredicateForAnd<TInput>(
        SyncPipelineStep<HandlerState<TInput, bool>> predicate)
    {
        // The reason this is public (and not just a thing that And does for you) is that it
        // avoids the creation of an extra array to hold the wrapped handlers passed to
        // HandlerPipeline.Build. By making BindPredicateForAnd public, code gen can directly create the
        // array that the inner pipeline loop iterates over.
        return predicate.Bind(static state => state.WasHandled(out bool result)
            ? (result ? state.NotHandled() : state.Handled(false))
            : throw new InvalidOperationException("Matcher did not handle the state."));
    }

    /// <summary>
    /// Wraps a series of steps produced by <see cref="BindPredicateForAnd{TInput}(SyncPipelineStep{HandlerState{TInput, bool}})"/>
    /// with a single handler that produces <see langword="true"/> only if all of the wrapped steps produce
    /// <see langword="true" />, and which terminates evalution early as soon as any step produces <see langword="false"/>.
    /// </summary>
    /// <typeparam name="TInput">The handler input type.</typeparam>
    /// <param name="andSteps">
    /// The handlers produced by <see cref="And{TInput}(SyncPipelineStep{HandlerState{TInput, bool}}[])"/>.
    /// </param>
    /// <returns>
    /// A handler.
    /// </returns>
    public static SyncPipelineStep<HandlerState<TInput, bool>> And<TInput>(
        params SyncPipelineStep<HandlerState<TInput, bool>>[] andSteps)
    {
        // We follow the standard predicate logic 'vacuous truth' rule in which an empty conjunction is true.
        // As it happens, the implementation we use for non-empty inputs does produce the right result, but
        // is horrifically inefficient, so we just hard-code the result in this case.
        if (andSteps.Length == 0)
        {
            return static state => state.Handled(true);
        }

        // As an implementation detail, the wrappers returned by BindPredicateForAnd return a NotHandled state
        // when their predicate returns true (to allow processing to continue), and a Handled(false)
        // state if a predicate returns false (to short-circuit evaluation).
        return HandlerPipeline.Build(andSteps).Bind(
            static state => state.WasHandled(out bool matched)
            ? (matched
                ? throw new InvalidOperationException("AND submatched should signify match with NotHandled; true result not expected here")
                : state.Handled(false))
            : state.Handled(true));
    }

    /// <summary>
    /// Wraps a single <see langword="bool"/>-producing handler in a form suitable for passing
    /// to <see cref="Or{TInput}(SyncPipelineStep{HandlerState{TInput, bool}}[])"/>.
    /// </summary>
    /// <typeparam name="TInput">The handler input type.</typeparam>
    /// <param name="predicate">
    /// The handler to wrap. This must handle the input: the returned wrapper will throw an
    /// <see cref="InvalidOperationException"/> when invoked if this predicate does not
    /// return a handled state.
    /// </param>
    /// <returns>
    /// The handler, wrapped in a form suitable for passing to
    /// <see cref="Or{TInput}(SyncPipelineStep{HandlerState{TInput, bool}}[])"/>.
    /// </returns>
    public static SyncPipelineStep<HandlerState<TInput, bool>> BindPredicateForOr<TInput>(
        SyncPipelineStep<HandlerState<TInput, bool>> predicate)
    {
        return predicate.Bind(static state => state.WasHandled(out bool result)
            ? (result ? state.Handled(true) : state.NotHandled())
            : throw new InvalidOperationException("Matcher did not handle the state."));
    }

    /// <summary>
    /// Wraps a series of steps produced by <see cref="BindPredicateForOr{TInput}(SyncPipelineStep{HandlerState{TInput, bool}})"/>
    /// with a single handler that produces <see langword="true"/> only if all of the wrapped steps produce
    /// <see langword="true" />, and which terminates evalution early as soon as any step produces <see langword="false"/>.
    /// </summary>
    /// <typeparam name="TInput">The handler input type.</typeparam>
    /// <param name="orSteps">
    /// The handlers produced by <see cref="Or{TInput}(SyncPipelineStep{HandlerState{TInput, bool}}[])"/>.
    /// </param>
    /// <returns>
    /// A handler.
    /// </returns>
    public static SyncPipelineStep<HandlerState<TInput, bool>> Or<TInput>(
        params SyncPipelineStep<HandlerState<TInput, bool>>[] orSteps)
    {
        // We follow the standard predicate logic rule in which an empty disjunction is false.
        // As it happens, the implementation we use for non-empty inputs does produce the right result, but
        // is horrifically inefficient, so we just hard-code the result in this case.
        if (orSteps.Length == 0)
        {
            return static state => state.Handled(false);
        }

        // As an implementation detail, the wrappers returned by OrStep return a NotHandled state
        // when their predicate returns false (to allow processing to continue), and a Handled(true)
        // state if a predicate returns true (to short-circuit evaluation).
        return HandlerPipeline.Build(orSteps).Bind(
            static state => state.WasHandled(out bool matched)
            ? (matched
                ? state.Handled(true)
                : throw new InvalidOperationException("OR submatched should signify match with NotHandled; false result not expected here"))
            : state.Handled(false));
    }

    /// <summary>
    /// Wraps a step that produces a <see langword="bool"/>. The resulting wrapper returns the
    /// negation of the result of the wrapped step.
    /// </summary>
    /// <typeparam name="TInput">The handler input type.</typeparam>
    /// <param name="predicate">
    /// The handler to wrap. This must handle the input: the returned wrapper will throw an
    /// <see cref="InvalidOperationException"/> when invoked if this predicate does not
    /// return a handled state.
    /// </param>
    /// <returns>
    /// A handler.
    /// </returns>
    public static SyncPipelineStep<HandlerState<TInput, bool>> Not<TInput>(
        SyncPipelineStep<HandlerState<TInput, bool>> predicate)
    {
        return predicate.Bind(static state => state.WasHandled(out bool result)
            ? state.Handled(!result)
            : throw new InvalidOperationException("Matcher did not handle the state."));
    }
}