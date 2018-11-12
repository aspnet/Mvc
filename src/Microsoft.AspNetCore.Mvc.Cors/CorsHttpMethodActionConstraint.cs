// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Mvc.Cors
{
    // Don't casually change the name of this. We reference the full type name in ActionConstraintCache.
    internal class CorsHttpMethodActionConstraint : HttpMethodActionConstraint
    {
        private readonly string OriginHeader = "Origin";
        private readonly string AccessControlRequestMethod = "Access-Control-Request-Method";
        private readonly string PreflightHttpMethod = "OPTIONS";

        public CorsHttpMethodActionConstraint(HttpMethodActionConstraint constraint)
            : base(constraint.HttpMethods)
        {
        }

        public override bool Accept(ActionConstraintContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var methods = (ReadOnlyCollection<string>)HttpMethods;
            if (methods.Count == 0)
            {
                return true;
            }

            var request = context.RouteContext.HttpContext.Request;
            // Perf: Check http method before accessing the Headers collection.
            if (string.Equals(request.Method, PreflightHttpMethod, StringComparison.OrdinalIgnoreCase) &&
                request.Headers.ContainsKey(OriginHeader) &&
                request.Headers.TryGetValue(AccessControlRequestMethod, out var accessControlRequestMethod) &&
                !StringValues.IsNullOrEmpty(accessControlRequestMethod))
            {
                for (var i = 0; i < methods.Count; i++)
                {
                    var supportedMethod = methods[i];
                    if (string.Equals(supportedMethod, accessControlRequestMethod, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }

            return base.Accept(context);
        }
    }
}
