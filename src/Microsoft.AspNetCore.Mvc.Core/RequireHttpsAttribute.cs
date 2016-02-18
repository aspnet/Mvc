// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNetCore.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class RequireHttpsAttribute : Attribute, IAuthorizationFilter, IOrderedFilter
    {
        public int Order { get; set; }

        public virtual void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException(nameof(filterContext));
            }

            if (!filterContext.HttpContext.Request.IsHttps)
            {
                HandleNonHttpsRequest(filterContext);
            }
        }

        protected virtual void HandleNonHttpsRequest(AuthorizationFilterContext filterContext)
        {
            // only redirect for GET requests, otherwise the browser might not propagate the verb and request
            // body correctly.
            if (!string.Equals(filterContext.HttpContext.Request.Method, "GET", StringComparison.OrdinalIgnoreCase))
            {
                filterContext.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            }
            else
            {
                var request = filterContext.HttpContext.Request;
                var newUrl = string.Concat(
                    "https://",
                    request.Host.ToUriComponent(),
                    request.PathBase.ToUriComponent(),
                    request.Path.ToUriComponent(),
                    request.QueryString.ToUriComponent());

                // redirect to HTTPS version of page
                filterContext.Result = new RedirectResult(newUrl, permanent: true);
            }
        }
    }
}
