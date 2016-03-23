// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Testing;
#if DNX451
using Microsoft.Extensions.CompilationAbstractions;
#endif
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class MvcTestFixture<TStartup> : IDisposable
    {
        private readonly TestServer _server;

        public MvcTestFixture()
            : this(Path.Combine("..", "..", "..", "..", "..", "WebSites"))
        {
        }

        protected MvcTestFixture(string relativePath)
        {
            // RequestLocalizationOptions saves the current culture when constructed, potentially changing response
            // localization i.e. RequestLocalizationMiddleware behavior. Ensure the saved culture
            // (DefaultRequestCulture) is consistent regardless of system configuration or personal preferences.
            using (new CultureReplacer())
            {
                var builder = new WebHostBuilder()
                    .UseContentRoot(GetApplicationPath(relativePath))
                    .ConfigureServices(InitializeServices)
                    .UseStartup(typeof(TStartup));

                _server = new TestServer(builder);
            }

            Client = _server.CreateClient();
            Client.BaseAddress = new Uri("http://localhost");
        }

        public HttpClient Client { get; }

        public void Dispose()
        {
            Client.Dispose();
            _server.Dispose();
        }

        private static string GetApplicationPath(string relativePath)
        {
            var startupAssembly = typeof(TStartup).GetTypeInfo().Assembly;
            var applicationName = startupAssembly.GetName().Name;
#if DNX451
            var libraryManager = DnxPlatformServices.Default.LibraryManager;
            var library = libraryManager.GetLibrary(applicationName);
            return Path.GetDirectoryName(library.Path);
#else
            var applicationBasePath = PlatformServices.Default.Application.ApplicationBasePath;
            return Path.GetFullPath(Path.Combine(applicationBasePath, relativePath, applicationName));
#endif
        }

        protected virtual void InitializeServices(IServiceCollection services)
        {
            var startupAssembly = typeof(TStartup).GetTypeInfo().Assembly;
            var applicationName = startupAssembly.GetName().Name;

            var applicationEnvironment = PlatformServices.Default.Application;
#if DNX451
            services.AddSingleton(CompilationServices.Default.LibraryExporter);
            services.AddSingleton<ICompilationService, DnxRoslynCompilationService>();
#endif

            // Inject a custom assembly provider. Overrides AddMvc() because that uses TryAdd().
            var assemblyProvider = new StaticAssemblyProvider();
            assemblyProvider.CandidateAssemblies.Add(startupAssembly);
            services.AddSingleton<IAssemblyProvider>(assemblyProvider);

            var collection = new ApplicationPartCollection();
            collection.Register(startupAssembly);
            services.AddSingleton(collection);
        }

        private class StaticAssemblyProvider : IAssemblyProvider
        {
            public IList<Assembly> CandidateAssemblies { get; } = new List<Assembly>();

            IEnumerable<Assembly> IAssemblyProvider.CandidateAssemblies => CandidateAssemblies;
        }
    }
}
