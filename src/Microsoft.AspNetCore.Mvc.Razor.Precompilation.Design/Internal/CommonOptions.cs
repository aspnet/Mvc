// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.AspNetCore.Mvc.Razor.Precompilation.Design.Internal
{
    public class CommonOptions
    {
        public CommandOption OutputPathOption { get; private set; }

        public CommandArgument ProjectArgument { get; private set; }

        public CommandOption ConfigureCompilationType { get; private set; }

        public CommandOption ContentRootOption { get; private set; }

        public CommandOption GeneratePdbOption { get; private set; }

        public void Configure(CommandLineApplication app)
        {
            app.Description = "Precompiles an application.";
            app.HelpOption("-?|-h|--help");

            ProjectArgument = app.Argument(
                "project",
                "The path to the project (project folder or project.json) with precompilation.");

            OutputPathOption = app.Option(
                "-o|--output",
                "The output (bin or publish) directory.",
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
        }
    }
}
