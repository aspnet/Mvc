// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using Microsoft.Cci;
using Microsoft.Cci.MutableCodeModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.AspNetCore.Mvc.Razor.Precompilation.Design.Internal
{
    public class PrecompileRunCommand
    {
        private const string PrecompiledAssemblyPrefix = "PrecompiledRazor.";

        private CommandOption OutputFilePath { get; set; }

        private MvcServicesProvider ServicesProvider { get; set; }

        private CommonOptions Options { get; } = new CommonOptions();

        private string ProjectPath { get; set; }

        public void Configure(CommandLineApplication app)
        {
            Options.Configure(app);
            app.OnExecute(() => Execute());
        }

        private int Execute()
        {
            ParseArguments();

            ServicesProvider = new MvcServicesProvider(
                ProjectPath,
                Options.ContentRootOption.Value(),
                Options.ConfigureCompilationType.Value());

            var result = CompileViews();
            if (!result.Success)
            {
                foreach (var error in result.RazorErrors)
                {
                    Console.Error.WriteLine($"{error.Location.FilePath} ({error.Location.LineIndex}): {error.Message}");
                }

                foreach (var error in result.RoslynErrors)
                {
                    Console.Error.WriteLine(CSharpDiagnosticFormatter.Instance.Format(error));
                }

                return 1;
            }

            var outputPath = Options.OutputPathOption.Value();
            UpdateAssembly(outputPath, result);
            return 0;
        }

        private void ParseArguments()
        {
            ProjectPath = Options.ProjectArgument.Value;
            if (string.IsNullOrEmpty(ProjectPath))
            {
                throw new ArgumentException("Project path not specified.");
            }
        }

        private void UpdateAssembly(string outputPath, PrecompilationResult precompilationResult)
        {
            var host = new PeReader.DefaultHost();

            var assemblyPath = Path.Combine(outputPath, Path.GetFileName(ProjectPath) + ".dll");
            var assembly = host.LoadUnitFrom(assemblyPath) as Cci.IAssembly;
            assembly = new MetadataDeepCopier(host).Copy(assembly);

            var rewriters = new MetadataRewriter[]
            {
                new AddResourcesRewriter(host, precompilationResult.CompileOutputs),
                new RemoveStrongNameRewriter(host)
            };

            var updatedAssembly = assembly;
            foreach (var rewriter in rewriters)
            {
                updatedAssembly = rewriter.Rewrite(updatedAssembly);
            }

            var outputFilePath = Path.ChangeExtension(assemblyPath, Constants.ModifiedAssemblyExtension);
            using (var stream = File.OpenWrite(outputFilePath))
            {
                PeWriter.WritePeToStream(updatedAssembly, host, stream);
            }
        }

        private PrecompilationResult CompileViews()
        {
            var precompilationResult = new PrecompilationResult();
            foreach (var fileInfo in FileProviderUtilities.GetRazorFiles(ServicesProvider.FileProvider))
            {
                CompileView(precompilationResult, fileInfo);
            }

            return precompilationResult;
        }

        private void CompileView(PrecompilationResult result, RelativeFileInfo fileInfo)
        {
            GeneratorResults generatorResults;
            using (var fileStream = fileInfo.FileInfo.CreateReadStream())
            {
                generatorResults = ServicesProvider.Host.GenerateCode(fileInfo.RelativePath, fileStream);
            }

            if (!generatorResults.Success)
            {
                result.RazorErrors.AddRange(generatorResults.ParserErrors);
                return;
            }

            var compileOutputs = new CompileOutputs(fileInfo.RelativePath, Options.GeneratePdbOption.HasValue());
            var emitResult = ServicesProvider.CompilationService.EmitAssembly(
                PrecompiledAssemblyPrefix + Guid.NewGuid(),
                generatorResults.GeneratedCode,
                compileOutputs.AssemblyStream,
                compileOutputs.PdbStream);

            if (!emitResult.Success)
            {
                result.RoslynErrors.AddRange(emitResult.Diagnostics);
                compileOutputs.Dispose();
            }
            else
            {
                result.CompileOutputs.Add(compileOutputs);
            }
        }
    }
}
