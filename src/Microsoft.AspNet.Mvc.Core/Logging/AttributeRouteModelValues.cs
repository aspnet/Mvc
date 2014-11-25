// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.Logging;
using Microsoft.AspNet.Mvc.ApplicationModels;

namespace Microsoft.AspNet.Mvc.Logging
{
    public class AttributeRouteModelValues : LoggerStructureBase
    {
        public AttributeRouteModelValues([NotNull] AttributeRouteModel inner)
        {
            Template = inner.Template;
            Order = inner.Order;
            Name = inner.Name;
            IsAbsoluteTemplate = inner.IsAbsoluteTemplate;
        }

        public string Template { get; set; }

        public int? Order { get; set; }

        public string Name { get; set; }

        public bool IsAbsoluteTemplate { get; set; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}