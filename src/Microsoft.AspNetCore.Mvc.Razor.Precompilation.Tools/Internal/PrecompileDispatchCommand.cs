// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Razor.Precompilation.Design.Internal;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.InternalAbstractions;
using Microsoft.DotNet.ProjectModel;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Internal;
using NuGet.Frameworks;

namespace Microsoft.AspNetCore.Mvc.Razor.Precompilation.Internal
{
    public class PrecompileDispatchCommand
    {
        private CommonOptions Options { get; } = new CommonOptions();

        private CommandOption NoBuildOption { get; set; }

        private CommandOption FrameworkOption { get; set; }

        private CommandOption ConfigurationOption { get; set; }

        private CommandOption OutputPathOption { get; set; }

        private NuGetFramework TargetFramework { get; set; }

        private CommandOption BuildBasePathOption { get; set; }

        private string OutputPath { get; set; }

        private string ProjectPath { get; set; }

        private string Configuration { get; set; }
        
        public void Configure(CommandLineApplication app)
        {
            Options.Configure(app);
            FrameworkOption = app.Option(
                "-f|--framework",
                "Target Framework",
                CommandOptionType.SingleValue);

            ConfigurationOption = app.Option(
                "-c|--configuration",
                "Configuration",
                CommandOptionType.SingleValue);

            NoBuildOption = app.Option(
                "--no-build",
                "Do not build project before compiling views.",
                CommandOptionType.NoValue);

            OutputPathOption = app.Option(
                "-o|--output",
                "The output (bin or publish) directory.",
                CommandOptionType.SingleValue);

            BuildBasePathOption = app.Option(
                "-b|--build-base-path",
                "Directory in which to place outputs.",
                CommandOptionType.SingleValue);

            app.OnExecute(() => Execute());
        }

        private int Execute()
        {
            ProjectPath = GetProjectPath();
            Configuration = ConfigurationOption.Value() ?? DotNet.Cli.Utils.Constants.DefaultConfiguration;
            TargetFramework = GetTargetFramework();
            var runtimeIdentifiers = RuntimeEnvironmentRidExtensions.GetAllCandidateRuntimeIdentifiers();
            var outputPaths = GetOutputPaths(runtimeIdentifiers);
            OutputPath = runtimeIdentifiers.Any() ? outputPaths.RuntimeOutputPath : outputPaths.CompilationOutputPath;
            var outputBinary = outputPaths.RuntimeFiles.Assembly;

            if (!NoBuildOption.HasValue())
            {
                var exitCode = BuildProject();
                if (exitCode != 0)
                {
                    return exitCode;
                }
            }

            var dispatchArgs = new List<string>
            {
                ProjectPath,
                "--output-file-path",
                outputBinary,
                "--content-root",
                Options.ContentRootOption.Value() ?? Directory.GetCurrentDirectory(),
            };

            if (Options.ConfigureCompilationType.HasValue())
            {
                dispatchArgs.Add("--configure-compilation-type");
                dispatchArgs.Add(Options.ConfigureCompilationType.Value());
            }

            if (Options.GeneratePdbOption.HasValue())
            {
                dispatchArgs.Add("--generate-pdbs");
            }

            var toolName = typeof(Design.Program).GetTypeInfo().Assembly.GetName().Name;
            var dispatchCommand = DotnetToolDispatcher.CreateDispatchCommand(
                dispatchArgs,
                TargetFramework,
                Configuration,
                outputPath: OutputPath,
                buildBasePath: BuildBasePathOption.Value(),
                projectDirectory: ProjectPath,
                toolName: toolName);

            var commandExitCode = dispatchCommand
                .ForwardStdErr(Console.Error)
                .ForwardStdOut(Console.Out)
                .Execute()
                .ExitCode;

            if (commandExitCode == 0)
            {
                var modifiedAssembly = Path.ChangeExtension(outputBinary, Design.Internal.Constants.ModifiedAssemblyExtension);

                if (File.Exists(outputBinary))
                {
                    File.Copy(modifiedAssembly, outputBinary, overwrite: true);
                    File.Delete(modifiedAssembly);
                }
            }

            return commandExitCode;
        }

        private string GetProjectPath()
        {
            string projectPath;
            if (!string.IsNullOrEmpty(Options.ProjectArgument.Value))
            {
                projectPath = Path.GetFullPath(Options.ProjectArgument.Value);
                if (string.Equals(Path.GetFileName(ProjectPath), "project.json", StringComparison.OrdinalIgnoreCase))
                {
                    projectPath = Path.GetDirectoryName(ProjectPath);
                }

                if (!Directory.Exists(projectPath))
                {
                    throw new InvalidOperationException($"Error: Could not find directory {projectPath}.");
                }
            }
            else
            {
                projectPath = Directory.GetCurrentDirectory();
            }

            return projectPath;
        }

        private OutputPaths GetOutputPaths(IEnumerable<string> runtimeIdentifiers)
        {
            var workspace = new BuildWorkspace(ProjectReaderSettings.ReadFromEnvironment());

            var projectContext = workspace.GetProjectContext(ProjectPath, TargetFramework);
            if (projectContext == null)
            {
                Debug.Assert(FrameworkOption.HasValue());
                throw new InvalidOperationException($"Project '{ProjectPath}' does not support framework: {FrameworkOption.Value()}");
            }

            var runtimeContext = workspace.GetRuntimeContext(projectContext, runtimeIdentifiers);
            return runtimeContext.GetOutputPaths(
                Configuration,
                BuildBasePathOption.Value(),
                OutputPathOption.Value());
        }

        private int BuildProject()
        {
            var arguments = new List<string>
            {
                ProjectPath,
                "--framework",
                TargetFramework.ToString(),
                "--configuration",
                Configuration,
            };

            if (BuildBasePathOption.HasValue())
            {
                arguments.Add("--build-base-path");
                arguments.Add(BuildBasePathOption.Value());
            }

            if (OutputPathOption.HasValue())
            {
                arguments.Add("--output");
                arguments.Add(OutputPathOption.Value());
            }

            return Command.CreateDotNet("build", arguments, TargetFramework, Configuration)
                .ForwardStdErr(Console.Error)
                .ForwardStdOut(Console.Out)
                .Execute()
                .ExitCode;
        }

        private NuGetFramework GetTargetFramework()
        {
            if (!FrameworkOption.HasValue())
            {
                var workspace = new BuildWorkspace(ProjectReaderSettings.ReadFromEnvironment());
                var projectContexts = workspace.GetProjectContextCollection(ProjectPath)
                    .FrameworkOnlyContexts
                    .ToList();

                if (projectContexts.Count == 0)
                {
                    throw new Exception($"Error: Project at {ProjectPath} does not have any specified frameworks.");

                }
                else if (projectContexts.Count > 1)
                {
                    throw new Exception($"Error: Project at {ProjectPath} supports multiple framework. " +
                        $"Specify a framework using {FrameworkOption.Template}.");
                }

                return projectContexts[0].TargetFramework;
            }
            else
            {
                return NuGetFramework.Parse(FrameworkOption.Value());
            }
        }
    }
}
