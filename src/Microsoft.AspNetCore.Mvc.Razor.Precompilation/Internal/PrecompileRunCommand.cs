// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using Microsoft.Cci;
using Microsoft.Cci.MutableCodeModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.ObjectPool;

namespace Microsoft.AspNetCore.Mvc.Razor.Precompilation.Internal
{
    public class PrecompileRunCommand : PrecompileCommandBase
    {
        private const string ConfigureMvcMethod = "ConfigureMvc";
        private const string PrecompiledAssemblyPrefix = "PrecompiledRazor.";

        private IMvcRazorHost Host { get; set; }

        private IRoslynCompilationService CompilationService { get; set; }

        private Action<IMvcBuilder> ConfigureMvcAction { get; set; }

        protected override int ExecuteCore()
        {
            ParseArguments();

            var services = ConfigureDefaultServices();
            Host = services.GetRequiredService<IMvcRazorHost>();
            var fileProvider = services.GetRequiredService<IRazorViewEngineFileProviderAccessor>().FileProvider;
            CompilationService = services.GetRequiredService<ICompilationService>() as IRoslynCompilationService;
            if (CompilationService == null)
            {
                Console.Error.WriteLine(
                    $"An {typeof(ICompilationService)} of type {typeof(IRoslynCompilationService)} " +
                    "is required for Razor precompilation.");
            }

            var result = CompileViews(fileProvider);
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

            var outputPath = GetBuildOutputPath();
            UpdateAssembly(outputPath, result);
            return 0;
        }

        private bool ParseArguments()
        {
            if (ConfigureCompilationType.HasValue())
            {
                var type = Type.GetType(ConfigureCompilationType.Value());
                if (type == null)
                {
                    Console.Error.WriteLine($"Unable to find type '{type}.");
                    return false;
                }

                var configureMethod = type?.GetMethod(ConfigureMvcMethod, BindingFlags.Public | BindingFlags.Static);
                if (configureMethod == null)
                {
                    Console.Error.WriteLine($"Could not find a method named {ConfigureMvcMethod} on {type}.");
                    return false;
                }

                ConfigureMvcAction = (Action<IMvcBuilder>)configureMethod.CreateDelegate(
                    typeof(Action<IMvcBuilder>),
                    target: null);
            }

            return true;
        }

        private void UpdateAssembly(string outputPath, PrecompilationResult precompilationResult)
        {
            var host = new PeReader.DefaultHost();

            var assemblyPath = Path.Combine(outputPath, Path.GetFileName(ProjectPath) + ".dll");
            var assembly = host.LoadUnitFrom(assemblyPath) as IAssembly;
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

            using (var stream = File.OpenWrite(Path.ChangeExtension(assemblyPath, ModifiedAssemblyExtension)))
            {
                PeWriter.WritePeToStream(updatedAssembly, host, stream);
            }
        }

        private PrecompilationResult CompileViews(IFileProvider fileProvider)
        {
            var precompilationResult = new PrecompilationResult();
            foreach (var fileInfo in FileProviderUtilities.GetRazorFiles(fileProvider))
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
                generatorResults = Host.GenerateCode(fileInfo.RelativePath, fileStream);
            }

            if (!generatorResults.Success)
            {
                result.RazorErrors.AddRange(generatorResults.ParserErrors);
                return;
            }

            var compileOutputs = new CompileOutputs(fileInfo.RelativePath, GeneratePdbOption.HasValue());
            var emitResult = CompilationService.EmitAssembly(
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

        private IServiceProvider ConfigureDefaultServices()
        {
            var services = new ServiceCollection();

            var applicationName = Path.GetFileName(ProjectPath.TrimEnd('/'));
            var contentRoot = ContentRootOption.HasValue()
                ? ContentRootOption.Value()
                : Directory.GetCurrentDirectory();
            var hostingEnvironment = new HostingEnvironment
            {
                ApplicationName = applicationName,
                WebRootFileProvider = new PhysicalFileProvider(ProjectPath),
                ContentRootFileProvider = new PhysicalFileProvider(contentRoot),
                ContentRootPath = contentRoot,
            };
            var diagnosticSource = new DiagnosticListener("Microsoft.AspNetCore");

            services
                .AddSingleton<IHostingEnvironment>(hostingEnvironment)
                .AddSingleton<DiagnosticSource>(diagnosticSource)
                .AddLogging()
                .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

            var mvcBuilder = services.AddMvc();
            ConfigureMvcAction?.Invoke(mvcBuilder);

            return services.BuildServiceProvider();
        }
    }
}
