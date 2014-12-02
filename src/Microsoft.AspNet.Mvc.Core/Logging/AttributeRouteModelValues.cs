// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.Logging;
using Microsoft.AspNet.Mvc.ApplicationModels;

namespace Microsoft.AspNet.Mvc.Logging
{
    /// <summary>
    /// Represents the state of a <see cref="AttributeRouteModel"/>. Logged as a substructure of
    /// <see cref="ControllerModelValues"/>, this contains the template, order and scope of the
    /// <see cref="AttributeRouteModel"/> and a flag to signify if it is an absolute template
    /// </summary>
    public class AttributeRouteModelValues : LoggerStructureBase
    {
        public AttributeRouteModelValues([NotNull] AttributeRouteModel inner)
        {
            Template = inner.Template;
            Order = inner.Order;
            Name = inner.Name;
            IsAbsoluteTemplate = inner.IsAbsoluteTemplate;
        }

        public string Template { get; }

        public int? Order { get; }

        public string Name { get; }

        public bool IsAbsoluteTemplate { get; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}