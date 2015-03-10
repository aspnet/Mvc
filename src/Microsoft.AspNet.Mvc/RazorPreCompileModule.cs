// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Framework.Cache.Memory;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Runtime.Roslyn;

namespace Microsoft.AspNet.Mvc
{
    public abstract class RazorPreCompileModule : ICompileModule
    {
        private readonly IServiceProvider _appServices;
        private readonly IMemoryCache _memoryCache;

        public RazorPreCompileModule(IServiceProvider services)
        {
            _appServices = services;

            // When ListenForMemoryPressure is true, the MemoryCache evicts items at every gen2 collection.
            // In DTH, gen2 happens frequently enough to make it undesirable for caching precompilation results. We'll
            // disable listening for memory pressure for the MemoryCache instance used by precompilation.
            _memoryCache = new MemoryCache(new MemoryCacheOptions { ListenForMemoryPressure = false });
        }

        /// <summary>
        /// Gets or sets a value that determines if symbols (.pdb) file for the precompiled views is generated.
        /// </summary>
        public bool GenerateSymbols { get; protected set; }

        protected virtual string FileExtension { get; } = ".cshtml";

        public virtual void BeforeCompile(IBeforeCompileContext context)
        {
            var applicationEnvironment = _appServices.GetRequiredService<IApplicationEnvironment>();
            var compilerOptionsProvider = _appServices.GetRequiredService<ICompilerOptionsProvider>();
            var compilationSettings = compilerOptionsProvider.GetCompilationSettings(applicationEnvironment);

            // Create something similar to a HttpContext.RequestServices provider. Necessary because this class is
            // instantiated in a lower-level "HttpContext.ApplicationServices" context. Most important added service
            // is an IOptions<RazorViewEngineOptions> but use AddMvc() for simplicity and flexibility.
            var serviceCollection = HostingServices.Create(_appServices);
            serviceCollection.AddMvc();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var viewCompiler = new RazorPreCompiler(serviceProvider, context, _memoryCache, compilationSettings)
            {
                GenerateSymbols = GenerateSymbols
            };

            viewCompiler.CompileViews();
        }

        public void AfterCompile(IAfterCompileContext context)
        {
        }
    }
}
