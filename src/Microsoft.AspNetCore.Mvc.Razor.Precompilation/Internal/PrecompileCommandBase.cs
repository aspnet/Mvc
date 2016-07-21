// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using Microsoft.DotNet.InternalAbstractions;
using Microsoft.DotNet.ProjectModel;
using Microsoft.Extensions.CommandLineUtils;
using NuGet.Frameworks;

namespace Microsoft.AspNetCore.Mvc.Razor.Precompilation.Internal
{
    public abstract class PrecompileCommandBase
    {
        protected static readonly string ModifiedAssemblyExtension = ".precompiled-razor";

        protected CommandOption OutputPathOption { get; set; }

        protected CommandArgument ProjectArgument { get; set; }

        protected CommandOption FrameworkOption { get; set; }

        protected CommandOption ConfigurationOption { get; set; }

        protected CommandOption ConfigureCompilationType { get; set; }

        protected CommandOption ContentRootOption { get; set; }

        protected CommandOption GeneratePdbOption { get; set; }

        protected string ProjectPath { get; private set; }

        protected NuGetFramework TargetFramework { get; private set; }

        protected string Configuration { get; private set; }

        public static void Register<TPrecompileCommandBase>(CommandLineApplication app) where
            TPrecompileCommandBase : PrecompileCommandBase, new()
        {
            var command = new TPrecompileCommandBase();
            command.Configure(app);
        }

        protected virtual void Configure(CommandLineApplication app)
        {
            app.Description = "Precompiles an application.";
            app.HelpOption("-?|-h|--help");
            ProjectArgument = app.Argument(
                "project",
                "The path to the project (project folder or project.json) with precompilation.");
            FrameworkOption = app.Option(
                "-f|--framework",
                "Target Framework",
                CommandOptionType.SingleValue);
            OutputPathOption = app.Option(
                "-o|--output",
                "The output (bin or publish) directory.",
                CommandOptionType.SingleValue);

            ConfigurationOption = app.Option(
                "-c|--configuration",
                "Configuration",
                CommandOptionType.SingleValue);

            ConfigureCompilationType = app.Option(
                "--configure-compilation-type",
                "Type with Configure method",
                CommandOptionType.SingleValue);

            ContentRootOption = app.Option(
                "--content-root",
                "The application's content root.",
                CommandOptionType.SingleValue);

            GeneratePdbOption = app.Option(
                "--generate-pdbs",
                "Generate pdbs for views.",
                CommandOptionType.NoValue);

            app.OnExecute((Func<int>)Execute);
        }

        protected int Execute()
        {
            if (!ParseArguments())
            {
                return 1;
            }

            return ExecuteCore();
        }

        protected abstract int ExecuteCore();

        private bool ParseArguments()
        {
            if (!string.IsNullOrEmpty(ProjectArgument.Value))
            {
                ProjectPath = Path.GetFullPath(ProjectArgument.Value);
                if (string.Equals(Path.GetFileName(ProjectPath), "project.json", StringComparison.OrdinalIgnoreCase))
                {
                    ProjectPath = Path.GetDirectoryName(ProjectPath);
                }

                if (!Directory.Exists(ProjectPath))
                {
                    Console.Error.WriteLine($"Error: Could not find directory {ProjectPath}");
                    return false;
                }
            }
            else
            {
                ProjectPath = Directory.GetCurrentDirectory();
            }

            if (!FrameworkOption.HasValue())
            {
                var workspace = new BuildWorkspace(ProjectReaderSettings.ReadFromEnvironment());
                var projectContexts = workspace.GetProjectContextCollection(ProjectPath)
                    .FrameworkOnlyContexts
                    .ToList();

                if (projectContexts.Count == 0)
                {
                    Console.Error.WriteLine($"Error: Project at {ProjectPath} does not have any specified frameworks.");
                    return false;
                }
                else if (projectContexts.Count > 1)
                {
                    Console.Error.WriteLine($"Error: Project at {ProjectPath} supports multiple framework. " +
                        $"Specify a framework using {FrameworkOption.Template}.");
                    return false;
                }

                TargetFramework = projectContexts[0].TargetFramework;
            }
            else
            {
                TargetFramework = NuGetFramework.Parse(FrameworkOption.Value());
            }

            if (ConfigurationOption.HasValue())
            {
                Configuration = ConfigurationOption.Value();
            }
            else
            {
                Configuration = "Debug";
            }

            return true;
        }

        protected string GetBuildOutputPath()
        {
            if (OutputPathOption.HasValue())
            {
                return OutputPathOption.Value();
            }

            var workspace = new BuildWorkspace(ProjectReaderSettings.ReadFromEnvironment());
            var projectContext = workspace.GetProjectContext(
                ProjectPath,
                TargetFramework);
            var runtimeContext = workspace.GetRuntimeContext(
                projectContext,
                RuntimeEnvironmentRidExtensions.GetAllCandidateRuntimeIdentifiers());

            var outputs = runtimeContext.GetOutputPaths(Configuration);
            string outputPath;
            if (!string.IsNullOrEmpty(runtimeContext.RuntimeIdentifier))
            {
                outputPath = outputs.RuntimeOutputPath;
            }
            else
            {
                outputPath = outputs.CompilationOutputPath;
            }

            return outputPath;
        }
    }
}
