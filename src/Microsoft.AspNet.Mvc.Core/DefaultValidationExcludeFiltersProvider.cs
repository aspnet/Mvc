﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Mvc.OptionDescriptors
{
    /// <inheritdoc />
    public class DefaultValidationExcludeFiltersProvider
        : OptionDescriptorBasedProvider<IExcludeTypeValidationFilter>, IValidationExcludeFiltersProvider
    {
        /// <summary>
        /// Initializes a new instance of the DefaultBodyValidationExcludeFiltersProvider class.
        /// </summary>
        /// <param name="options">An accessor to the <see cref="MvcOptions"/> configured for this application.</param>
        /// <param name="typeActivator">An <see cref="ITypeActivator"/> instance used to instantiate types.</param>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> instance that retrieves services from the
        /// service collection.</param>
        public DefaultValidationExcludeFiltersProvider(IOptions<MvcOptions> optionsAccessor,
                                              ITypeActivator typeActivator,
                                              IServiceProvider serviceProvider)
            : base(optionsAccessor.Options.ValidationExcludeFilters, typeActivator, serviceProvider)
        {
        }

        /// <inheritdoc />
        public IReadOnlyList<IExcludeTypeValidationFilter> ExcludeFilters
        {
            get
            {
                return Options;
            }
        }
    }
}