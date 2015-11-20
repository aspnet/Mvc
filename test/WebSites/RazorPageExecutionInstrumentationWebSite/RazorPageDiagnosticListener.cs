// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Extensions.DiagnosticAdapter;

namespace RazorPageExecutionInstrumentationWebSite
{
    public class RazorPageDiagnosticListener
    {
        public List<string> PageInstrumentationData { get; } = new List<string>();

        [DiagnosticName("Microsoft.AspNet.Mvc.Razor.BeginInstrumentationContext")]
        public virtual void OnBeginPageInstrumentationContext(
            string path,
            int position,
            int length,
            bool isLiteral)
        {
            var literal = isLiteral ? "Literal" : "Non-literal";
            PageInstrumentationData.Add($"{path}: {literal} at {position} contains {length} characters.");
        }
    }
}
