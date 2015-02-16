﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.OptionDescriptors;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Mvc
{
    /// <inheritdoc />
    public class DefaultOutputFormattersProvider
        : OptionDescriptorBasedProvider<IOutputFormatter>, IOutputFormattersProvider
    {
        /// <summary>
        /// Initializes a new instance of the DefaultOutputFormattersProvider class.
        /// </summary>
        /// <param name="options">An accessor to the <see cref="MvcOptions"/> configured for this application.</param>
        /// <param name="optionActivator">As <see cref="IOptionActivator{TOption}"/> instance that creates an instance of type 
        /// <see cref="IOutputFormatter"/>.</param>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> instance that retrieves services from the
        /// service collection.</param>
        public DefaultOutputFormattersProvider(IOptions<MvcOptions> optionsAccessor,
                                               IOptionActivator<IOutputFormatter> optionActivator,
                                               IServiceProvider serviceProvider)
            : base(optionsAccessor.Options.OutputFormatters, optionActivator, serviceProvider)
        {
        }

        /// <inheritdoc />
        public IReadOnlyList<IOutputFormatter> OutputFormatters
        {
            get
            {
                return Options;
            }
        }
    }
}