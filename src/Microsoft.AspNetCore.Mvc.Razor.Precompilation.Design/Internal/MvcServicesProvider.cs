// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc.DesignTime;
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
        private readonly string _projectPath;
        private readonly string _contentRoot;
        private readonly string _applicationName;

        public MvcServicesProvider(
            string projectPath,
            string outputFilePath,
            string contentRoot,
            string configureCompilationType)
        {
            _projectPath = projectPath;
            _contentRoot = contentRoot;
            _applicationName = Path.GetFileNameWithoutExtension(outputFilePath);

            var mvcBuilderConfiguration = GetConfigureCompilationAction(configureCompilationType);
            var serviceProvider = GetProvider(mvcBuilderConfiguration);

            Host = serviceProvider.GetRequiredService<IMvcRazorHost>();
            CompilationService = serviceProvider.GetRequiredService<ICompilationService>() as DefaultRoslynCompilationService;
            if (CompilationService == null)
            {
                throw new InvalidOperationException(
                    $"An {typeof(ICompilationService)} of type {typeof(DefaultRoslynCompilationService)} " +
                    "is required for Razor precompilation.");
            }

            FileProvider = serviceProvider.GetRequiredService<IRazorViewEngineFileProviderAccessor>().FileProvider;
        }

        public IMvcRazorHost Host { get; }

        public DefaultRoslynCompilationService CompilationService { get; }

        public IFileProvider FileProvider { get; }

        private IMvcBuilderConfiguration GetConfigureCompilationAction(string configureCompilationType)
        {
            Type type;
            if (!string.IsNullOrEmpty(configureCompilationType))
            {
                type = Type.GetType(configureCompilationType);
                if (type == null)
                {
                    throw new InvalidOperationException($"Unable to find type '{type}.");
                }
            }
            else
            {
                var assemblyName = new AssemblyName(_applicationName);
                var assembly = Assembly.Load(assemblyName);
                type = assembly
                    .GetExportedTypes()
                    .FirstOrDefault(typeof(IMvcBuilderConfiguration).IsAssignableFrom);
            }

            if (type == null)
            {
                return null;
            }

            var instance = Activator.CreateInstance(type) as IMvcBuilderConfiguration;
            if (instance == null)
            {
                throw new InvalidOperationException($"Type {configureCompilationType} does not implement " +
                    $"{typeof(IMvcBuilderConfiguration)}.");
            }

            return instance;
        }

        private IServiceProvider GetProvider(IMvcBuilderConfiguration mvcBuilderConfiguration)
        {
            var services = new ServiceCollection();

            var hostingEnvironment = new HostingEnvironment
            {
                ApplicationName = _applicationName,
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
            mvcBuilderConfiguration?.ConfigureMvc(mvcBuilder);

            return mvcBuilder.Services.BuildServiceProvider();
        }
    }
}
