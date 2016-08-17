﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.AspNetCore.Mvc.Razor.Precompilation.Design.Internal
{
    public class CommonOptions
    {
        public static readonly string ConfigureCompilationTypeTemplate = "--configure-compilation-type";
        public static readonly string ContentRootTemplate = "--content-root";

        public CommandArgument ProjectArgument { get; private set; }

        public CommandOption ConfigureCompilationType { get; private set; }

        public CommandOption ContentRootOption { get; private set; }

        public void Configure(CommandLineApplication app)
        {
            app.Description = "Precompiles an application.";
            app.HelpOption("-?|-h|--help");

            ProjectArgument = app.Argument(
                "project",
                "The path to the project (project folder or project.json) with precompilation.");

            ConfigureCompilationType = app.Option(
                ConfigureCompilationTypeTemplate,
                "Type with Configure method",
                CommandOptionType.SingleValue);

            ContentRootOption = app.Option(
                ContentRootTemplate,
                "The application's content root.",
                CommandOptionType.SingleValue);
        }
    }
}
