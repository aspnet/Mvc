// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.OptionDescriptors
{
    /// <summary>
    /// Encapsulates information that creates a <typeparamref name="TOption"/> option on <see cref="MvcOptions"/>.
    /// </summary>
    /// <typeparam name="TOption">The type of the option.</typeparam>
    public interface IOptionActivator<TOption>
    {
        /// <summary>
        /// Creates an instance of <typeparamref name="TOption"/>.
        /// </summary>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> instance that retrieves services from the
        /// service collection.</param>
        /// <param name="optionType">The <see cref="Type"/> of the <typeparamref name="TOption"/> to create.</param>
        TOption CreateInstance(IServiceProvider serviceProvider, Type optionType);
    }
}