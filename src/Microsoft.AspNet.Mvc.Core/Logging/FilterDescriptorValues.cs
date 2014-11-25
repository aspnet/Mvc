// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    public class FilterDescriptorValues : LoggerStructureBase
    {
        public FilterDescriptorValues(FilterDescriptor inner)
        {
            if (inner.Filter is IFilterFactory)
            {
                IsFactory = true;
                if (inner.Filter is ServiceFilterAttribute)
                {
                    FilterType = ((ServiceFilterAttribute)inner.Filter).ServiceType;
                }
                else if (inner.Filter is TypeFilterAttribute)
                {
                    FilterType = ((TypeFilterAttribute)inner.Filter).ImplementationType;
                }
                if (FilterType != null)
                {
                    FilterInterfaces = FilterType.GetInterfaces().ToList();
                }
            }
            FilterMetadataType = inner.Filter.GetType();
            Order = inner.Order;
            Scope = inner.Scope;
        }

        public bool IsFactory { get; set; }

        public Type FilterMetadataType { get; set; }

        public Type FilterType { get; set; }

        public List<Type> FilterInterfaces { get; set; }

        public int Order { get; set; }

        public int Scope { get; set; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}