// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Razor;
using Microsoft.CodeAnalysis;

namespace Microsoft.AspNetCore.Mvc.Razor.Precompilation.Internal
{
    public class PrecompilationResult
    {
        public string OutputPath { get; set; }

        public List<RazorError> RazorErrors { get; } = new List<RazorError>();

        public List<Diagnostic> RoslynErrors { get; } = new List<Diagnostic>();

        public bool Success => (RazorErrors.Count == 0) && (RoslynErrors.Count == 0);

        public List<CompileOutputs> CompileOutputs { get; } = new List<CompileOutputs>();
    }
}
