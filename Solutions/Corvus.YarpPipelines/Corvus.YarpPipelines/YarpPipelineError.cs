// <copyright file="YarpPipelineError.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.YarpPipelines;

/// <summary>
/// Represents an error that occurred during YARP pipeline execution.
/// </summary>
/// <param name="Message">The error message given human-readable information about the error.</param>
/// <param name="InnerException">The exception that occurred, if any.</param>
public record YarpPipelineError(string Message, Exception? InnerException = null);