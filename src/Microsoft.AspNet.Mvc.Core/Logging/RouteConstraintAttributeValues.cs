// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    public class RouteConstraintAttributeValues : LoggerStructureBase
    {
        public RouteConstraintAttributeValues([NotNull] RouteConstraintAttribute inner)
        {
            RouteKey = inner.RouteKey;
            RouteValue = inner.RouteValue;
            RouteKeyHandling = inner.RouteKeyHandling;
            BlockNonAttributedActions = inner.BlockNonAttributedActions;
        }

        public string RouteKey { get; set; }

        public string RouteValue { get; set; }

        public RouteKeyHandling RouteKeyHandling { get; set; }

        public bool BlockNonAttributedActions { get; set; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}