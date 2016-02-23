// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class MvcTestFixture<TStartup> : IDisposable
    {
        private readonly TestServer _server;

        public MvcTestFixture()
            : this("../../../Websites/")
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
                    .ConfigureServices(serviceCollection => InitializeServices(serviceCollection, relativePath))
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

        protected virtual void InitializeServices(IServiceCollection services, string relativePath)
        {
            // When an application executes in a regular context, the application base path points to the root
            // directory where the application is located, for example .../samples/MvcSample.Web. However, when
            // executing an application as part of a test, the ApplicationBasePath of the IApplicationEnvironment
            // points to the root folder of the test project.
            // To compensate, we need to calculate the correct project path and override the application
            // environment value so that components like the view engine work properly in the context of the test.
            var startupAssembly = typeof(TStartup).GetTypeInfo().Assembly;
            var applicationName = startupAssembly.GetName().Name;

            var applicationEnvironment = PlatformServices.Default.Application;
#if DNXCORE50 || DNX451
            var libraryManager = DnxPlatformServices.Default.LibraryManager;
            var library = libraryManager.GetLibrary(applicationName);
            var applicationRoot = Path.GetDirectoryName(library.Path);    
#else       
            var applicationRoot = Path.GetFullPath(Path.Combine(
               applicationEnvironment.ApplicationBasePath,
               relativePath,
               applicationName
            ));
#endif

            services.AddSingleton<IApplicationEnvironment>(
                new TestApplicationEnvironment(applicationEnvironment, applicationName, applicationRoot));

            // Inject a custom assembly provider. Overrides AddMvc() because that uses TryAdd().
            var assemblyProvider = new StaticAssemblyProvider();
            assemblyProvider.CandidateAssemblies.Add(startupAssembly);
            services.AddSingleton<IAssemblyProvider>(assemblyProvider);
        }
    }
}
