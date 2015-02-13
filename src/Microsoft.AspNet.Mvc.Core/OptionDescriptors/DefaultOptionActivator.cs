// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc.OptionDescriptors
{
    /// <summary>
    /// Encapsulates information that creates a <typeparamref name="TOption"/> option on <see cref="MvcOptions"/>.
    /// </summary>
    /// <typeparam name="TOption">The type of the option.</typeparam>
    public class DefaultOptionActivator<TOption> : IOptionActivator<TOption>
    {
        private Func<Type, ObjectFactory> CreateFactory =
            (t) => ActivatorUtilities.CreateFactory(t, Type.EmptyTypes);
        private ConcurrentDictionary<Type, ObjectFactory> _optionActivatorCache =
               new ConcurrentDictionary<Type, ObjectFactory>();

        /// <summary>
        /// Creates an instance of <typeparamref name="TOption"/>.
        /// </summary>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> instance that retrieves services from the
        /// service collection.</param>
        /// <param name="optionType">The <see cref="Type"/> of the <typeparamref name="TOption"/> to create.</param>
        public TOption CreateInstance([NotNull] IServiceProvider serviceProvider, [NotNull] Type optionType)
        {
            var optionFactory = _optionActivatorCache.GetOrAdd(optionType, CreateFactory);
            return (TOption)optionFactory(serviceProvider, null);
        }
    }
}