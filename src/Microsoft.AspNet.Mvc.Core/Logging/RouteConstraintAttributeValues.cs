// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    /// <summary>
    /// Represents a <see cref="RouteConstraintAttribute"/>. Logged as a substructure of
    /// <see cref="ControllerModelValues"/>
    /// </summary>
    public class RouteConstraintAttributeValues : LoggerStructureBase
    {
        public RouteConstraintAttributeValues([NotNull] RouteConstraintAttribute inner)
        {
            RouteKey = inner.RouteKey;
            RouteValue = inner.RouteValue;
            RouteKeyHandling = inner.RouteKeyHandling;
            BlockNonAttributedActions = inner.BlockNonAttributedActions;
        }

        public string RouteKey { get; }

        public string RouteValue { get; }

        public RouteKeyHandling RouteKeyHandling { get; }

        public bool BlockNonAttributedActions { get; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}