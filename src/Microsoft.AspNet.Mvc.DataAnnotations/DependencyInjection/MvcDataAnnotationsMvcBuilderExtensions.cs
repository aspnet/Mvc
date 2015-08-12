﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;
using Microsoft.Framework.DependencyInjection.Extensions;
using Microsoft.Framework.Internal;
using Microsoft.Framework.OptionsModel;
using Microsoft.Framework.DependencyInjection.Extensions;

namespace Microsoft.Framework.DependencyInjection
{
    public static class MvcDataAnnotationsMvcBuilderExtensions
    {
        public static IMvcBuilder AddDataAnnotations([NotNull] this IMvcBuilder builder)
        {
            AddDataAnnotationsServices(builder.Services);
            return builder;
        }

        // Internal for testing.
        internal static void AddDataAnnotationsServices(IServiceCollection services)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, MvcDataAnnotationsMvcOptionsSetup>());
        }
    }
}
