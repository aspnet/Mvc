// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    public static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor MVC1000_ApiActionsMustBeAttributeRouted =
            new DiagnosticDescriptor(
                "MVC1000",
                "Actions on types annotated with ApiControllerAttribute must be attribute routed.",
                "Actions on types annotated with ApiControllerAttribute must be attribute routed.",
                "Usage",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MVC1001_ApiActionsHaveBadModelStateFilter =
            new DiagnosticDescriptor(
                "MVC1001",
                "Actions on types annotated with ApiControllerAttribute do not require explicit ModelState validity check.",
                "Actions on types annotated with ApiControllerAttribute do not require explicit ModelState validity check.",
                "Usage",
                DiagnosticSeverity.Info,
                isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MVC1002_ApiActionsShouldReturnActionResultOf =
            new DiagnosticDescriptor(
                "MVC1002",
                "Actions on types annotated with ApiControllerAttribute should return ActionResult<T> when allowed.",
                "Actions on types annotated with ApiControllerAttribute should return ActionResult<T> when allowed.",
                "Usage",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true);
    }
}
