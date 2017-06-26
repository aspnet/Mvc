// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    public static class MvcRazorDiagnosticSourceExtensions
    {
        public static void BeforeRazorPage(
            this DiagnosticSource diagnosticSource,
            IRazorPage page,
            ViewContext viewContext)
        {
            if (diagnosticSource.IsEnabled("Microsoft.AspNetCore.Mvc.Razor.BeforeRazorView"))
            {
                diagnosticSource.Write(
                    "Microsoft.AspNetCore.Mvc.Razor.BeforeRazorView",
                    new
                    {
                        page = page,
                        viewContext = viewContext,
                        actionDescriptor = viewContext.ActionDescriptor,
                        httpContext = viewContext.HttpContext,
                    });
            }
        }

        public static void AfterRazorPage(
            this DiagnosticSource diagnosticSource,
            IRazorPage page,
            ViewContext viewContext)
        {
            if (diagnosticSource.IsEnabled("Microsoft.AspNetCore.Mvc.Razor.AfterRazorView"))
            {
                diagnosticSource.Write(
                    "Microsoft.AspNetCore.Mvc.Razor.AfterRazorView",
                    new
                    {
                        page = page,
                        viewContext = viewContext,
                        actionDescriptor = viewContext.ActionDescriptor,
                        httpContext = viewContext.HttpContext,
                    });
            }
        }
    }
}
