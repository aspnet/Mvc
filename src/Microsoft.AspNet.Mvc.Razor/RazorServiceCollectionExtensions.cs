// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.Framework.Internal;

namespace Microsoft.Framework.DependencyInjection
{
    /// <summary>
    /// Contains extension methods to <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configures a set of <see cref="RazorViewEngineOptions"/> for the application.
        /// </summary>
        /// <param name="services">The services available in the application.</param>
        /// <param name="setupAction">An action to configure the <see cref="RazorViewEngineOptions"/>.</param>
        public static void ConfigureRazorViewEngineOptions(
            [NotNull] this IServiceCollection services,
            [NotNull] Action<RazorViewEngineOptions> setupAction)
        {
            services.Configure(setupAction);
        }

        /// <summary>
        /// Adds an initialization callback for a given <see cref="ITagHelper"/> type.
        /// </summary>
        /// <remarks>
        /// The callback will be invoked on the <see cref="ITagHelper"/> of the specified type before the
        /// <see cref="ITagHelper.ProcessAsync(TagHelperContext, TagHelperOutput)"/> method is called.
        /// </remarks>
        /// <typeparam name="TTagHelper">The type of <see cref="ITagHelper"/> being initialized.</typeparam>
        /// <param name="services">The services available in the application.</param>
        /// <param name="initialize">An action to initialize the <see cref="ITagHelper"/>.</param>
        public static void InitializeTagHelper<TTagHelper>(
            [NotNull] this IServiceCollection services,
            [NotNull] Action<TTagHelper, ViewContext> initialize)
            where TTagHelper : ITagHelper
        {
            var initializer = new InitializeTagHelper<TTagHelper>(initialize);

            services.AddInstance(typeof(IInitializeTagHelper<TTagHelper>), initializer);
        }
    }
}