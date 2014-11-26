// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    /// <summary>
    /// Represents the state of a <see cref="RouteDataActionConstraint"/>. Logged as a substructure of
    /// <see cref="ActionDescriptorValues"/>.
    /// </summary>
    public class RouteDataActionConstraintValues : LoggerStructureBase
    {
        public RouteDataActionConstraintValues([NotNull] RouteDataActionConstraint inner)
        {
            RouteKey = inner.RouteKey;
            RouteValue = inner.RouteValue;
            KeyHandling = inner.KeyHandling;
        }

        public string RouteKey { get; }

        public string RouteValue { get; }

        public RouteKeyHandling KeyHandling { get; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}