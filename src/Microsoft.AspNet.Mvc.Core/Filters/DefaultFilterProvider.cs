// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNet.Cors.Core;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.Framework.Internal;
using System.Linq;

namespace Microsoft.AspNet.Mvc.Filters
{
    public class DefaultFilterProvider : IFilterProvider
    {
        public DefaultFilterProvider(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public int Order
        {
            get { return DefaultOrder.DefaultFrameworkSortOrder; }
        }

        protected IServiceProvider ServiceProvider { get; private set; }

        /// <inheritdoc />
        public void OnProvidersExecuting([NotNull] FilterProviderContext context)
        {
            if (context.ActionContext.ActionDescriptor.FilterDescriptors != null)
            {
                foreach (var item in context.Results)
                {
                    ProvideFilter(context, item);
                }
                context.Results =
                    context.Results
                    .OrderBy(filter => filter.Descriptor, FilterDescriptorCorsComparer.Comparer)
                    .ToList();
            }
        }

        /// <inheritdoc />
        public void OnProvidersExecuted([NotNull] FilterProviderContext context)
        {
        }

        public virtual void ProvideFilter(FilterProviderContext context, FilterItem filterItem)
        {
            if (filterItem.Filter != null)
            {
                return;
            }

            var filter = filterItem.Descriptor.Filter;

            var filterFactory = filter as IFilterFactory;
            if (filterFactory == null)
            {
                filterItem.Filter = filter;
            }
            else
            {
                filterItem.Filter = filterFactory.CreateInstance(ServiceProvider);

                if (filterItem.Filter == null)
                {
                    throw new InvalidOperationException(Resources.FormatTypeMethodMustReturnNotNullValue(
                        "CreateInstance",
                        typeof(IFilterFactory).Name));
                }

                ApplyFilterToContainer(filterItem.Filter, filterFactory);
            }
        }

        private void ApplyFilterToContainer(object actualFilter, IFilter filterMetadata)
        {
            Debug.Assert(actualFilter != null, "actualFilter should not be null");
            Debug.Assert(filterMetadata != null, "filterMetadata should not be null");

            var container = actualFilter as IFilterContainer;

            if (container != null)
            {
                container.FilterDefinition = filterMetadata;
            }
        }

        private class FilterDescriptorCorsComparer : IComparer<FilterDescriptor>
        {
            private static readonly FilterDescriptorCorsComparer _comparer = new FilterDescriptorCorsComparer();

            public static FilterDescriptorCorsComparer Comparer
            {
                get { return _comparer; }
            }

            public int Compare([NotNull]FilterDescriptor x, [NotNull]FilterDescriptor y)
            {
                var isThisCorsFilter = x.Filter is ICorsAuthorizationFilter;
                var isOtherCorsFilter = y.Filter is ICorsAuthorizationFilter;
                if (isThisCorsFilter && !isOtherCorsFilter)
                {
                    return -1;
                }

                if (isOtherCorsFilter && !isThisCorsFilter)
                {
                    return 1;
                }

                return 0;
            }
        }
    }
}
