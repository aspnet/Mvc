// Copyright (c) .NET  Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace Microsoft.AspNetCore.Mvc.Testing
{
    /// <summary>
    /// Fixture for bootstrapping an application in memory for functional end to end tests.
    /// </summary>
    /// <typeparam name="TTestClass">The test class type.</typeparam>
    /// <typeparam name="TStartup">The applications startup class.</typeparam>
    public class WebApplicationTestFixture<TTestClass, TStartup> : IDisposable where TTestClass : class where TStartup : class
    {
        private TestServer _server;
        private string _solutionRelativeContentRootPath;
        private HttpClient _client;
        private IWebHostBuilder _builder;
        private readonly string _solutionSearchPattern;

        /// <summary>
        /// <para>
        /// Creates a TestServer instance using the MVC application defined by<typeparamref name="TStartup"/>.
        /// The startup code defined in <typeparamref name = "TStartup" /> will be executed to configure the application.
        /// </para>
        /// <para>
        /// This constructor will infer the application root directive by searching for an <see cref="AssemblyMetadataAttribute"/>
        /// on the <typeparamref name="TTestClass"/> assembly with the key 
        /// Microsoft.AspNetCore.Testing.ContentRoot[<c><typeparamref name="TStartup" /> assembly file name</c>] and we will use
        /// the value of that attribute as the content root. In case we can't find an attribute with the right key, we fall back to
        /// searching for a solution file (*.sln) and then appending the path<c>{AssemblyName}</c> to the solution directory.
        /// The application root directory will be used to discover views and content files.
        /// </para>
        /// <para>
        /// The application assemblies will be loaded from the dependency context of the assembly containing
        /// <typeparamref name = "TStartup" />.This means that project dependencies of the assembly containing
        /// <typeparamref name = "TStartup" /> will be loaded as application assemblies.
        /// </para>
        /// </summary>
        public WebApplicationTestFixture()
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
        protected WebApplicationTestFixture(string solutionRelativePath, string solutionSearchPattern)
        {
            _solutionRelativeContentRootPath = solutionRelativePath;
            _solutionSearchPattern = solutionSearchPattern;
        }

        /// <summary>
        /// <para>
        /// Creates a TestServer instance using the MVC application defined by<typeparamref name="TStartup"/>.
        /// The startup code defined in <typeparamref name = "TStartup" /> will be executed to configure the application.
        /// </para>
        /// <para>
        /// This constructor will infer the application root directive by searching for an <see cref="AssemblyMetadataAttribute"/>
        /// on the <typeparamref name="TTestClass"/> assembly with the key 
        /// Microsoft.AspNetCore.Testing.ContentRoot[<c><typeparamref name="TStartup" /> assembly file name</c>] and we will use
        /// the value of that attribute as the content root. In case we can't find an attribute with the right key, we fall back to
        /// searching for a solution file (*.sln) and then appending the path<c>{AssemblyName}</c> to the solution directory.
        /// The application root directory will be used to discover views and content files.
        /// </para>
        /// <para>
        /// The application assemblies will be loaded from the dependency context of the assembly containing
        /// <typeparamref name = "TStartup" />.This means that project dependencies of the assembly containing
        /// <typeparamref name = "TStartup" /> will be loaded as application assemblies.
        /// </para>
        /// </summary>
        public static WebApplicationTestFixture<TTestClass, TStartup> Create() =>
            new WebApplicationTestFixture<TTestClass, TStartup>();

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
        public static WebApplicationTestFixture<TTestClass, TStartup> Create(string solutionRelativePath) =>
            new WebApplicationTestFixture<TTestClass, TStartup>(solutionRelativePath);

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
        public static WebApplicationTestFixture<TTestClass, TStartup> Create(
            string solutionRelativePath,
            string solutionSearchPattern) =>
            new WebApplicationTestFixture<TTestClass, TStartup>(solutionRelativePath, solutionSearchPattern);

        private void EnsureServer()
        {
            if (_server != null)
            {
                return;
            }

            EnsureBuilder();
            EnsureDepsFile();

            SetContentRoot();

            ConfigureWebHost(_builder);
            _server = CreateServer(_builder);
        }

        private void SetContentRoot()
        {
            if (_solutionRelativeContentRootPath != null)
            {
                if (_solutionSearchPattern == null)
                {
                    _builder.UseSolutionRelativeContentRoot(_solutionRelativeContentRootPath);
                }
                else
                {
                    _builder.UseSolutionRelativeContentRoot(_solutionRelativeContentRootPath, _solutionSearchPattern);
                }
            }
            else
            {
                var testAssembly = typeof(TTestClass).Assembly;
                var metadataAttributes = testAssembly.GetCustomAttributes<AssemblyMetadataAttribute>().ToArray();
                var startupAssembly = typeof(TStartup).Assembly;
                var assemblyFile = new Uri(startupAssembly.CodeBase).Segments.Last();
                var metadataKey = $"Microsoft.AspNetCore.Testing.ContentRoot[{assemblyFile}]";
                string contentRoot = null;
                for (var i = 0; i < metadataAttributes.Length; i++)
                {
                    var metadataAttribute = metadataAttributes[i];
                    if (string.Equals(metadataAttribute.Key, metadataKey, StringComparison.OrdinalIgnoreCase))
                    {
                        contentRoot = metadataAttribute.Value;
                    }
                }

                if (contentRoot != null)
                {
                    _builder.UseContentRoot(contentRoot);
                }
                else
                {
                    _builder.UseSolutionRelativeContentRoot(startupAssembly.GetName().Name);
                }
            }
        }

        private void EnsureDepsFile()
        {
            var depsFileName = $"{typeof(TStartup).Assembly.GetName().Name}.deps.json";
            var depsFile = new FileInfo(Path.Combine(AppContext.BaseDirectory, depsFileName));
            if (!depsFile.Exists)
            {
                throw new InvalidOperationException(Resources.FormatMissingDepsFile(
                    depsFile.FullName,
                    Path.GetFileName(depsFile.FullName)));
            }
        }

        /// <summary>
        /// Creates a <see cref="IWebHostBuilder"/> used to setup <see cref="TestServer"/>.
        /// <remarks>
        /// The default implementation of this method looks for a <c>public static IWebHostBuilder CreateDefaultBuilder(string[] args)</c>
        /// method defined on the entry point of the assembly of <typeparamref name="TStartup" /> and invokes it passing an empty string
        /// array as arguments. In case this method can't be found,
        /// </remarks>
        /// </summary>
        /// <returns>A <see cref="IWebHostBuilder"/> instance.</returns>
        protected virtual IWebHostBuilder CreateWebHostBuilder() =>
            WebHostBuilderFactory.CreateFromTypesAssemblyEntryPoint<TStartup>(Array.Empty<string>()) ?? new WebHostBuilder();

        /// <summary>
        /// Creates the <see cref="TestServer"/> with the bootstrapped application in <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IWebHostBuilder"/> used to
        /// create the server.</param>
        /// <returns>The <see cref="TestServer"/> with the bootstrapped application.</returns>
        protected virtual TestServer CreateServer(IWebHostBuilder builder) => new TestServer(builder);

        /// <summary>
        /// Gives a fixture an opportunity to configure the application before it gets built.
        /// </summary>
        /// <param name="builder">The <see cref="IWebHostBuilder"/> for the application.</param>
        protected virtual void ConfigureWebHost(IWebHostBuilder builder)
        {
        }

        /// <summary>
        /// Gives a fixture an opportunity to configure the application before it gets built.
        /// </summary>
        /// <param name="configure">The <see cref="Action{IWebHostBuilder}"/> to configure the <see cref="IWebHostBuilder"/>.</param>
        public void ConfigureWebHost(Action<IWebHostBuilder> configure)
        {
            if (_server != null)
            {
                throw new InvalidOperationException("The server was already built.");
            }

            EnsureBuilder();
            configure(_builder);
        }

        private void EnsureBuilder()
        {
            if (_builder != null)
            {
                return;
            }

            _builder = CreateWebHostBuilder();
            _builder.UseStartup<TStartup>();
        }

        /// <summary>
        /// Gets an instance of the <see cref="HttpClient"/> used to send <see cref="HttpRequestMessage"/> to the server.
        /// </summary>
        public HttpClient Client
        {
            get
            {
                EnsureServer();
                if (_client == null)
                {
                    _client = _server.CreateClient();
                }

                return _client;
            }
        }

        /// <summary>
        /// Creates a new instance of an <see cref="HttpClient"/> that can be used to
        /// send <see cref="HttpRequestMessage"/> to the server.
        /// </summary>
        /// <returns>The <see cref="HttpClient"/></returns>
        public HttpClient CreateClient()
        {
            EnsureServer();
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
            EnsureServer();
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
            if (Client != null)
            {
                Client.Dispose();
            }

            if (_server != null)
            {
                _server.Dispose();
            }
        }
    }
}
