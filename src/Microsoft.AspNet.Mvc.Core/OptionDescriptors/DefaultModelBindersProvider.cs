﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Mvc.OptionDescriptors
{
    /// <inheritdoc />
    public class DefaultModelBindersProvider : OptionDescriptorBasedProvider<IModelBinder>, IModelBinderProvider
    {
        /// <summary>
        /// Initializes a new instance of the DefaultModelBindersProvider class.
        /// </summary>
        /// <param name="options">An accessor to the <see cref="MvcOptions"/> configured for this application.</param>
        /// <param name="optionActivator">As <see cref="IOptionActivator{TOption}"/> instance that creates an instance of type 
        /// <see cref="IModelBinder"/>.</param>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> instance that retrieves services from the
        /// service collection.</param>
        public DefaultModelBindersProvider(
                IOptions<MvcOptions> optionsAccessor,
                IOptionActivator<IModelBinder> optionActivator,
                IServiceProvider serviceProvider)
            : base(optionsAccessor.Options.ModelBinders, optionActivator, serviceProvider)
        {
        }

        /// <inheritdoc />
        public IReadOnlyList<IModelBinder> ModelBinders
        {
            get
            {
                return Options;
            }
        }
    }
}