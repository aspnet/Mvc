// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    /// <summary>
    /// XUnit fixture for bootstrapping an application in memory for functional end to end tests.
    /// </summary>
    /// <typeparam name="TStartup">The applications startup class.</typeparam>
    public class WebApplicationTestFixture<TStartup> : IDisposable where TStartup : class
    {
        private readonly TestServer _server;

        public WebApplicationTestFixture()
            : this("src")
        {
        }

        public WebApplicationTestFixture(string solutionRelativePath)
            : this("*.sln", solutionRelativePath)
        {
        }

        public WebApplicationTestFixture(string solutionSearchPattern, string solutionRelativePath)
        {
            var startupAssembly = typeof(TStartup).GetTypeInfo().Assembly;

            // This step assumes project name = assembly name.
            var projectName = startupAssembly.GetName().Name;
            var projectPath = Path.Combine(solutionRelativePath, projectName);
            var builder = new MvcWebApplicationBuilder<TStartup>()
                .UseSolutionRelativeContentRoot(projectPath)
                .UseApplicationAssemblies()
                .UseCultureReplacer();

            _server = builder.Build();
            Client = _server.CreateClient();
            Client.BaseAddress = new Uri("http://localhost");
        }

        public HttpClient Client { get; }

        public HttpClient CreateClient()
        {
            var client = _server.CreateClient();
            client.BaseAddress = new Uri("http://localhost");

            return client;
        }

        public HttpClient CreateClient(Uri baseAddress, params DelegatingHandler[] handlers)
        {
            if (handlers.Length == 0)
            {
                var client = _server.CreateClient();
                client.BaseAddress = baseAddress;

                return client;
            }
            else
            {

                for (var i = handlers.Length - 1; i > 1; i++)
                {
                    handlers[i - 1].InnerHandler = handlers[i];
                }

                var serverHandler = _server.CreateHandler();
                handlers[handlers.Length - 1].InnerHandler = serverHandler;
                var client = new HttpClient(handlers[0]);
                client.BaseAddress = baseAddress;

                return client;
            }
        }

        public void Dispose()
        {
            Client.Dispose();
            _server.Dispose();
        }
    }
}
