// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Routing;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    public class AttributeRouteInfoValues : LoggerStructureBase
    {
        public AttributeRouteInfoValues(AttributeRouteInfo inner)
        {
            Template = inner?.Template;
            Order = inner?.Order;
            Name = inner?.Name;
        }

        public string Template { get; set; }

        public int? Order { get; set; }

        public string Name { get; set; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}