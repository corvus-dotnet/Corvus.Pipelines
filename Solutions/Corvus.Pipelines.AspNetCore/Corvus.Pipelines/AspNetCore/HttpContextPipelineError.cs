// <copyright file="HttpContextPipelineError.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.Pipelines.AspNetCore;

/// <summary>
/// Represents an error that occurred during HTTP pipeline execution.
/// </summary>
/// <param name="Message">The error message given human-readable information about the error.</param>
/// <param name="InnerException">The exception that occurred, if any.</param>
public readonly record struct HttpContextPipelineError(string Message, Exception? InnerException = null);