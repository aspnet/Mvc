// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc.Core;

namespace Microsoft.AspNet.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public abstract class ParametrizedFilterAttribute : Attribute, IFilterFactory, IOrderedFilter
    {
        private readonly Type _serviceType;

        protected ParametrizedFilterAttribute()
        {
            _serviceType = typeof (IParametrizedFilter<>).MakeGenericType(GetType());
        }

        public int Order { get; set; }

        public IFilter CreateInstance(IServiceProvider serviceProvider)
        {
            var handler = serviceProvider.GetService(_serviceType);

            if (handler == null || !_serviceType.IsAssignableFrom(handler.GetType()))
            {
                throw new InvalidOperationException(
                    Resources.FormatFilterFactoryAttribute_TypeMustImplementIFilter(
                        GetType(),
                        _serviceType.Name
                    )
                );
            }

            return Wrap((dynamic) handler, (dynamic)this);
        }

        private IFilter Wrap<T>(IParametrizedFilter<T> handler, T data)
        {
            return new ParametrizedFilterWrapper<T>(handler, data);
        }
    }
}