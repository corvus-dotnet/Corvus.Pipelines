// <copyright file="ICanFail.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Pipelines;

/// <summary>
/// An interface implemented by state for steps that can fail.
/// </summary>
/// <typeparam name="TSelf">The type implementing the capability.</typeparam>
/// <remarks>
/// This is used by retry operators to determine whether a failure is transient or permanent.
/// </remarks>
public interface ICanFail<TSelf>
    where TSelf : struct, ICanFail<TSelf>
{
    /// <summary>
    /// Gets the operation status.
    /// </summary>
    PipelineStepStatus ExecutionStatus { get; init; }
}