﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.ModelBinding.Validation;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Mvc.DataAnnotations.Internal
{
    /// <summary>
    /// Sets up default options for <see cref="MvcDataAnnotationsLocalizationOptions"/>.
    /// </summary>
    public class MvcDataAnnotationsLocalizationOptionsSetup : ConfigureOptions<MvcDataAnnotationsLocalizationOptions>
    {
        public MvcDataAnnotationsLocalizationOptionsSetup()
            : base(ConfigureMvc)
        {
        }

        public static void ConfigureMvc(MvcDataAnnotationsLocalizationOptions options)
        {
            options.DataAnnotationLocalizerProvider = (modelType, stringLocalizerFactory) =>
                stringLocalizerFactory.Create(modelType);
        }
    }
}
