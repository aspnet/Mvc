// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.ObjectPool;

namespace Microsoft.AspNetCore.Mvc.Razor.Precompilation.Design.Internal
{
    public class MvcServicesProvider
    {
        private const string ConfigureMvcMethod = "ConfigureMvc";
        private readonly string _projectPath;
        private readonly string _contentRoot;

        public MvcServicesProvider(
            string projectPath,
            string contentRoot,
            string configureCompilationType)
        {
            _projectPath = projectPath;
            _contentRoot = contentRoot;

            var configureCompilationAction = GetConfigureCompilationAction(configureCompilationType);
            var serviceProvider = GetProvider(configureCompilationAction);

            Host = serviceProvider.GetRequiredService<IMvcRazorHost>();
            CompilationService = serviceProvider.GetRequiredService<ICompilationService>() as IRoslynCompilationService;
            if (CompilationService == null)
            {
                throw new InvalidOperationException(
                    $"An {typeof(ICompilationService)} of type {typeof(IRoslynCompilationService)} " +
                    "is required for Razor precompilation.");
            }

            FileProvider = serviceProvider.GetRequiredService<IRazorViewEngineFileProviderAccessor>().FileProvider;
        }

        public IMvcRazorHost Host { get; }

        public IRoslynCompilationService CompilationService { get; }

        public IFileProvider FileProvider { get; }

        private static Action<IMvcBuilder> GetConfigureCompilationAction(string configureCompilationType)
        {
            if (!string.IsNullOrEmpty(configureCompilationType))
            {
                var type = Type.GetType(configureCompilationType);
                if (type == null)
                {
                    throw new InvalidOperationException($"Unable to find type '{type}.");
                }

                var configureMethod = type.GetMethod(ConfigureMvcMethod, BindingFlags.Public | BindingFlags.Static);
                if (configureMethod == null)
                {
                    throw new InvalidOperationException($"Could not find a method named {ConfigureMvcMethod} on {type}.");
                }

                return (Action<IMvcBuilder>)configureMethod.CreateDelegate(
                    typeof(Action<IMvcBuilder>),
                    target: null);
            }

            // Todo: Add support for assembly scanning.
            return null;
        }

        private IServiceProvider GetProvider(Action<IMvcBuilder> configureBuilder)
        {
            var services = new ServiceCollection();
            var applicationName = Path.GetFileName(_projectPath.TrimEnd(Path.DirectorySeparatorChar));
            var hostingEnvironment = new HostingEnvironment
            {
                ApplicationName = applicationName,
                WebRootFileProvider = new PhysicalFileProvider(_projectPath),
                ContentRootFileProvider = new PhysicalFileProvider(_contentRoot),
                ContentRootPath = _contentRoot,
            };
            var diagnosticSource = new DiagnosticListener("Microsoft.AspNetCore");

            services
                .AddSingleton<IHostingEnvironment>(hostingEnvironment)
                .AddSingleton<DiagnosticSource>(diagnosticSource)
                .AddLogging()
                .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

            var mvcCoreBuilder = services
                .AddMvcCore()
                .AddRazorViewEngine();

            var mvcBuilder = new MvcBuilder(mvcCoreBuilder.Services, mvcCoreBuilder.PartManager);
            configureBuilder?.Invoke(mvcBuilder);

            return mvcBuilder.Services.BuildServiceProvider();
        }
    }
}
