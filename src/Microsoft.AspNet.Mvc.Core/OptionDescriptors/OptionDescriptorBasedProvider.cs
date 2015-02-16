﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc.OptionDescriptors
{
    /// <summary>
    /// Provides a default implementation for instantiating options from a sequence of
    /// <see cref="OptionDescriptor{TOption}"/>.
    /// </summary>
    /// <typeparam name="TOption">The type of the option.</typeparam>
    public abstract class OptionDescriptorBasedProvider<TOption>
    {
        private readonly IEnumerable<OptionDescriptor<TOption>> _optionDescriptors;
        private ITypeActivatorCache _optionActivator;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionDescriptorBasedProvider"/> class.
        /// </summary>
        /// <param name="optionDescriptors">An enumerable of <see cref="OptionDescriptor{TOption}"/>.</param>
        /// <param name="optionActivator">As <see cref="ITypeActivatorCache{TOption}"/> instance that creates an
        /// instance of type <typeparamref name="TOption"/>.</param>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> instance that retrieves services from the
        /// service collection.</param>
        public OptionDescriptorBasedProvider(
            [NotNull] IEnumerable<OptionDescriptor<TOption>> optionDescriptors,
            [NotNull] ITypeActivatorCache optionActivator,
            [NotNull] IServiceProvider serviceProvider)
        {
            _optionDescriptors = optionDescriptors;
            _optionActivator = optionActivator;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Gets an activated sequence of <typeparamref name="TOption"/> instance.
        /// </summary>
        protected IReadOnlyList<TOption> Options
        {
            get
            {
                var result = new List<TOption>();
                foreach (var descriptor in _optionDescriptors)
                {
                    var instance = descriptor.Instance;
                    if (instance == null)
                    {
                        instance = _optionActivator.CreateInstance(_serviceProvider, descriptor.OptionType);
                    }

                    result.Add(instance);
                }

                return result;
            }
        }
    }
}