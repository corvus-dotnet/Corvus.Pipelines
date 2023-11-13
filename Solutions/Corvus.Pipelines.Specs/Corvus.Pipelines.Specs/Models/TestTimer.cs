// <copyright file="TestTimer.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using System.Diagnostics;
using NUnit.Framework;

namespace Corvus.Pipelines.Specs.Models;

public class TestTimer
{
    private readonly Stopwatch stopwatch;

    public TestTimer()
    {
        this.stopwatch = new();
    }

    public void Start()
    {
        this.stopwatch.Start();
    }

    public void Stop()
    {
        this.stopwatch.Stop();
    }

    public void AssertInRange(TimeSpan elapsedTime, TimeSpan permittedError)
    {
        double elapsed = elapsedTime.TotalMilliseconds;
        double error = permittedError.TotalMilliseconds;

        Assert.AreEqual(elapsed, this.stopwatch.ElapsedMilliseconds, error);
    }
}