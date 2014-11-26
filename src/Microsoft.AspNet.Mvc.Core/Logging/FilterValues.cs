using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    /// <summary>
    /// Represents an <see cref="IFilter"/>. Logged as a component of <see cref="FilterDescriptorValues"/>,
    /// and as a substructure of <see cref="ControllerModelValues"/> and <see cref="ActionModelValues"/>.
    /// </summary>
    public class FilterValues : LoggerStructureBase
    {
        public FilterValues(IFilter inner)
        {
            if (inner is IFilterFactory)
            {
                IsFactory = true;
                if (inner is ServiceFilterAttribute)
                {
                    FilterType = ((ServiceFilterAttribute)inner).ServiceType;
                }
                else if (inner is TypeFilterAttribute)
                {
                    FilterType = ((TypeFilterAttribute)inner).ImplementationType;
                }
                if (FilterType != null)
                {
                    FilterInterfaces = FilterType.GetInterfaces().ToList();
                }
            }
            FilterMetadataType = inner.GetType();
        }

        public bool IsFactory { get; }

        public Type FilterMetadataType { get; }

        public Type FilterType { get; }

        public List<Type> FilterInterfaces { get; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}