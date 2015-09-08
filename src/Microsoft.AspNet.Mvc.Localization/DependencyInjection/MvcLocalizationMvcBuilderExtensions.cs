// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Localization;
using Microsoft.AspNet.Mvc.Localization.Internal;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.Framework.DependencyInjection.Extensions;
using Microsoft.Framework.Internal;
using Microsoft.Framework.WebEncoders;

namespace Microsoft.Framework.DependencyInjection
{
    /// <summary>
    /// Extension methods for configuring MVC view localization.
    /// </summary>
    public static class MvcLocalizationMvcBuilderExtensions
    {
        /// <summary>
        /// Adds MVC localization to the application.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
        /// <returns>The <see cref="IMvcBuilder"/>.</returns>
        public static IMvcBuilder AddViewLocalization([NotNull] this IMvcBuilder builder)
        {
            return AddViewLocalization(builder, LanguageViewLocationExpanderFormat.Suffix);
        }

        /// <summary>
        ///  Adds MVC localization to the application.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
        /// <param name="format">The view format for localized views.</param>
        /// <returns>The <see cref="IMvcBuilder"/>.</returns>
        public static IMvcBuilder AddViewLocalization(
            [NotNull] this IMvcBuilder builder,
            LanguageViewLocationExpanderFormat format)
        {
            MvcLocalizationServices.AddLocalizationServices(builder.Services, format);
            return builder;
        }
    }
}