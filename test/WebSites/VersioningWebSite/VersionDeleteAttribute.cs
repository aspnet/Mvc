// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Routing;

namespace VersioningWebSite
{
    public class VersionDeleteAttribute : VersionRouteAttribute, IActionHttpMethodProvider
    {
        public VersionDeleteAttribute(string template)
            : base(template)
        {
        }

        public VersionDeleteAttribute(string template, string versionRange)
            : base(template, versionRange)
        {
        }

        private readonly IEnumerable<string> _httpMethods = new[] { "DELETE" };

        public IEnumerable<string> HttpMethods
        {
            get
            {
                return _httpMethods;
            }
        }
    }
}