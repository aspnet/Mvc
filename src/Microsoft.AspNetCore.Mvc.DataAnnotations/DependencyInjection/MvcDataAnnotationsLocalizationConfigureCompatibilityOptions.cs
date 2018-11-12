﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    internal class MvcDataAnnotationsLocalizationConfigureCompatibilityOptions : ConfigureCompatibilityOptions<MvcDataAnnotationsLocalizationOptions>
    {
        public MvcDataAnnotationsLocalizationConfigureCompatibilityOptions(
            ILoggerFactory loggerFactory,
            IOptions<MvcCompatibilityOptions> compatibilityOptions)
            : base(loggerFactory, compatibilityOptions)
        {
        }

        protected override IReadOnlyDictionary<string, object> DefaultValues
        {
            get
            {
                var values = new Dictionary<string, object>();

                if (Version >= CompatibilityVersion.Version_2_2)
                {
                    values[nameof(MvcDataAnnotationsLocalizationOptions.AllowDataAnnotationsLocalizationForEnumDisplayAttributes)] = true;
                }

                return values;
            }
        }
    }
}
