// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using Microsoft.Cci;
using Microsoft.Cci.MutableCodeModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.AspNetCore.Mvc.Razor.Precompilation.Design.Internal
{
    public class PrecompileRunCommand
    {
        private readonly string PrecompiledAssemblyNameSuffix = Guid.NewGuid().ToString();

        private CommandOption OutputFilePath { get; set; }

        private MvcServicesProvider ServicesProvider { get; set; }

        private CommonOptions Options { get; } = new CommonOptions();

        private string ProjectPath { get; set; }

        public void Configure(CommandLineApplication app)
        {
            Options.Configure(app);
            OutputFilePath = app.Option(
                "--output-file-path", 
                "Path to the output binary (assembly or executable).",
                CommandOptionType.SingleValue);

            app.OnExecute(() => Execute());
        }

        private int Execute()
        {
            ParseArguments();

            ServicesProvider = new MvcServicesProvider(
                ProjectPath,
                OutputFilePath.Value(),
                Options.ContentRootOption.Value(),
                Options.ConfigureCompilationType.Value());

            Console.WriteLine("Running Razor view precompilation.");

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

            Console.WriteLine($"Successfully compiled {result.CompileOutputs.Count} Razor views.");
            Console.WriteLine($"Injecting precompiled views into assembly {OutputFilePath.Value()}.");

            UpdateAssembly(result);
            return 0;
        }

        private void ParseArguments()
        {
            ProjectPath = Options.ProjectArgument.Value;
            if (string.IsNullOrEmpty(ProjectPath))
            {
                throw new ArgumentException("Project path not specified.");
            }

            if (!OutputFilePath.HasValue())
            {
                throw new ArgumentException($"Option {OutputFilePath.Template} does not specify a value.");
            }
        }

        private void UpdateAssembly(PrecompilationResult precompilationResult)
        {
            var host = new PeReader.DefaultHost();

            var outputFilePath = OutputFilePath.Value();
            var assembly = host.LoadUnitFrom(outputFilePath) as Cci.IAssembly;
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

            var saveFilePath = Path.ChangeExtension(outputFilePath, Constants.ModifiedAssemblyExtension);
            using (var stream = File.OpenWrite(saveFilePath))
            {
                var pdbPath = Path.ChangeExtension(outputFilePath, ".pdb");
                using (var pdb = new PdbReaderWriter(host, pdbPath))
                {
                    PeWriter.WritePeToStream(assembly, host, stream, pdb.PdbReader, localScopeProvider: null, pdbWriter: pdb.PdbWriter);
                }
            }
        }

        private PrecompilationResult CompileViews()
        {
            var files = new List<RelativeFileInfo>();
            GetRazorFiles(ServicesProvider.FileProvider, files, root: string.Empty);
            var precompilationResult = new PrecompilationResult();
            foreach (var fileInfo in files)
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
                Constants.PrecompiledAssemblyNamePrefix + Guid.NewGuid(),
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

        private class PdbReaderWriter : IDisposable
        {
            public PdbReaderWriter(IMetadataHost host, string pdbPath)
            {
                if (File.Exists(pdbPath))
                {
                    using (var pdbStream = File.OpenRead(pdbPath))
                    {
                        PdbReader = new PdbReader(pdbStream, host);
                    }

                    PdbWriter = new PdbWriter(pdbPath, PdbReader);
                }
            }

            public PdbReader PdbReader { get; }

            public PdbWriter PdbWriter { get; }

            public void Dispose()
            {
                PdbWriter?.Dispose();
                PdbReader?.Dispose();
            }
        }
    }
}
