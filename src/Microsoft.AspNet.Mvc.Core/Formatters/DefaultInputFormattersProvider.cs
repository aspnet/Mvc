// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Mvc.OptionDescriptors
{
    /// <inheritdoc />
    public class DefaultInputFormattersProvider
        : OptionDescriptorBasedProvider<IInputFormatter>, IInputFormattersProvider
    {
        /// <summary>
        /// Initializes a new instance of the DefaultInputFormattersProvider class.
        /// </summary>
        /// <param name="options">An accessor to the <see cref="MvcOptions"/> configured for this application.</param>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> instance that retrieves services from the
        /// service collection.</param>
        public DefaultInputFormattersProvider(IOptions<MvcOptions> optionsAccessor,
                                              IOptionActivator<IInputFormatter> optionActivator)
            : base(optionsAccessor.Options.InputFormatters, optionActivator)
        {
        }

        /// <inheritdoc />
        public IReadOnlyList<IInputFormatter> InputFormatters
        {
            get
            {
                return Options;
            }
        }
    }
}