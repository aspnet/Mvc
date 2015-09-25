﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc.DataAnnotations.Internal;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;

namespace Microsoft.Framework.DependencyInjection
{
    public static class MvcDataAnnotationsMvcBuilderExtensions
    {
        /// <summary>
        /// Adds MVC data annotations localization to the application.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
        /// <returns>The <see cref="IMvcBuilder"/>.</returns>
        public static IMvcBuilder AddDataAnnotationsLocalization(this IMvcBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return AddDataAnnotationsLocalization(builder, setupAction: null);
        }

        /// <summary>
        /// Adds MVC data annotations localization to the application.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
        /// <param name="setupAction">The action to configure <see cref="MvcDataAnnotationsLocalizationOptions"/>.
        /// </param>
        /// <returns>The <see cref="IMvcBuilder"/>.</returns>
        public static IMvcBuilder AddDataAnnotationsLocalization(
            this IMvcBuilder builder,
            Action<MvcDataAnnotationsLocalizationOptions> setupAction)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            DataAnnotationsLocalizationService.AddDataAnnotationsLocalizationServices(
                builder.Services,
                setupAction);

            return builder;
        }
    }
}
