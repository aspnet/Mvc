// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Mvc.OptionDescriptors
{
    /// <inheritdoc />
    public class DefaultModelValidatorProviderProvider : OptionDescriptorBasedProvider<IModelValidatorProvider>,
                                                         IModelValidatorProviderProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultModelValidatorProviderProvider"/> class.
        /// </summary>
        /// <param name="options">An accessor to the <see cref="MvcOptions"/> configured for this application.</param>
        /// <param name="optionActivator">A <see cref="IServiceProvider"/> instance that retrieves services from the
        /// service collection.</param>
        public DefaultModelValidatorProviderProvider(
                IOptions<MvcOptions> optionsAccessor,
                IOptionActivator<IModelValidatorProvider> optionActivator)
            : base(optionsAccessor.Options.ModelValidatorProviders, optionActivator)
        {
        }

        /// <inheritdoc />
        public IReadOnlyList<IModelValidatorProvider> ModelValidatorProviders
        {
            get { return Options; }
        }
    }
}