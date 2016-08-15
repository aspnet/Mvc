// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.AspNetCore.Mvc.Razor.Precompilation.Design.Internal
{
    public class PrecompileRunCommand
    {
        public static readonly string ApplicationNameTemplate = "--applicationName";
        public static readonly string OutputPathTemplate = "--output-path";

        private CommandOption OutputPathOption { get; set; }

        private CommandOption ApplicationNameOption { get; set; }

        

        private MvcServicesProvider ServicesProvider { get; set; }

        private CommonOptions Options { get; } = new CommonOptions();

        private StrongNameOptions StrongNameOptions { get; } = new StrongNameOptions();

        private string ProjectPath { get; set; }

        public void Configure(CommandLineApplication app)
        {
            Options.Configure(app);
            StrongNameOptions.Configure(app);

            OutputPathOption = app.Option(
               OutputPathTemplate,
                "Path to the emit the precompiled assembly to.",
                CommandOptionType.SingleValue);

            ApplicationNameOption = app.Option(
                ApplicationNameTemplate,
                "Name of the application to produce precompiled assembly for.",
                CommandOptionType.SingleValue);

            app.OnExecute(() => Execute());
        }

        private int Execute()
        {
            ParseArguments();

            ServicesProvider = new MvcServicesProvider(
                ProjectPath,
                ApplicationNameOption.Value(),
                Options.ContentRootOption.Value(),
                Options.ConfigureCompilationType.Value());

            Console.WriteLine("Running Razor view precompilation.");

            var results = ParseViews();
            var success = true;
            foreach (var result in results)
            {
                if (!result.GeneratorResults.Success)
                {
                    success = false;
                    foreach (var error in result.GeneratorResults.ParserErrors)
                    {
                        Console.Error.WriteLine($"{error.Location.FilePath} ({error.Location.LineIndex}): {error.Message}");
                    }
                }
            }

            if (!success)
            {
                return 1;
            }

            var precompileAssemblyName = $"{ApplicationNameOption.Value()}{AssemblyPart.PrecompiledViewsAssemblySuffix}";
            var compilation = CompileViews(results, precompileAssemblyName);

            var assemblyPath = Path.Combine(OutputPathOption.Value(), precompileAssemblyName + ".dll");
            var emitResult = EmitAssembly(compilation, assemblyPath);

            if (!emitResult.Success)
            {
                foreach (var diagnostic in emitResult.Diagnostics)
                {
                    Console.Error.WriteLine(CSharpDiagnosticFormatter.Instance.Format(diagnostic));
                }

                return 1;
            }

            Console.WriteLine($"Successfully compiled {results.Count} Razor views.");
            Console.WriteLine($"Precompiled views emitted to {assemblyPath}.");
            return 0;
        }

        private EmitResult EmitAssembly(CSharpCompilation compilation, string assemblyPath)
        {
            EmitResult emitResult;
            using (var assemblyStream = File.OpenWrite(assemblyPath))
            {
                using (var pdbStream = File.OpenWrite(Path.ChangeExtension(assemblyPath, ".pdb")))
                {
                    emitResult = compilation.Emit(
                        assemblyStream,
                        pdbStream,
                        options: ServicesProvider.Compiler.EmitOptions);
                }
            }

            return emitResult;
        }

        private CSharpCompilation CompileViews(List<ViewCompilationInfo> results, string assemblyname)
        {
            var compiler = ServicesProvider.Compiler;
            var compilation = compiler.CreateCompilation(assemblyname);

            foreach (var result in results)
            {
                var sourceText = SourceText.From(result.GeneratorResults.GeneratedCode, Encoding.UTF8);
                var syntaxTree = compiler.CreateSyntaxTree(sourceText);
                compilation = compilation.AddSyntaxTrees(syntaxTree);
                result.TypeName = ReadTypeInfo(compilation, syntaxTree);
            }

            compilation = compiler.ProcessCompilation(compilation);
            var codeGenerator = new ViewCollectionCodeGenerator(compiler, compilation);
            codeGenerator.AddViewFactory(results);

            var assemblyName = new AssemblyName(ApplicationNameOption.Value());
            assemblyName = Assembly.Load(assemblyName).GetName();
            codeGenerator.AddAssemblyMetadata(assemblyName, StrongNameOptions);

            return codeGenerator.Compilation;
        }

        private void ParseArguments()
        {
            ProjectPath = Options.ProjectArgument.Value;
            if (string.IsNullOrEmpty(ProjectPath))
            {
                throw new ArgumentException("Project path not specified.");
            }

            if (!OutputPathOption.HasValue())
            {
                throw new ArgumentException($"Option {OutputPathTemplate} does not specify a value.");
            }

            if (!ApplicationNameOption.HasValue())
            {
                throw new ArgumentException($"Option {ApplicationNameTemplate} does not specify a value.");
            }
        }

        private List<ViewCompilationInfo> ParseViews()
        {
            var results = new List<ViewCompilationInfo>();
            var files = new List<RelativeFileInfo>();
            GetRazorFiles(ServicesProvider.FileProvider, files, root: string.Empty);
            foreach (var fileInfo in files)
            {
                using (var fileStream = fileInfo.FileInfo.CreateReadStream())
                {
                    var result = ServicesProvider.Host.GenerateCode(fileInfo.RelativePath, fileStream);
                    results.Add(new ViewCompilationInfo(fileInfo, result));
                }
            }

            return results;
        }

        private static void GetRazorFiles(IFileProvider fileProvider, List<RelativeFileInfo> razorFiles, string root)
        {
            foreach (var fileInfo in fileProvider.GetDirectoryContents(root))
            {
                var relativePath = Path.Combine(root, fileInfo.Name);
                if (fileInfo.IsDirectory)
                {
                    GetRazorFiles(fileProvider, razorFiles, relativePath);
                }
                else if (fileInfo.Name.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
                {
                    razorFiles.Add(new RelativeFileInfo(fileInfo, relativePath));
                }
            }
        }

        private string ReadTypeInfo(CSharpCompilation compilation, SyntaxTree syntaxTree)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree, ignoreAccessibility: true);
            var classDeclarations = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();
            foreach (var declaration in classDeclarations)
            {
                var typeModel = semanticModel.GetDeclaredSymbol(declaration);
                if (typeModel.ContainingType == null && typeModel.DeclaredAccessibility == Accessibility.Public)
                {
                    return typeModel.ToDisplayString();
                }
            }

            return null;
        }
    }
}
