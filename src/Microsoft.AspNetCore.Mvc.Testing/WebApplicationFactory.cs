// Copyright (c) .NET  Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyModel;

namespace Microsoft.AspNetCore.Mvc.Testing
{
    /// <summary>
    /// Factory for bootstrapping an application in memory for functional end to end tests.
    /// </summary>
    /// <typeparam name="TEntryPoint">A type in the entry point assembly of the application.
    /// Typically the Startup or Program classes can be used.</typeparam>
    public class WebApplicationFactory<TEntryPoint> : IDisposable where TEntryPoint : class
    {
        private TestServer _server;
        private IWebHostBuilder _builder;
        private IList<HttpClient> _clients = new List<HttpClient>();

        /// <summary>
        /// <para>
        /// Creates a TestServer instance using the MVC application defined by <typeparamref name="TEntryPoint"/>.
        /// The <see cref="WebApplicationFactory{TEntryPoint}"/> will find the entry point class of <typeparamref name="TEntryPoint"/>
        /// assembly and initialize the application by calling <c>IWebHostBuilder CreateWebHostBuilder(string [] args)</c>
        /// on <typeparamref name="TEntryPoint"/>.
        /// </para>
        /// <para>
        /// This constructor will infer the application content root path by searching for an
        /// <see cref="WebApplicationFactoryContentRootAttribute"/> on the assembly containing the functional tests with
        /// a key equal to the <typeparamref name="TEntryPoint"/> assembly <see cref="Assembly.FullName"/>.
        /// In case an attribute with the right key can't be found, <see cref="WebApplicationFactory{TEntryPoint}"/>
        /// will fall back to searching for a solution file (*.sln) and then appending <typeparamref name="TEntryPoint"/> asembly name
        /// to the solution directory. The application root directory will be used to discover views and content files.
        /// </para>
        /// <para>
        /// The application assemblies will be loaded from the dependency context of the assembly containing
        /// <typeparamref name = "TEntryPoint" />.This means that project dependencies of the assembly containing
        /// <typeparamref name = "TEntryPoint" /> will be loaded as application assemblies.
        /// </para>
        /// </summary>
        public WebApplicationFactory()
        {
        }

        /// <summary>
        /// The <see cref="IWebHostBuilder"/> used to create the <see cref="TestServer"/>.
        /// </summary>
        public IWebHostBuilder WebHostBuilder
        {
            get
            {
                if(_server != null)
                {
                    throw new InvalidOperationException($"The '{nameof(TestServer)}' instance was already built.");
                }
                if (_builder == null)
                {
                    EnsureBuilder();
                }

                return _builder;
            }
        }

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
            var metadataAttributes = GetContentRootMetadataAttributes(typeof(TEntryPoint).Assembly.FullName);
            string contentRoot = null;
            for (var i = 0; i < metadataAttributes.Length; i++)
            {
                var contentRootAttribute = metadataAttributes[i];
                var contentRootCandidate = Path.Combine(
                    AppContext.BaseDirectory,
                    contentRootAttribute.ContentRootPath);

                var contentRootMarker = Path.Combine(
                    contentRootCandidate,
                    Path.GetFileName(contentRootAttribute.ContentRootTest));

                if (File.Exists(contentRootMarker))
                {
                    contentRoot = contentRootCandidate;
                    break;
                }
            }

            if (contentRoot != null)
            {
                _builder.UseContentRoot(contentRoot);
            }
            else
            {
                _builder.UseSolutionRelativeContentRoot(typeof(TEntryPoint).Assembly.GetName().Name);
            }
        }

        private WebApplicationFactoryContentRootAttribute[] GetContentRootMetadataAttributes(string key)
        {
            var testAssembly = GetTestAssemblies();
            var metadataAttributes = testAssembly
                .SelectMany(a => a.GetCustomAttributes<WebApplicationFactoryContentRootAttribute>())
                .Where(a => string.Equals(a.Key, key, StringComparison.OrdinalIgnoreCase))
                .OrderBy(a => a.Priority)
                .ToArray();

            return metadataAttributes;
        }

