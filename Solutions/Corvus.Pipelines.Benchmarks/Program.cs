// <copyright file="Program.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

using Perfolizer.Mathematics.OutlierDetection;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(
    args,
#if DEBUG
    new DebugInProcessConfigDry());
#else
    ManualConfig.Create(DefaultConfig.Instance));
#endif