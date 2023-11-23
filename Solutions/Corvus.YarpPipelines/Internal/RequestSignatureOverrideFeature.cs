// <copyright file="RequestSignatureOverrideFeature.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.YarpPipelines.Internal;

/// <summary>
/// Holds the nominal request signature for requests where this is not the same
/// as the actual request signature.
/// </summary>
internal record RequestSignatureOverrideFeature(RequestSignature RequestSignature);