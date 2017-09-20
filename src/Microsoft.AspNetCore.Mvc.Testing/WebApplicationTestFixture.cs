// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.TestHost;

namespace Microsoft.AspNetCore.Mvc.Testing
{
    /// <summary>
    /// Fixture for bootstrapping an application in memory for functional end to end tests.
    /// </summary>
    /// <typeparam name="TStartup">The applications startup class.</typeparam>
    public class WebApplicationTestFixture<TStartup> : IDisposable where TStartup : class
    {
        private readonly TestServer _server;

        /// <summary>
        /// <para>
        /// Creates a TestServer instance using the MVC application defined by<typeparamref name="TStartup"/>.
        /// The startup code defined in <typeparamref name = "TStartup" /> will be executed to configure the application.
        /// </para>
        /// <para>
        /// This constructor will infer the application root directive by searching for a solution file (*.sln) and then
        /// appending the path<c> src/{AssemblyName}</c> to the solution directory.The application root directory will be
        /// used to discover views and content files.
        /// </para>
        /// <para>
        /// The application assemblies will be loaded from the dependency context of the assembly containing
        /// <typeparamref name = "TStartup" />.This means that project dependencies of the assembly containing
        /// <typeparamref name = "TStartup" /> will be loaded as application assemblies.
        /// </para>
        /// </summary>
        public WebApplicationTestFixture()
            : this(Path.Combine("src", typeof(TStartup).Assembly.GetName().Name))
        {
        }

        /// <summary>
        /// <para>
        /// Creates a TestServer instance using the MVC application defined by<typeparamref name="TStartup"/>.
        /// The startup code defined in <typeparamref name = "TStartup" /> will be executed to configure the application.
        /// </para>
        /// <para>
        /// This constructor will infer the application root directive by searching for a solution file (*.sln) and then
        /// appending the path <paramref name="solutionRelativePath"/> to the solution directory.The application root
        /// directory will be used to discover views and content files.
        /// </para>
        /// <para>
        /// The application assemblies will be loaded from the dependency context of the assembly containing
        /// <typeparamref name = "TStartup" />.This means that project dependencies of the assembly containing
        /// <typeparamref name = "TStartup" /> will be loaded as application assemblies.
        /// </para>
        /// </summary>
        /// <param name="solutionRelativePath">The path to the project folder relative to the solution file of your
        /// application. The folder of the first .sln file found traversing up the folder hierarchy from the test execution
        /// folder is considered as the base path.</param>
        protected WebApplicationTestFixture(string solutionRelativePath)
            : this("*.sln", solutionRelativePath)
        {
        }

        /// <summary>
        /// <para>
        /// Creates a TestServer instance using the MVC application defined by<typeparamref name="TStartup"/>.
        /// The startup code defined in <typeparamref name = "TStartup" /> will be executed to configure the application.
        /// </para>
        /// <para>
        /// This constructor will infer the application root directive by searching for a solution file that matches the pattern
        /// <paramref name="solutionSearchPattern"/> and then appending the path <paramref name="solutionRelativePath"/>
        /// to the solution directory.The application root directory will be used to discover views and content files.
        /// </para>
        /// <para>
        /// The application assemblies will be loaded from the dependency context of the assembly containing
        /// <typeparamref name = "TStartup" />.This means that project dependencies of the assembly containing
        /// <typeparamref name = "TStartup" /> will be loaded as application assemblies.
        /// </para>
        /// </summary>
        /// <param name="solutionSearchPattern">The glob pattern to use when searching for a solution file by
        /// traversing up the folder hierarchy from the test execution folder.</param>
        /// <param name="solutionRelativePath">The path to the project folder relative to the solution file of your
        /// application. The folder of the first sln file that matches the <paramref name="solutionSearchPattern"/>
        /// found traversing up the folder hierarchy from the test execution folder is considered as the base path.</param>
        protected WebApplicationTestFixture(string solutionSearchPattern, string solutionRelativePath)
        {
            var builder = new MvcWebApplicationBuilder<TStartup>()
                .UseSolutionRelativeContentRoot(solutionRelativePath)
                .UseApplicationAssemblies();

            ConfigureApplication(builder);
            _server = CreateServer(builder);

            Client = _server.CreateClient();
            Client.BaseAddress = new Uri("http://localhost");
        }

        /// <summary>
        /// Creates the <see cref="TestServer"/> with the bootstrapped application in <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="MvcWebApplicationBuilder{TStartup}"/> used to
        /// create the server.</param>
        /// <returns>The <see cref="TestServer"/> with the bootstrapped application.</returns>
        protected virtual TestServer CreateServer(MvcWebApplicationBuilder<TStartup> builder)
        {
            return builder.Build();
        }

        /// <summary>
        /// Gives a fixture an opportunity to configure the application before it gets built.
        /// </summary>
        /// <param name="builder">The <see cref="MvcWebApplicationBuilder{TStartup}"/> for the application.</param>
        protected virtual void ConfigureApplication(MvcWebApplicationBuilder<TStartup> builder)
        {
        }

        /// <summary>
        /// Gets an instance of the <see cref="HttpClient"/> used to send <see cref="HttpRequestMessage"/> to the server.
        /// </summary>
        public HttpClient Client { get; }

        /// <summary>
        /// Creates a new instance of an <see cref="HttpClient"/> that can be used to
        /// send <see cref="HttpRequestMessage"/> to the server.
        /// </summary>
        /// <returns>The <see cref="HttpClient"/></returns>
        public HttpClient CreateClient()
        {
            var client = _server.CreateClient();
            client.BaseAddress = new Uri("http://localhost");

            return client;
        }

        /// <summary>
        /// Creates a new instance of an <see cref="HttpClient"/> that can be used to
        /// send <see cref="HttpRequestMessage"/> to the server.
        /// </summary>
        /// <param name="baseAddress">The base address of the <see cref="HttpClient"/> instance.</param>
        /// <param name="handlers">A list of <see cref="DelegatingHandler"/> instances to setup on the
        /// <see cref="HttpClient"/>.</param>
        /// <returns>The <see cref="HttpClient"/>.</returns>
        public HttpClient CreateClient(Uri baseAddress, params DelegatingHandler[] handlers)
        {
            if (handlers == null || handlers.Length == 0)
            {
                var client = _server.CreateClient();
                client.BaseAddress = baseAddress;

                return client;
            }
            else
            {

                for (var i = handlers.Length - 1; i > 1; i--)
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

        /// <inheritdoc />
        public void Dispose()
        {
            Client.Dispose();
            _server.Dispose();
        }
    }
}