        /// <summary>
        /// Gets the assemblies containing the functional tests where the
        /// <see cref="WebApplicationFactoryContentRootAttribute"/> define
        /// content root to use for the given <typeparamref name="TEntryPoint"/>.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<Assembly> GetTestAssemblies()
        {
            try
            {
                // The default dependency context will be populated in .net core applications.
                var context = DependencyContext.Default;
                if (context != null)
                {
                    // Find the list of projects
                    var projects = context.CompileLibraries.Where(l => l.Type == "project");

                    // Find the list of projects runtime information and their assembly names.
                    var runtimeProjectLibraries = context.RuntimeLibraries
                        .Where(r => projects.Any(p => p.Name == r.Name))
                        .ToDictionary(r => r, r => r.GetDefaultAssemblyNames(context).ToArray());

                    var entryPointAssemblyName = typeof(TEntryPoint).Assembly.GetName().Name;

                    // Find the project containing TEntryPoint
                    var entryPointRuntimeLibrary = runtimeProjectLibraries
                        .Single(rpl => rpl.Value.Any(a => string.Equals(a.Name, entryPointAssemblyName, StringComparison.Ordinal)));

                    // Find the list of projects referencing TEntryPoint.
                    var candidates = runtimeProjectLibraries
                        .Where(rpl => rpl.Key.Dependencies
                            .Any(d => string.Equals(d.Name, entryPointRuntimeLibrary.Key.Name, StringComparison.Ordinal)));

                    return candidates.SelectMany(rl => rl.Value).Select(Assembly.Load);
                }
                else
                {
                    // The app domain friendly name will be populated in full framework.
                    return new[] { Assembly.Load(AppDomain.CurrentDomain.FriendlyName) };
                }
            }
            catch (Exception)
            {
            }

            return Array.Empty<Assembly>();
        }

        private void EnsureDepsFile()
        {
            var depsFileName = $"{typeof(TEntryPoint).Assembly.GetName().Name}.deps.json";
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
        /// method defined on the entry point of the assembly of <typeparamref name="TEntryPoint" /> and invokes it passing an empty string
        /// array as arguments. In case this method can't be found,
        /// </remarks>
        /// </summary>
        /// <returns>A <see cref="IWebHostBuilder"/> instance.</returns>
        protected virtual IWebHostBuilder CreateWebHostBuilder() =>
            WebHostBuilderFactory.CreateFromTypesAssemblyEntryPoint<TEntryPoint>(Array.Empty<string>()) ??
            throw new InvalidOperationException($"No method 'public static {nameof(IWebHostBuilder)} CreateWebHostBuilder(string[] args)' " +
                $"found on '{typeof(TEntryPoint).Assembly.EntryPoint.DeclaringType.FullName}'. Alternatively, " +
                $"{typeof(WebApplicationFactory<TEntryPoint>).Name} can be extended and 'protected virtual {nameof(IWebHostBuilder)} " +
                $"{nameof(CreateWebHostBuilder)}()' can be overriden to provide your own {nameof(IWebHostBuilder)} instance.");

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

        private void EnsureBuilder()
        {
            if (_builder != null)
            {
                return;
            }

            _builder = CreateWebHostBuilder();
        }

        /// <summary>
        /// Creates a new instance of an <see cref="HttpClient"/> that can be used to
        /// send <see cref="HttpRequestMessage"/> to the server.
        /// </summary>
        /// <returns>The <see cref="HttpClient"/></returns>
        public HttpClient CreateClient(params DelegatingHandler[] handlers)
        {
            return CreateClient(new Uri("http://localhost"),Array.Empty<DelegatingHandler>());
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

                var client = new HttpClient(handlers[0])
                {
                    BaseAddress = baseAddress
                };

                _clients.Add(client);

                return client;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (var client in _clients)
            {
                client.Dispose();
            }

            _server?.Dispose();
        }
    }
}
