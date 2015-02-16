﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Mvc.OptionDescriptors
{
    /// <inheritdoc />
    public class DefaultValueProviderFactoryProvider :
        OptionDescriptorBasedProvider<IValueProviderFactory>, IValueProviderFactoryProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultValueProviderFactoryProvider"/> class.
        /// </summary>
        /// <param name="options">An accessor to the <see cref="MvcOptions"/> configured for this application.</param>
        /// <param name="optionActivator">A <see cref="IServiceProvider"/> instance that retrieves services from the
        /// service collection.</param>
        public DefaultValueProviderFactoryProvider(
            IOptions<MvcOptions> optionsAccessor,
            IOptionActivator<IValueProviderFactory> optionActivator)
            : base(optionsAccessor.Options.ValueProviderFactories, optionActivator)
        {
        }

        /// <inheritdoc />
        public IReadOnlyList<IValueProviderFactory> ValueProviderFactories
        {
            get
            {
                return Options;
            }
        }
    }
}