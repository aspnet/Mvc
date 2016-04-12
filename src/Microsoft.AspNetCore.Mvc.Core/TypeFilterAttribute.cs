// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// A filter that activates another filter of type <see cref="ImplementationType"/>.
    /// </summary>
    /// <remarks>Primarily used in <see cref="M:FilterCollection.Add"/> calls.</remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    [DebuggerDisplay("TypeFilter: Type={ImplementationType} Order={Order}")]
    public class TypeFilterAttribute : Attribute, IFilterFactory, IOrderedFilter
    {
        private ObjectFactory _factory;

        /// <summary>
        /// Instantiates a new <see cref="TypeFilterAttribute"/> instance.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of filter to create.</param>
        public TypeFilterAttribute(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            ImplementationType = type;
        }

        /// <summary>
        /// Gets or sets the non-service arguments to pass to the <see cref="ImplementationType"/> constructor.
        /// </summary>
        public object[] Arguments { get; set; }

        /// <summary>
        /// Gets the <see cref="Type"/> of filter to activate.
        /// </summary>
        public Type ImplementationType { get; }

        /// <inheritdoc />
        public int Order { get; set; }

        /// <inheritdoc />
        public bool IsReusable { get; set; }

        /// <inheritdoc />
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            if (_factory == null)
            {
                var argumentTypes = Arguments?.Select(a => a.GetType())?.ToArray();

                _factory = ActivatorUtilities.CreateFactory(ImplementationType, argumentTypes ?? Type.EmptyTypes);
            }

            return (IFilterMetadata)_factory(serviceProvider, Arguments);
        }
    }
}