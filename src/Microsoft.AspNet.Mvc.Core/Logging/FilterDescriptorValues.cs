// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    /// <summary>
    /// Represents the state of a <see cref="FilterDescriptor"/>. Logged as a substructure of
    /// <see cref="ActionDescriptorValues"/>.
    /// </summary>
    public class FilterDescriptorValues : LoggerStructureBase
    {
        public FilterDescriptorValues(FilterDescriptor inner)
        {
            Filter = new FilterValues(inner.Filter);
            Order = inner.Order;
            Scope = inner.Scope;
        }

        public FilterValues Filter { get; }

        public int Order { get; }

        public int Scope { get; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}