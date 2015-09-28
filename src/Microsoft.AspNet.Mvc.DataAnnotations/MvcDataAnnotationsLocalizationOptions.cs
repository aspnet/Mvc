﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Framework.Localization;

namespace Microsoft.AspNet.Mvc.ModelBinding.Validation
{
    /// <summary>
    /// Provides programmatic configuration for DataAnnotations localization in the MVC framework.
    /// </summary>
    public class MvcDataAnnotationsLocalizationOptions
    {
        /// <summary>
        /// The delegate to invoke for creating <see cref="IStringLocalizer"/>.
        /// </summary>
        public Func<Type, IStringLocalizerFactory, IStringLocalizer> DataAnnotationLocalizerProvider;
    }
}
