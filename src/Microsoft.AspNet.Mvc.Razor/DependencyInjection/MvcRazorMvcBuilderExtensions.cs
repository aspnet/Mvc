// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.AspNet.Mvc.Razor.Compilation;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.Framework.DependencyInjection.Extensions;
using Microsoft.Framework.Internal;

namespace Microsoft.Framework.DependencyInjection
{
    /// <summary>
    /// Extensions methods for configuring MVC via an <see cref="IMvcBuilder"/>.
    /// </summary>
    public static class MvcRazorMvcBuilderExtensions
    {
        /// <summary>
        /// Configures a set of <see cref="RazorViewEngineOptions"/> for the application.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
        /// <param name="setupAction">An action to configure the <see cref="RazorViewEngineOptions"/>.</param>
        /// <returns>The <see cref="IMvcBuilder"/>.</returns>
        public static IMvcBuilder AddRazorOptions(
            [NotNull] this IMvcBuilder builder,
            [NotNull] Action<RazorViewEngineOptions> setupAction)
        {
            builder.Services.Configure(setupAction);
            return builder;
        }

        /// <summary>
        /// Adds an initialization callback for a given <typeparamref name="TTagHelper"/>.
        /// </summary>
        /// <remarks>
        /// The callback will be invoked on any <typeparamref name="TTagHelper"/> instance before the
        /// <see cref="ITagHelper.ProcessAsync(TagHelperContext, TagHelperOutput)"/> method is called.
        /// </remarks>
        /// <typeparam name="TTagHelper">The type of <see cref="ITagHelper"/> being initialized.</typeparam>
        /// <param name="builder">The <see cref="IMvcBuilder"/> instance this method extends.</param>
        /// <param name="initialize">An action to initialize the <typeparamref name="TTagHelper"/>.</param>
        /// <returns>The <see cref="IMvcBuilder"/> instance this method extends.</returns>
        public static IMvcBuilder InitializeTagHelper<TTagHelper>(
            [NotNull] this IMvcBuilder builder,
            [NotNull] Action<TTagHelper, ViewContext> initialize)
            where TTagHelper : ITagHelper
        {
            var initializer = new TagHelperInitializer<TTagHelper>(initialize);

            builder.Services.AddInstance(typeof(ITagHelperInitializer<TTagHelper>), initializer);

            return builder;
        }

        public static IMvcBuilder AddPrecompiledRazorViews(
            [NotNull] this IMvcBuilder builder,
            [NotNull] params Assembly[] assemblies)
        {
            builder.Services.Replace(
                ServiceDescriptor.Singleton<ICompilerCacheProvider>(serviceProvider =>
                    ActivatorUtilities.CreateInstance<PrecompiledViewsCompilerCacheProvider>(
                        serviceProvider,
                        assemblies.AsEnumerable())));

            return builder;
        }

        public static IServiceCollection AddPrecompiledRazorViews(
            [NotNull] this IServiceCollection collection,
            [NotNull] params Assembly[] assemblies)
        {
            var razorFileInfos = GetFileInfoCollections(assemblies);
            collection.TryAddEnumerable(ServiceDescriptor.Instance(razorFileInfos));

            return collection;
        }

        public static IMvcBuilder ConfigureRazorViewEngine(
            [NotNull] this IMvcBuilder builder,
            [NotNull] Action<RazorViewEngineOptions> setupAction)
        {
            builder.Services.Configure(setupAction);
            return builder;
        }

        // Internal for testing.
        internal static void AddRazorViewEngineServices(IServiceCollection services)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<MvcViewOptions>, MvcRazorMvcViewOptionsSetup>());
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<RazorViewEngineOptions>, RazorViewEngineOptionsSetup>());

            services.TryAddSingleton<IRazorViewEngine, RazorViewEngine>();

            // Caches view locations that are valid for the lifetime of the application.
            services.TryAddSingleton<IViewLocationCache, DefaultViewLocationCache>();
            services.TryAdd(ServiceDescriptor.Singleton<IChunkTreeCache>(serviceProvider =>
            {
                var cachedFileProvider = serviceProvider.GetRequiredService<IOptions<RazorViewEngineOptions>>();
                return new DefaultChunkTreeCache(cachedFileProvider.Value.FileProvider);
            }));

            // The host is designed to be discarded after consumption and is very inexpensive to initialize.
            services.TryAddTransient<IMvcRazorHost, MvcRazorHost>();

            // Caches compilation artifacts across the lifetime of the application.
            services.TryAddSingleton<ICompilerCache, CompilerCache>();

            // This caches compilation related details that are valid across the lifetime of the application
            // and is required to be a singleton.
            services.TryAddSingleton<ICompilationService, RoslynCompilationService>();

            // Both the compiler cache and roslyn compilation service hold on the compilation related
            // caches. RazorCompilation service is just an adapter service, and it is transient to ensure
            // the IMvcRazorHost dependency does not maintain state.
            services.TryAddTransient<IRazorCompilationService, RazorCompilationService>();

            // The ViewStartProvider needs to be able to consume scoped instances of IRazorPageFactory
            services.TryAddScoped<IViewStartProvider, ViewStartProvider>();
            services.TryAddTransient<IRazorViewFactory, RazorViewFactory>();
            services.TryAddSingleton<IRazorPageActivator, RazorPageActivator>();

            // Virtual path view factory needs to stay scoped so views can get get scoped services.
            services.TryAddScoped<IRazorPageFactory, VirtualPathRazorPageFactory>();

            // Only want one ITagHelperActivator so it can cache Type activation information. Types won't conflict.
            services.TryAddSingleton<ITagHelperActivator, DefaultTagHelperActivator>();

            // Consumed by the Cache tag helper to cache results across the lifetime of the application.
            services.TryAddSingleton<IMemoryCache, MemoryCache>();
        }

        private static IEnumerable<RazorFileInfoCollection> GetFileInfoCollections(IEnumerable<Assembly> assemblies) =>
            assemblies
                .SelectMany(assembly => assembly.ExportedTypes)
                .Where(IsValidRazorFileInfoCollection)
                .Select(Activator.CreateInstance)
                .Cast<RazorFileInfoCollection>();

        internal static bool IsValidRazorFileInfoCollection(Type type) =>
            RazorFileInfoCollectionType.IsAssignableFrom(type) &&
            !type.GetTypeInfo().IsAbstract &&
            !type.GetTypeInfo().ContainsGenericParameters;
    }
}
