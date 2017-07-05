﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyModel;
using DependencyContextCompilationOptions = Microsoft.Extensions.DependencyModel.CompilationOptions;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    public class CSharpCompiler
    {
        private readonly RazorReferenceManager _referenceManager;
        private readonly DebugInformationFormat _pdbFormat = SymbolsUtility.SupportsFullPdbGeneration() ?
            DebugInformationFormat.Pdb :
            DebugInformationFormat.PortablePdb;

        public CSharpCompiler(RazorReferenceManager manager, IHostingEnvironment hostingEnvironment)
            : this(manager, hostingEnvironment, GetDependencyContextCompilationOptions(hostingEnvironment))
        {
        }

        internal CSharpCompiler(
            RazorReferenceManager manager,
            IHostingEnvironment hostingEnvironment,
            DependencyContextCompilationOptions dependencyContextOptions)
        {
            _referenceManager = manager ?? throw new ArgumentNullException(nameof(manager));
            if (hostingEnvironment == null)
            {
                throw new ArgumentNullException(nameof(hostingEnvironment));
            }

            ParseOptions = GetParseOptions(hostingEnvironment, dependencyContextOptions);
            CSharpCompilationOptions = GetCompilationOptions(hostingEnvironment, dependencyContextOptions);
            EmitOptions = new EmitOptions(debugInformationFormat: _pdbFormat);
        }

        public CSharpParseOptions ParseOptions { get; }

        public CSharpCompilationOptions CSharpCompilationOptions { get; }

        public EmitOptions EmitOptions { get; }

        private static CSharpCompilationOptions GetCompilationOptions(
            IHostingEnvironment hostingEnvironment,
            DependencyContextCompilationOptions dependencyContextOptions)
        {
            var csharpCompilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

            // Disable 1702 until roslyn turns this off by default
            csharpCompilationOptions = csharpCompilationOptions.WithSpecificDiagnosticOptions(
                new Dictionary<string, ReportDiagnostic>
                {
                    {"CS1701", ReportDiagnostic.Suppress}, // Binding redirects
                    {"CS1702", ReportDiagnostic.Suppress},
                    {"CS1705", ReportDiagnostic.Suppress}
                });

            if (dependencyContextOptions.AllowUnsafe.HasValue)
            {
                csharpCompilationOptions = csharpCompilationOptions.WithAllowUnsafe(
                    dependencyContextOptions.AllowUnsafe.Value);
            }

            OptimizationLevel optimizationLevel;
            if (dependencyContextOptions.Optimize.HasValue)
            {
                optimizationLevel = dependencyContextOptions.Optimize.Value ?
                    OptimizationLevel.Release :
                    OptimizationLevel.Debug;
            }
            else
            {
                optimizationLevel = hostingEnvironment.IsDevelopment() ?
                    OptimizationLevel.Debug :
                    OptimizationLevel.Release;
            }
            csharpCompilationOptions = csharpCompilationOptions.WithOptimizationLevel(optimizationLevel);

            if (dependencyContextOptions.WarningsAsErrors.HasValue)
            {
                var reportDiagnostic = dependencyContextOptions.WarningsAsErrors.Value ?
                    ReportDiagnostic.Error :
                    ReportDiagnostic.Default;
                csharpCompilationOptions = csharpCompilationOptions.WithGeneralDiagnosticOption(reportDiagnostic);
            }

            return csharpCompilationOptions;
        }

        private static CSharpParseOptions GetParseOptions(
            IHostingEnvironment hostingEnvironment,
            DependencyContextCompilationOptions dependencyContextOptions)
        {
            var configurationSymbol = hostingEnvironment.IsDevelopment() ? "DEBUG" : "RELEASE";
            var defines = dependencyContextOptions.Defines.Concat(new[] { configurationSymbol });

            var parseOptions = new CSharpParseOptions(
                LanguageVersion.CSharp7_1,
                preprocessorSymbols: defines);

            return parseOptions;
        }

        public SyntaxTree CreateSyntaxTree(SourceText sourceText)
        {
            return CSharpSyntaxTree.ParseText(
                sourceText,
                options: ParseOptions);
        }

        public CSharpCompilation CreateCompilation(string assemblyName)
        {
            return CSharpCompilation.Create(
                assemblyName,
                options: CSharpCompilationOptions,
                references: _referenceManager.CompilationReferences);
        }

        // Internal for unit testing.
        internal static DependencyContextCompilationOptions GetDependencyContextCompilationOptions(
            IHostingEnvironment hostingEnvironment)
        {
            if (!string.IsNullOrEmpty(hostingEnvironment.ApplicationName))
            {
                var applicationAssembly = Assembly.Load(new AssemblyName(hostingEnvironment.ApplicationName));
                var dependencyContext = DependencyContext.Load(applicationAssembly);
                if (dependencyContext?.CompilationOptions != null)
                {
                    return dependencyContext.CompilationOptions;
                }
            }

            return DependencyContextCompilationOptions.Default;
        }
    }
}
