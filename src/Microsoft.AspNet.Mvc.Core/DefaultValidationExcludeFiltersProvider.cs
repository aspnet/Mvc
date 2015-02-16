﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.ModelBinding;
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
        /// <param name="optionActivator">As <see cref="IOptionActivator{TOption}"/> instance that creates an
        ///  instance of type <see cref="IExcludeTypeValidationFilter"/>.</param>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> instance that retrieves services from the
        /// service collection.</param>
        public DefaultValidationExcludeFiltersProvider(IOptions<MvcOptions> optionsAccessor,
                                                       IOptionActivator<IExcludeTypeValidationFilter> optionActivator,
                                                       IServiceProvider serviceProvider)
            : base(optionsAccessor.Options.ValidationExcludeFilters, optionActivator, serviceProvider)
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