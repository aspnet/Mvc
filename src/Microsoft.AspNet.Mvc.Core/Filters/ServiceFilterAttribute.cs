﻿using System;
using System.Diagnostics;
using Microsoft.AspNet.Mvc.Core;

namespace Microsoft.AspNet.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    [DebuggerDisplay("ServiceFilter: Type={ServiceType} Order={Order}")]
    public class ServiceFilterAttribute : Attribute, IFilterFactory, IOrderedFilter
    {
        public ServiceFilterAttribute([NotNull] Type type)
        {
            ServiceType = type;
        }

        public Type ServiceType { get; private set; }

        public int Order { get; set; }

        public IFilter CreateInstance([NotNull] IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetService(ServiceType);

            var filter = service as IFilter;
            if (filter == null)
            {
                throw new InvalidOperationException(Resources.FormatFilterFactoryAttribute_TypeMustImplementIFilter(
                    typeof(ServiceFilterAttribute).Name,
                    typeof(IFilter).Name));
            }

            return filter;
        }
    }
}
