// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Routing;

namespace VersioningWebSite
{
    public class RangeVersionValidator : IActionConstraint
    {
        private readonly int? _minVersion;
        private readonly int? _maxVersion;

        public int Order
        {
            get
            {
                return 0;
            }
        }

        public RangeVersionValidator(int? minVersion, int? maxVersion)
        {
            _minVersion = minVersion;
            _maxVersion = maxVersion;
        }

        public static string GetVersion(HttpRequest request)
        {
            return request.Query.Get("version");
        }

        private bool Accept(RouteContext context)
        {
            int version;
            if (int.TryParse(GetVersion(context.HttpContext.Request), out version))
            {
                return (_minVersion == null || _minVersion <= version) &&
                    (_maxVersion == null || _maxVersion >= version);
            }
            else
            {
                return false;
            }
        }

        public bool Accept(ActionConstraintContext context)
        {
            return Accept(context.RouteContext);
        }
    }
}