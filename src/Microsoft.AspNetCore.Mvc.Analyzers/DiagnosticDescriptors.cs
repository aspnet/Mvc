﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    public static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor MVC1000_HtmlHelperPartialShouldBeAvoided =
            new DiagnosticDescriptor(
                "MVC1000",
                "Use of IHtmlHelper.{0} should be avoided.",
                "Use of IHtmlHelper.{0} may result in application deadlocks. Consider using <partial> Tag Helper or IHtmlHelper.{0}Async.",
                "Usage",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MVC1001_FiltersShouldNotBeAppliedToPageHandlerMethods =
            new DiagnosticDescriptor(
                "MVC1001",
                "Filters cannot be applied to page handler methods.",
                "'{0}' cannot be applied to Razor Page handler methods. It may be applied either to the Razor Page model or applied globally.",
                "Usage",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MVC1002_RouteAttributesShouldNotBeAppliedToPageHandlerMethods =
            new DiagnosticDescriptor(
                "MVC1002",
                "Route attributes cannot be applied to page handler methods.",
                "'{0}' cannot be applied to Razor Page handler methods. Routes for Razor Pages must be declared using the @page directive or using conventions.",
                "Usage",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MVC1003_RouteAttributesShouldNotBeAppliedToPageModels =
            new DiagnosticDescriptor(
                "MVC1003",
                "Route attributes cannot be applied to page models.",
                "'{0}' cannot be applied to a Razor Page model. Routes for Razor Pages must be declared using the @page directive or using conventions.",
                "Usage",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MVC1004_ParameterNameCollidesWithTopLevelProperty =
            new DiagnosticDescriptor(
                "MVC1004",
                "Rename model bound parameter.",
                "Property on type '{0}' has the same name as parameter '{1}'. This may result in incorrect model binding. Consider renaming the parameter or using a model binding attribute to override the name.",
                "Naming",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                helpLinkUri: "https://aka.ms/AA20pbc");
    }
}
