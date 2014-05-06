// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    public class UrlHelper : IUrlHelper
    {
        private readonly HttpContext _httpContext;
        private readonly IRouter _router;
        private readonly IDictionary<string, object> _ambientValues;
        private readonly IActionSelector _actionSelector;

        public UrlHelper(IContextAccessor<ActionContext> contextAccessor, IActionSelector actionSelector)
        {
            _httpContext = contextAccessor.Value.HttpContext;
            _router = contextAccessor.Value.Router;
            _ambientValues = contextAccessor.Value.RouteValues;
            _actionSelector = actionSelector;
        }

        public string Action(string action, string controller, object values, string protocol, string host, string fragment)
        {
            var valuesDictionary = TypeHelper.ObjectToDictionary(values);

            if (action != null)
            {
                valuesDictionary["action"] = action;
            }

            if (controller != null)
            {
                valuesDictionary["controller"] = controller;
            }

            var context = new VirtualPathContext(_httpContext, _ambientValues, valuesDictionary);
            var actions = _actionSelector.GetCandidateActions(context);

            var actionCandidate = actions.FirstOrDefault();
            if (actionCandidate == null)
            {
                return null;
            }

            foreach (var constraint in actionCandidate.RouteConstraints)
            {
                if (constraint.KeyHandling == RouteKeyHandling.DenyKey &&
                    _ambientValues.ContainsKey(constraint.RouteKey))
                {
                    valuesDictionary[constraint.RouteKey] = null;
                }
            }

            var path = GeneratePathFromRoute(valuesDictionary);
            if (path == null)
            {
                return null;
            }

            return GenerateUrl(protocol, host, path, fragment);
        }

        public bool IsLocalUrl(string url)
        {
            return
                !string.IsNullOrEmpty(url) &&

                // Allows "/" or "/foo" but not "//" or "/\".
                ((url[0] == '/' && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'))) ||

                // Allows "~/" or "~/foo".
                (url.Length > 1 && url[0] == '~' && url[1] == '/'));
        }

        public string RouteUrl(string routeName, object values, string protocol, string host, string fragment)
        {
            var valuesDictionary = TypeHelper.ObjectToDictionary(values);
            var path = GeneratePathFromRoute(routeName, valuesDictionary);
            if (path == null)
            {
                return null;
            }

            return GenerateUrl(protocol, host, path, fragment);
        }

        private string GeneratePathFromRoute(IDictionary<string, object> values)
        {
            return GeneratePathFromRoute(routeName: null, values: values);
        }

        private string GeneratePathFromRoute(string routeName, IDictionary<string, object> values)
        {
            var context = new VirtualPathContext(_httpContext, _ambientValues, values, routeName);
            var path = _router.GetVirtualPath(context);
            if (path == null)
            {
                return null;
            }

            // See Routing Issue#31
            if (path.Length > 0 && path[0] != '/')
            {
                path = "/" + path;
            }

            var fullPath = _httpContext.Request.PathBase.Add(new PathString(path)).Value;
            if (fullPath.Length == 0)
            {
                return "/";
            }
            else
            {
                return fullPath;
            }
        }

        public string Content([NotNull] string contentPath)
        {
            return GenerateClientUrl(_httpContext.Request.PathBase, contentPath);
        }

        private static string GenerateClientUrl([NotNull] PathString applicationPath,
                                                [NotNull] string path)
        {
            if (path.StartsWith("~/", StringComparison.Ordinal))
            {
                var segment = new PathString(path.Substring(1));
                return applicationPath.Add(segment).Value;
            }
            return path;
        }

        private string GenerateUrl(string protocol, string host, string path, string fragment)
        {
            // We should have a robust and centrallized version of this code. See HttpAbstractions#28
            Contract.Assert(path != null);

            var url = path;
            if (!string.IsNullOrEmpty(fragment))
            {
                url += "#" + fragment;
            }

            if (string.IsNullOrEmpty(protocol) && string.IsNullOrEmpty(host))
            {
                // We're returning a partial url (just path + query + fragment), but we still want it
                // to be rooted.
                if (!url.StartsWith("/", StringComparison.Ordinal))
                {
                    url = "/" + url;
                }

                return url;
            }
            else
            {
                protocol = string.IsNullOrEmpty(protocol) ? "http" : protocol;
                host = string.IsNullOrEmpty(host) ? _httpContext.Request.Host.Value : host;

                url = protocol + "://" + host + url;
                return url;
            }
        }
    }
}