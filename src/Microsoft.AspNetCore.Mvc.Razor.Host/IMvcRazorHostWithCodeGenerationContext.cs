// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.CodeGenerators;

namespace Microsoft.AspNetCore.Mvc.Razor
{
    /// <summary>
    /// Specifies the contracts for a Razor host that parses Razor files and generates C# code.
    /// </summary>
    public interface IMvcRazorHostWithTemplateEngineContext
    {
        /// <summary>
        /// Parses and generates the contents of a Razor file.
        /// </summary>
        /// <returns>The <see cref="GeneratorResults"/>.</returns>
        GeneratorResults GenerateCode(TemplateEngineContext context);
    }
}
