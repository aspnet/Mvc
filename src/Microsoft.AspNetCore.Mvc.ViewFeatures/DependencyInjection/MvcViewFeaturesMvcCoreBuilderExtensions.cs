// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MvcViewFeaturesMvcCoreBuilderExtensions
    {
        public static IMvcCoreBuilder AddViews(this IMvcCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddDataAnnotations();
            AddViewComponentApplicationPartsProviders(builder.PartManager);
            AddViewServices(builder.Services);
            return builder;
        }

        private static void AddViewComponentApplicationPartsProviders(ApplicationPartManager manager)
        {
            if (!manager.FeatureProviders.OfType<ViewComponentFeatureProvider>().Any())
            {
                manager.FeatureProviders.Add(new ViewComponentFeatureProvider());
            }
        }

        public static IMvcCoreBuilder AddViews(
            this IMvcCoreBuilder builder,
            Action<MvcViewOptions> setupAction)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            builder.AddDataAnnotations();
            AddViewServices(builder.Services);

            if (setupAction != null)
            {
                builder.Services.Configure(setupAction);
            }

            return builder;
        }

        public static IMvcCoreBuilder ConfigureViews(
            this IMvcCoreBuilder builder,
            Action<MvcViewOptions> setupAction)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            builder.Services.Configure(setupAction);
            return builder;
        }

        // Internal for testing.
        internal static void AddViewServices(IServiceCollection services)
        {
            services.AddDataProtection();
            services.AddAntiforgery();
            services.AddWebEncoders();

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<MvcViewOptions>, MvcViewOptionsSetup>());
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, TempDataMvcOptionsSetup>());

            //
            // View Engine and related infrastructure
            //
            services.TryAddSingleton<ICompositeViewEngine, CompositeViewEngine>();
            services.TryAddSingleton<ViewResultExecutor>();
            services.TryAddSingleton<PartialViewResultExecutor>();

            // Support for activating ViewDataDictionary
            services.TryAddEnumerable(
                ServiceDescriptor
                    .Transient<IControllerPropertyActivator, ViewDataDictionaryControllerPropertyActivator>());

            //
            // HTML Helper
            //
            services.TryAddTransient<IHtmlHelper, HtmlHelper>();
            services.TryAddTransient(typeof(IHtmlHelper<>), typeof(HtmlHelper<>));
            services.TryAddSingleton<IHtmlGenerator, DefaultHtmlGenerator>();
            services.TryAddSingleton<ExpressionTextCache>();
            services.TryAddSingleton<IModelExpressionProvider, ModelExpressionProvider>();

            //
            // JSON Helper
            //
            services.TryAddSingleton<IJsonHelper, JsonHelper>();
            services.TryAdd(ServiceDescriptor.Singleton<JsonOutputFormatter>(serviceProvider =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<MvcJsonOptions>>().Value;
                var charPool = serviceProvider.GetRequiredService<ArrayPool<char>>();
                return new JsonOutputFormatter(options.SerializerSettings, charPool);
            }));

            //
            // View Components
            //
            
            // These do caching so they should stay singleton
            services.TryAddSingleton<IViewComponentSelector, DefaultViewComponentSelector>();
            services.TryAddSingleton<IViewComponentFactory, DefaultViewComponentFactory>();
            services.TryAddSingleton<IViewComponentActivator, DefaultViewComponentActivator>();
            services.TryAddSingleton<
                IViewComponentDescriptorCollectionProvider,
                DefaultViewComponentDescriptorCollectionProvider>();
            services.TryAddTransient<ViewComponentResultExecutor>();

            services.TryAddSingleton<ViewComponentInvokerCache>();
            services.TryAddTransient<IViewComponentDescriptorProvider, DefaultViewComponentDescriptorProvider>();
            services.TryAddSingleton<IViewComponentInvokerFactory, DefaultViewComponentInvokerFactory>();
            services.TryAddTransient<IViewComponentHelper, DefaultViewComponentHelper>();

            //
            // Temp Data
            //
            // This does caching so it should stay singleton
            services.TryAddSingleton<ITempDataProvider, SessionStateTempDataProvider>();

            //
            // Antiforgery
            //
            services.TryAddSingleton<ValidateAntiforgeryTokenAuthorizationFilter>();
            services.TryAddSingleton<AutoValidateAntiforgeryTokenAuthorizationFilter>();

            // These are stateless so their lifetime isn't really important.
            services.TryAddSingleton<ITempDataDictionaryFactory, TempDataDictionaryFactory>();
            services.TryAddSingleton<SaveTempDataFilter>();

            services.TryAddSingleton(ArrayPool<ViewBufferValue>.Shared);
            services.TryAddScoped<IViewBufferScope, MemoryPoolViewBufferScope>();
        }
    }
}
