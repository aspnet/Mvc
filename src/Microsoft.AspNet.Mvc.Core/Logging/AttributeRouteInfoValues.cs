// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Routing;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    /// <summary>
    /// Represents the state of a <see cref="AttributeRouteInfo"/>. Logged as a substructure of
    /// <see cref="ActionDescriptorValues"/>, this contains the template, order, and name of the
    /// given <see cref="AttributeRouteInfo"/>.
    /// </summary>
    public class AttributeRouteInfoValues : LoggerStructureBase
    {
        public AttributeRouteInfoValues(AttributeRouteInfo inner)
        {
            Template = inner?.Template;
            Order = inner?.Order;
            Name = inner?.Name;
        }

        public string Template { get; }

        public int? Order { get; }

        public string Name { get; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}