// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Localization;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for configuring MVC view and data annotations localization services.
    /// </summary>
    public static class MvcLocalizationMvcBuilderExtensions
    {
        /// <summary>
        /// Adds MVC view localization services to the application.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
        /// <returns>The <see cref="IMvcBuilder"/>.</returns>
        public static IMvcBuilder AddViewLocalization(this IMvcBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return AddViewLocalization(builder, LanguageViewLocationExpanderFormat.Suffix);
        }

        /// <summary>
        ///  Adds MVC view localization services to the application.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
        /// <param name="format">The view format for localized views.</param>
        /// <returns>The <see cref="IMvcBuilder"/>.</returns>
        public static IMvcBuilder AddViewLocalization(
            this IMvcBuilder builder,
            LanguageViewLocationExpanderFormat format)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            AddViewLocalization(builder, format, setupAction: null);
            return builder;
        }

        /// <summary>
        ///  Adds MVC view localization services to the application.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
        /// <param name="setupAction">An action to configure the <see cref="LocalizationOptions"/>.</param>
        /// <returns>The <see cref="IMvcBuilder"/>.</returns>
        public static IMvcBuilder AddViewLocalization(
            this IMvcBuilder builder,
            Action<LocalizationOptions> setupAction)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            AddViewLocalization(builder, LanguageViewLocationExpanderFormat.Suffix, setupAction);
            return builder;
        }

        /// <summary>
        ///  Adds MVC view localization services to the application.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
        /// <param name="format">The view format for localized views.</param>
        /// <param name="setupAction">An action to configure the <see cref="LocalizationOptions"/>.</param>
        /// <returns>The <see cref="IMvcBuilder"/>.</returns>
        public static IMvcBuilder AddViewLocalization(
            this IMvcBuilder builder,
            LanguageViewLocationExpanderFormat format,
            Action<LocalizationOptions> setupAction)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            MvcLocalizationServices.AddLocalizationServices(builder.Services, format, setupAction);
            return builder;
        }

        /// <summary>
        /// Adds MVC view and data annotations localization services to the application.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
        /// <returns>The <see cref="IMvcBuilder"/>.</returns>
        /// <remarks>
        /// Adding localization also adds support for views via
        /// <see cref="MvcViewFeaturesMvcCoreBuilderExtensions.AddViews(IMvcCoreBuilder)"/> and the Razor view engine
        /// via <see cref="MvcRazorMvcCoreBuilderExtensions.AddRazorViewEngine(IMvcCoreBuilder)"/>.
        /// </remarks>
        public static IMvcBuilder AddMvcLocalization(this IMvcBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return AddMvcLocalization(
                builder,
                localizationOptionsSetupAction: null,
                format: LanguageViewLocationExpanderFormat.Suffix,
                dataAnnotationsLocalizationOptionsSetupAction: null);
        }

        /// <summary>
        /// Adds MVC view and data annotations localization services to the application.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
        /// <param name="localizationOptionsSetupAction">An action to configure the <see cref="LocalizationOptions"/>.</param>
        /// <returns>The <see cref="IMvcBuilder"/>.</returns>
        /// <remarks>
        /// Adding localization also adds support for views via
        /// <see cref="MvcViewFeaturesMvcCoreBuilderExtensions.AddViews(IMvcCoreBuilder)"/> and the Razor view engine
        /// via <see cref="MvcRazorMvcCoreBuilderExtensions.AddRazorViewEngine(IMvcCoreBuilder)"/>.
        /// </remarks>
        public static IMvcBuilder AddMvcLocalization(
            this IMvcBuilder builder,
            Action<LocalizationOptions> localizationOptionsSetupAction)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return AddMvcLocalization(
                builder,
                localizationOptionsSetupAction,
                LanguageViewLocationExpanderFormat.Suffix,
                dataAnnotationsLocalizationOptionsSetupAction: null);
        }

        /// <summary>
        /// Adds MVC view and data annotations localization services to the application.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
        /// <param name="format">The view format for localized views.</param>
        /// <returns>The <see cref="IMvcBuilder"/>.</returns>
        /// <remarks>
        /// Adding localization also adds support for views via
        /// <see cref="MvcViewFeaturesMvcCoreBuilderExtensions.AddViews(IMvcCoreBuilder)"/> and the Razor view engine
        /// via <see cref="MvcRazorMvcCoreBuilderExtensions.AddRazorViewEngine(IMvcCoreBuilder)"/>.
        /// </remarks>
        public static IMvcBuilder AddMvcLocalization(
            this IMvcBuilder builder,
            LanguageViewLocationExpanderFormat format)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return AddMvcLocalization(
                builder,
                localizationOptionsSetupAction: null,
                format: format,
                dataAnnotationsLocalizationOptionsSetupAction: null);
        }

        /// <summary>
        /// Adds MVC view and data annotations localization services to the application.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
        /// <param name="localizationOptionsSetupAction">An action to configure the
        /// <see cref="LocalizationOptions"/>.</param>
        /// <param name="format">The view format for localized views.</param>
        /// <returns>The <see cref="IMvcBuilder"/>.</returns>
        /// <remarks>
        /// Adding localization also adds support for views via
        /// <see cref="MvcViewFeaturesMvcCoreBuilderExtensions.AddViews(IMvcCoreBuilder)"/> and the Razor view engine
        /// via <see cref="MvcRazorMvcCoreBuilderExtensions.AddRazorViewEngine(IMvcCoreBuilder)"/>.
        /// </remarks>
        public static IMvcBuilder AddMvcLocalization(
            this IMvcBuilder builder,
            Action<LocalizationOptions> localizationOptionsSetupAction,
            LanguageViewLocationExpanderFormat format)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return AddMvcLocalization(
                builder,
                localizationOptionsSetupAction: localizationOptionsSetupAction,
                format: format,
                dataAnnotationsLocalizationOptionsSetupAction: null);
        }

        /// <summary>
        /// Adds MVC view and data annotations localization services to the application.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
        /// <param name="dataAnnotationsLocalizationOptionsSetupAction">An action to configure the
        /// <see cref="MvcDataAnnotationsLocalizationOptions"/>.</param>
        /// <returns>The <see cref="IMvcBuilder"/>.</returns>
        /// <remarks>
        /// Adding localization also adds support for views via
        /// <see cref="MvcViewFeaturesMvcCoreBuilderExtensions.AddViews(IMvcCoreBuilder)"/> and the Razor view engine
        /// via <see cref="MvcRazorMvcCoreBuilderExtensions.AddRazorViewEngine(IMvcCoreBuilder)"/>.
        /// </remarks>
        public static IMvcBuilder AddMvcLocalization(
            this IMvcBuilder builder,
            Action<MvcDataAnnotationsLocalizationOptions> dataAnnotationsLocalizationOptionsSetupAction)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return AddMvcLocalization(
                builder,
                localizationOptionsSetupAction: null,
                format: LanguageViewLocationExpanderFormat.Suffix,
                dataAnnotationsLocalizationOptionsSetupAction: dataAnnotationsLocalizationOptionsSetupAction);
        }

        /// <summary>
        /// Adds MVC view and data annotations localization services to the application.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
        /// <param name="localizationOptionsSetupAction">An action to configure the
        /// <see cref="LocalizationOptions"/>.</param>
        /// <param name="dataAnnotationsLocalizationOptionsSetupAction">An action to configure the
        /// <see cref="MvcDataAnnotationsLocalizationOptions"/>.</param>
        /// <returns>The <see cref="IMvcBuilder"/>.</returns>
        /// <remarks>
        /// Adding localization also adds support for views via
        /// <see cref="MvcViewFeaturesMvcCoreBuilderExtensions.AddViews(IMvcCoreBuilder)"/> and the Razor view engine
        /// via <see cref="MvcRazorMvcCoreBuilderExtensions.AddRazorViewEngine(IMvcCoreBuilder)"/>.
        /// </remarks>
        public static IMvcBuilder AddMvcLocalization(
            this IMvcBuilder builder,
            Action<LocalizationOptions> localizationOptionsSetupAction,
            Action<MvcDataAnnotationsLocalizationOptions> dataAnnotationsLocalizationOptionsSetupAction)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return AddMvcLocalization(
                builder,
                localizationOptionsSetupAction: localizationOptionsSetupAction,
                format: LanguageViewLocationExpanderFormat.Suffix,
                dataAnnotationsLocalizationOptionsSetupAction: dataAnnotationsLocalizationOptionsSetupAction);
        }

        /// <summary>
        /// Adds MVC view and data annotations localization services to the application.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
        /// <param name="format">The view format for localized views.</param>
        /// <param name="dataAnnotationsLocalizationOptionsSetupAction">An action to configure the
        /// <see cref="MvcDataAnnotationsLocalizationOptions"/>.</param>
        /// <returns>The <see cref="IMvcBuilder"/>.</returns>
        /// <remarks>
        /// Adding localization also adds support for views via
        /// <see cref="MvcViewFeaturesMvcCoreBuilderExtensions.AddViews(IMvcCoreBuilder)"/> and the Razor view engine
        /// via <see cref="MvcRazorMvcCoreBuilderExtensions.AddRazorViewEngine(IMvcCoreBuilder)"/>.
        /// </remarks>
        public static IMvcBuilder AddMvcLocalization(
            this IMvcBuilder builder,
            LanguageViewLocationExpanderFormat format,
            Action<MvcDataAnnotationsLocalizationOptions> dataAnnotationsLocalizationOptionsSetupAction)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return AddMvcLocalization(
                builder,
                localizationOptionsSetupAction: null,
                format: format,
                dataAnnotationsLocalizationOptionsSetupAction: dataAnnotationsLocalizationOptionsSetupAction);
        }

        /// <summary>
        /// Adds MVC view and data annotations localization services to the application.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
        /// <param name="localizationOptionsSetupAction">An action to configure the <see cref="LocalizationOptions"/>.
        /// Can be <c>null</c>.</param>
        /// <param name="format">The view format for localized views.</param>
        /// <param name="dataAnnotationsLocalizationOptionsSetupAction">An action to configure 
        /// the <see cref="MvcDataAnnotationsLocalizationOptions"/>. Can be <c>null</c>.</param>
        /// <returns>The <see cref="IMvcBuilder"/>.</returns>
        /// <remarks>
        /// Adding localization also adds support for views via
        /// <see cref="MvcViewFeaturesMvcCoreBuilderExtensions.AddViews(IMvcCoreBuilder)"/> and the Razor view engine
        /// via <see cref="MvcRazorMvcCoreBuilderExtensions.AddRazorViewEngine(IMvcCoreBuilder)"/>.
        /// </remarks>
        public static IMvcBuilder AddMvcLocalization(
            this IMvcBuilder builder,
            Action<LocalizationOptions> localizationOptionsSetupAction,
            LanguageViewLocationExpanderFormat format,
            Action<MvcDataAnnotationsLocalizationOptions> dataAnnotationsLocalizationOptionsSetupAction)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder
                .AddViewLocalization(format, localizationOptionsSetupAction)
                .AddDataAnnotationsLocalization(dataAnnotationsLocalizationOptionsSetupAction);
        }
    }
}