// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.DependencyInjection.ServiceLookup;
using Microsoft.Framework.OptionsModel;
using Microsoft.Framework.Runtime;

namespace Microsoft.AspNet.Mvc
{
    public abstract class RazorPreCompileModule : ICompileModule
    {
        private readonly IServiceProvider _appServices;

        public RazorPreCompileModule(IServiceProvider services)
        {
            _appServices = services;
        }

        protected virtual string FileExtension { get; } = ".cshtml";

        public virtual void BeforeCompile(IBeforeCompileContext context)
        {
            var sc = new ServiceCollection();
            var appEnv = _appServices.GetRequiredService<IApplicationEnvironment>();

            var setup = new RazorViewEngineOptionsSetup(appEnv);
            var accessor = new OptionsManager<RazorViewEngineOptions>(new[] { setup });
            sc.AddInstance<IOptions<RazorViewEngineOptions>>(accessor);
            sc.Add(MvcServices.GetDefaultServices());

            var viewCompiler = new RazorPreCompiler(BuildFallbackServiceProvider(sc, _appServices));
            viewCompiler.CompileViews(context);
        }

        public void AfterCompile(IAfterCompileContext context)
        {
        }

        private class DelegatingServiceProvider : IServiceProvider
        {
            private readonly IServiceProvider _fallback;
            private readonly IServiceProvider _services;

            public DelegatingServiceProvider(IServiceProvider fallback, IServiceProvider services)
            {
                _fallback = fallback;
                _services = services;
            }

            public object GetService(Type serviceType)
            {
                return _services.GetService(serviceType) ?? _fallback.GetService(serviceType);
            }
        }

        public static IServiceProvider BuildFallbackServiceProvider(IEnumerable<IServiceDescriptor> services, IServiceProvider fallback)
        {
            var sc = new ServiceCollection();
            sc.Import(fallback);
            sc.Add(services);

            // Build the manifest
            var manifestTypes = services.Where(t => t.ServiceType.GetTypeInfo().GenericTypeParameters.Length == 0
                    && t.ServiceType != typeof(IServiceManifest)
                    && t.ServiceType != typeof(IServiceProvider))
                    .Select(t => t.ServiceType).Distinct();
            sc.AddInstance<IServiceManifest>(new ServiceManifest(manifestTypes, fallback.GetRequiredService<IServiceManifest>()));
            return new DelegatingServiceProvider(fallback, sc.BuildServiceProvider());
        }

        private class ServiceManifest : IServiceManifest
        {
            public ServiceManifest(IEnumerable<Type> services, IServiceManifest fallback = null)
            {
                Services = services;
                if (fallback != null)
                {
                    Services = Services.Concat(fallback.Services).Distinct();
                }
            }

            public IEnumerable<Type> Services { get; private set; }
        }
    }
}

namespace Microsoft.Framework.Runtime
{
    [AssemblyNeutral]
    public interface ICompileModule
    {
        void BeforeCompile(IBeforeCompileContext context);

        void AfterCompile(IAfterCompileContext context);
    }

    [AssemblyNeutral]
    public interface IAfterCompileContext
    {
        CSharpCompilation CSharpCompilation { get; set; }

        IList<Diagnostic> Diagnostics { get; }
    }
}