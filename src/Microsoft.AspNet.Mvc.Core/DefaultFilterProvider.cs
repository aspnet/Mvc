// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.Filters
{
    public class DefaultFilterProvider : IFilterProvider
    {
        public int Order
        {
            get { return DefaultOrder.DefaultFrameworkSortOrder; }
        }

        /// <inheritdoc />
        public void OnProvidersExecuting([NotNull] FilterProviderContext context)
        {
            if (context.ActionContext.ActionDescriptor.FilterDescriptors != null)
            {
                foreach (var item in context.Results)
                {
                    ProvideFilter(context, item);
                }
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
                var services = context.ActionContext.HttpContext.RequestServices;
                filterItem.Filter = filterFactory.CreateInstance(services);

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
    }
}
