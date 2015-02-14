// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    [DebuggerDisplay("TypeFilter: Type={ImplementationType} Order={Order}")]
    public class TypeFilterAttribute : Attribute, IFilterFactory, IOrderedFilter
    {
        private ObjectFactory _createFactory;

        public TypeFilterAttribute([NotNull] Type type)
        {
            ImplementationType = type;
        }

        public object[] Arguments { get; set; }

        public Type ImplementationType { get; private set; }

        public int Order { get; set; }

        private ObjectFactory CreateFactory
        {
            get
            {
                if (_createFactory == null)
                {
                    _createFactory = ActivatorUtilitiesHelper.CreateFactory(ImplementationType);
                }

                return _createFactory;
            }
        }

        public IFilter CreateInstance([NotNull] IServiceProvider serviceProvider)
        {
            return (IFilter)CreateFactory(serviceProvider, null);
        }
    }
}