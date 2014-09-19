// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc;

namespace VersioningWebSite
{
    public class VersionRoute : RouteAttribute, IActionConstraintFactory
    {
        private readonly IActionConstraint _constraint;

        public VersionRoute(string template)
            : base(template)
        {
        }

        public VersionRoute(string template, string minVersion, string maxVersion)
            : base(template)
        {
            int? parsedMinVersion = null;
            int? parsedMaxVersion = null;

            if (minVersion != null)
            {
                parsedMinVersion = int.Parse(minVersion);
            }

            if (maxVersion != null)
            {
                parsedMaxVersion = int.Parse(maxVersion);
            }

            _constraint = new RangeVersionValidator(parsedMinVersion, parsedMaxVersion);
        }

        public IActionConstraint CreateInstance(IServiceProvider services)
        {
            return _constraint;
        }
    }
}