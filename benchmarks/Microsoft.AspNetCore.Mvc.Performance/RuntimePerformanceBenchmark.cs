// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;

namespace Microsoft.AspNetCore.Mvc.Performance
{


    public class RuntimePerformanceBenchmark
    {
        private class NullLoggerFactory : ILoggerFactory, ILogger
        {
            void ILoggerFactory.AddProvider(ILoggerProvider provider) {}
            ILogger ILoggerFactory.CreateLogger(string categoryName) => this;
            void IDisposable.Dispose() {}
            IDisposable ILogger.BeginScope<TState>(TState state) => null;
            bool ILogger.IsEnabled(LogLevel logLevel) => false;
            void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) {}
        }


        private class BenchmarkHostingEnvironment : IHostingEnvironment
        {
            public BenchmarkHostingEnvironment()
            {
                ApplicationName = typeof(ViewAssemblyMarker).Assembly.FullName;
                WebRootFileProvider = new NullFileProvider();
                ContentRootFileProvider = new NullFileProvider();
                ContentRootPath = AppContext.BaseDirectory;
                WebRootPath = AppContext.BaseDirectory;
            }

            public string EnvironmentName { get; set; }
            public string ApplicationName { get; set; }
            public string WebRootPath { get; set; }
            public IFileProvider WebRootFileProvider { get; set; }
            public string ContentRootPath { get; set; }
            public IFileProvider ContentRootFileProvider { get; set; }
        }

        public string[] ViewPaths { get; } = new string[]
        {
            "~/Views/HelloWorld.cshtml"
        };

        [ParamsSource(nameof(ViewPaths))]
        public string ViewPath;

        protected IView View;

        private ServiceProvider _serviceProvider;
        private RouteData _routeData;
        private ActionDescriptor _actionDescriptor;

        // runs once for every Document value
        [GlobalSetup]
        public void GlobalSetup()
        {
            var loader = new RazorCompiledItemLoader();
            Console.WriteLine("Base Directory: " + AppContext.BaseDirectory);
            var viewsDll = Path.ChangeExtension(typeof(ViewAssemblyMarker).Assembly.Location, "Views.dll");
            var viewsAssembly = Assembly.Load(File.ReadAllBytes(viewsDll));
            var services = new ServiceCollection();
            var listener = new DiagnosticListener(GetType().Assembly.FullName);
            var partManager = new ApplicationPartManager();
            partManager.ApplicationParts.Add(CompiledRazorAssemblyApplicationPartFactory.GetDefaultApplicationParts(viewsAssembly).Single());
            var builder = services
                .AddSingleton<ILoggerFactory, NullLoggerFactory>()
                .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
                .AddSingleton<DiagnosticSource>(listener)
                .AddSingleton(listener)
                .AddSingleton<IHostingEnvironment, BenchmarkHostingEnvironment>()
                .AddSingleton<ApplicationPartManager>(partManager)
                .AddMvc();

            _serviceProvider = services.BuildServiceProvider();
            _routeData = new RouteData();
            _actionDescriptor = new ActionDescriptor();
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _serviceProvider.Dispose();
        }

        [Benchmark(Description = "Perf-testing running a razor view result")]
        public Task RunTest()
        {
            using (var requestScope = _serviceProvider.CreateScope())
            {
                var viewResult = new ViewResult
                {
                    ViewName = ViewPath,
                };
                var actionContext = new ActionContext(
                    new DefaultHttpContext()
                    {
                        RequestServices = requestScope.ServiceProvider
                    },
                    _routeData,
                    _actionDescriptor);
                return viewResult.ExecuteResultAsync(actionContext);
            }
        }
    }
}
