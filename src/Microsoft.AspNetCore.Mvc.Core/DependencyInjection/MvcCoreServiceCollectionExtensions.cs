// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up essential MVC services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class MvcCoreServiceCollectionExtensions
    {
        /// <summary>
        /// Adds essential MVC services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>An <see cref="IMvcCoreBuilder"/> that can be used to further configure the MVC services.</returns>
        public static IMvcCoreBuilder AddMvcCore(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var partManager = GetApplicationPartManager(services);
            services.TryAddSingleton(partManager);

            ConfigureDefaultFeatureProviders(partManager);
            ConfigureDefaultServices(services);
            AddMvcCoreServices(services);

            var builder = new MvcCoreBuilder(services, partManager);

            return builder;
        }

        private static void ConfigureDefaultFeatureProviders(ApplicationPartManager manager)
        {
            if (!manager.FeatureProviders.OfType<ControllerFeatureProvider>().Any())
            {
                manager.FeatureProviders.Add(new ControllerFeatureProvider());
            }
        }

        private static ApplicationPartManager GetApplicationPartManager(IServiceCollection services)
        {
            var manager = GetServiceFromCollection<ApplicationPartManager>(services);
            if (manager == null)
            {
                manager = new ApplicationPartManager();

                var environment = GetServiceFromCollection<IHostingEnvironment>(services);
                if (string.IsNullOrEmpty(environment?.ApplicationName))
                {
                    return manager;
                }

                var parts = DefaultAssemblyPartDiscoveryProvider.DiscoverAssemblyParts(environment.ApplicationName);
                foreach (var part in parts)
                {
                    manager.ApplicationParts.Add(part);
                }
            }

            return manager;
        }

        private static T GetServiceFromCollection<T>(IServiceCollection services)
        {
            return (T)services
                .LastOrDefault(d => d.ServiceType == typeof(T))
                ?.ImplementationInstance;
        }

        /// <summary>
        /// Adds essential MVC services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="setupAction">An <see cref="Action{MvcOptions}"/> to configure the provided <see cref="MvcOptions"/>.</param>
        /// <returns>An <see cref="IMvcCoreBuilder"/> that can be used to further configure the MVC services.</returns>
        public static IMvcCoreBuilder AddMvcCore(
            this IServiceCollection services,
            Action<MvcOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            var builder = services.AddMvcCore();
            services.Configure(setupAction);

            return builder;
        }

        // To enable unit testing
        internal static void AddMvcCoreServices(IServiceCollection services)
        {
            //
            // Options
            //
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, MvcCoreMvcOptionsSetup>());
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<RouteOptions>, MvcCoreRouteOptionsSetup>());

            //
            // Action Discovery
            //
            // These are consumed only when creating action descriptors, then they can be deallocated

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IApplicationModelProvider, DefaultApplicationModelProvider>());
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IActionDescriptorProvider, ControllerActionDescriptorProvider>());
            services.TryAddSingleton<IActionDescriptorCollectionProvider, ActionDescriptorCollectionProvider>();

            //
            // Action Selection
            //
            services.TryAddSingleton<IActionSelector, ActionSelector>();
            services.TryAddSingleton<ActionConstraintCache>();

            // Will be cached by the DefaultActionSelector
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IActionConstraintProvider, DefaultActionConstraintProvider>());

            //
            // Controller Factory
            //
            // This has a cache, so it needs to be a singleton
            services.TryAddSingleton<IControllerFactory, DefaultControllerFactory>();

            // Will be cached by the DefaultControllerFactory
            services.TryAddTransient<IControllerActivator, DefaultControllerActivator>();

            services.TryAddSingleton<IControllerFactoryProvider, ControllerFactoryProvider>();
            services.TryAddSingleton<IControllerActivatorProvider, ControllerActivatorProvider>();
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IControllerPropertyActivator, DefaultControllerPropertyActivator>());

            //
            // Action Invoker
            //
            // The IActionInvokerFactory is cachable
            services.TryAddSingleton<IActionInvokerFactory, ActionInvokerFactory>();
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IActionInvokerProvider, ControllerActionInvokerProvider>());

            // These are stateless
            services.TryAddSingleton<ControllerActionInvokerCache>();
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IFilterProvider, DefaultFilterProvider>());

            //
            // Resource Filters
            //
            services.TryAddTransient<RequestSizeLimitResourceFilter>();
            services.TryAddTransient<DisableRequestSizeLimitResourceFilter>();

            //
            // ModelBinding, Validation
            //
            // The DefaultModelMetadataProvider does significant caching and should be a singleton.
            services.TryAddSingleton<IModelMetadataProvider, DefaultModelMetadataProvider>();
            services.TryAdd(ServiceDescriptor.Transient<ICompositeMetadataDetailsProvider>(s =>
            {
                var options = s.GetRequiredService<IOptions<MvcOptions>>().Value;
                return new DefaultCompositeMetadataDetailsProvider(options.ModelMetadataDetailsProviders);
            }));
            services.TryAddSingleton<IModelBinderFactory, ModelBinderFactory>();
            services.TryAddSingleton<IObjectModelValidator>(s =>
            {
                var options = s.GetRequiredService<IOptions<MvcOptions>>().Value;
                var metadataProvider = s.GetRequiredService<IModelMetadataProvider>();
                return new DefaultObjectValidator(metadataProvider, options.ModelValidatorProviders);
            });
            services.TryAddSingleton<ClientValidatorCache>();
            services.TryAddSingleton<ParameterBinder>();

            //
            // Random Infrastructure
            //
            services.TryAddSingleton<MvcMarkerService, MvcMarkerService>();
            services.TryAddSingleton<ITypeActivatorCache, TypeActivatorCache>();
            services.TryAddSingleton<IUrlHelperFactory, UrlHelperFactory>();
            services.TryAddSingleton<IHttpRequestStreamReaderFactory, MemoryPoolHttpRequestStreamReaderFactory>();
            services.TryAddSingleton<IHttpResponseStreamWriterFactory, MemoryPoolHttpResponseStreamWriterFactory>();
            services.TryAddSingleton(ArrayPool<byte>.Shared);
            services.TryAddSingleton(ArrayPool<char>.Shared);
            services.TryAddSingleton<ObjectResultExecutor>();
            services.TryAddSingleton<PhysicalFileResultExecutor>();
            services.TryAddSingleton<VirtualFileResultExecutor>();
            services.TryAddSingleton<FileStreamResultExecutor>();
            services.TryAddSingleton<FileContentResultExecutor>();
            services.TryAddSingleton<RedirectResultExecutor>();
            services.TryAddSingleton<LocalRedirectResultExecutor>();
            services.TryAddSingleton<RedirectToActionResultExecutor>();
            services.TryAddSingleton<RedirectToRouteResultExecutor>();
            services.TryAddSingleton<RedirectToPageResultExecutor>();
            services.TryAddSingleton<ContentResultExecutor>();

            //
            // Route Handlers
            //
            services.TryAddSingleton<MvcRouteHandler>(); // Only one per app
            services.TryAddTransient<MvcAttributeRouteHandler>(); // Many per app

            //
            // Middleware pipeline filter related
            //
            services.TryAddSingleton<MiddlewareFilterConfigurationProvider>();
            // This maintains a cache of middleware pipelines, so it needs to be a singleton
            services.TryAddSingleton<MiddlewareFilterBuilder>();
        }

        private static void ConfigureDefaultServices(IServiceCollection services)
        {
            services.AddRouting();
        }
    }
}
