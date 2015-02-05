// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Mvc.OptionDescriptors
{
    /// <inheritdoc />
    public class DefaultViewEngineProvider : OptionDescriptorBasedProvider<IViewEngine>, IViewEngineProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultViewEngineProvider"/> class.
        /// </summary>
        /// <param name="options">An accessor to the <see cref="MvcOptions"/> configured for this application.</param>
        /// <param name="optionActivator">A <see cref="IServiceProvider"/> instance that retrieves services from the
        /// service collection.</param>
        public DefaultViewEngineProvider(
                IOptions<MvcOptions> optionsAccessor,
                IOptionActivator<IViewEngine> optionActivator,
                IServiceProvider serviceProvider)
            : base(optionsAccessor.Options.ViewEngines, optionActivator, serviceProvider)
        {
        }

        /// <inheritdoc />
        public IReadOnlyList<IViewEngine> ViewEngines
        {
            get
            {
                return Options;
            }
        }
    }
}