// <copyright file="PipelineStepStatus.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Pipelines;

/// <summary>
/// Determines the success status of the operation.
/// </summary>
public enum PipelineStepStatus
{
    /// <summary>
    /// The operation succeeded.
    /// </summary>
    Success,

    /// <summary>
    /// The operation failed permanently and cannot be retried.
    /// </summary>
    PermanentFailure,

    /// <summary>
    /// The operation failed transiently and could be retried.
    /// </summary>
    TransientFailure,
}