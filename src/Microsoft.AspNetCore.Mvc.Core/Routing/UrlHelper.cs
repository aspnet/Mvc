// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Mvc.Routing
{
    /// <summary>
    /// An implementation of <see cref="IUrlHelper"/> that contains methods to
    /// build URLs for ASP.NET MVC within an application.
    /// </summary>
    public class UrlHelper : IUrlHelper
    {

        // Perf: Share the StringBuilder object across multiple calls of GenerateURL for this UrlHelper
        private StringBuilder _stringBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlHelper"/> class using the specified action context and
        /// action selector.
        /// </summary>
        /// <param name="actionContext">The <see cref="Mvc.ActionContext"/> for the current request.</param>
        public UrlHelper(ActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }

            ActionContext = actionContext;
        }

        /// <inheritdoc />
        public ActionContext ActionContext { get; }

        protected RouteValueDictionary AmbientValues => ActionContext.RouteData.Values;

        protected HttpContext HttpContext => ActionContext.HttpContext;

        protected IRouter Router => ActionContext.RouteData.Routers[0];

        /// <inheritdoc />
        public virtual string Action(UrlActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }

            var valuesDictionary = new RouteValueDictionary(actionContext.Values);

            if (actionContext.Action == null)
            {
                object action;
                if (!valuesDictionary.ContainsKey("action") &&
                    AmbientValues.TryGetValue("action", out action))
                {
                    valuesDictionary["action"] = action;
                }
            }
            else
            {
                valuesDictionary["action"] = actionContext.Action;
            }

            if (actionContext.Controller == null)
            {
                object controller;
                if (!valuesDictionary.ContainsKey("controller") &&
                    AmbientValues.TryGetValue("controller", out controller))
                {
                    valuesDictionary["controller"] = controller;
                }
            }
            else
            {
                valuesDictionary["controller"] = actionContext.Controller;
            }

            var virtualPathData = GetVirtualPathData(routeName: null, values: valuesDictionary);
            return GenerateUrl(actionContext.Protocol, actionContext.Host, virtualPathData, actionContext.Fragment);
        }

        /// <inheritdoc />
        public virtual bool IsLocalUrl(string url)
        {
            return
                !string.IsNullOrEmpty(url) &&

                // Allows "/" or "/foo" but not "//" or "/\".
                ((url[0] == '/' && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'))) ||

                // Allows "~/" or "~/foo".
                (url.Length > 1 && url[0] == '~' && url[1] == '/'));
        }

        /// <inheritdoc />
        public virtual string RouteUrl(UrlRouteContext routeContext)
        {
            if (routeContext == null)
            {
                throw new ArgumentNullException(nameof(routeContext));
            }

            var valuesDictionary = new RouteValueDictionary(routeContext.Values);
            var virtualPathData = GetVirtualPathData(routeContext.RouteName, valuesDictionary);
            return GenerateUrl(routeContext.Protocol, routeContext.Host, virtualPathData, routeContext.Fragment);
        }

        /// <summary>
        /// Gets the <see cref="VirtualPathData"/> for the specified route values by using the specified route name.
        /// </summary>
        /// <param name="routeName">The name of the route that is used to generate the <see cref="VirtualPathData"/>.
        /// </param>
        /// <param name="values">A dictionary that contains the parameters for a route.</param>
        /// <returns>The <see cref="VirtualPathData"/>.</returns>
        protected virtual VirtualPathData GetVirtualPathData(string routeName, RouteValueDictionary values)
        {
            var context = new VirtualPathContext(HttpContext, AmbientValues, values, routeName);
            return Router.GetVirtualPath(context);
        }

        // Internal for unit testing.
        internal void AppendPathAndFragment(StringBuilder builder, VirtualPathData pathData, string fragment)
        {
            var pathBase = HttpContext.Request.PathBase;

            if (!pathBase.HasValue)
            {
                if (pathData.VirtualPath.Length == 0)
                {
                    builder.Append("/");
                }
                else
                {
                    if (!pathData.VirtualPath.StartsWith("/", StringComparison.Ordinal))
                    {
                        builder.Append("/");
                    }

                    builder.Append(pathData.VirtualPath);
                }
            }
            else
            {
                if (pathData.VirtualPath.Length == 0)
                {
                    builder.Append(pathBase.Value);
                }
                else
                {
                    builder.Append(pathBase.Value);

                    if (pathBase.Value.EndsWith("/", StringComparison.Ordinal))
                    {
                        builder.Length--;
                    }

                    if (!pathData.VirtualPath.StartsWith("/", StringComparison.Ordinal))
                    {
                        builder.Append("/");
                    }

                    builder.Append(pathData.VirtualPath);
                }
            }

            if (!string.IsNullOrEmpty(fragment))
            {
                builder.Append("#").Append(fragment);
            }
        }

        /// <inheritdoc />
        public virtual string Content(string contentPath)
        {
            if (string.IsNullOrEmpty(contentPath))
            {
                return null;
            }
            else if (contentPath[0] == '~')
            {
                var segment = new PathString(contentPath.Substring(1));
                var applicationPath = HttpContext.Request.PathBase;

                return applicationPath.Add(segment).Value;
            }

            return contentPath;
        }

        /// <inheritdoc />
        public virtual string Link(string routeName, object values)
        {
            return RouteUrl(new UrlRouteContext()
            {
                RouteName = routeName,
                Values = values,
                Protocol = HttpContext.Request.Scheme,
                Host = HttpContext.Request.Host.ToUriComponent()
            });
        }

        private StringBuilder GetStringBuilder()
        {
            if(_stringBuilder == null)
            {
                _stringBuilder = new StringBuilder();
            }

            return _stringBuilder;
        }

        /// <summary>
        /// Generates the URL using the specified components.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        /// <param name="host">The host.</param>
        /// <param name="pathData">The <see cref="VirtualPathData"/>.</param>
        /// <param name="fragment">The URL fragment.</param>
        /// <returns>The generated URL.</returns>
        internal virtual string GenerateUrl(string protocol, string host, VirtualPathData pathData, string fragment)
        {
            if (pathData == null)
            {
                return null;
            }

            // VirtualPathData.VirtualPath returns string.Empty instead of null.
            Debug.Assert(pathData.VirtualPath != null);

            // Perf: In most of the common cases, GenerateUrl is called with a null protocol, host and fragment. 
            // In such cases, we might not need to build any URL as the url generated is mostly same as the virtual path available in pathData.
            // For such common cases, this FastGenerateUrl method saves a string allocation per GenerateUrl call.
            string url;
            if (TryFastGenerateUrl(protocol, host, pathData, fragment, out url))
            {
                return url;
            }

            var builder = GetStringBuilder();
            try
            {
                if (string.IsNullOrEmpty(protocol) && string.IsNullOrEmpty(host))
                {
                    AppendPathAndFragment(builder, pathData, fragment);
                    // We're returning a partial URL (just path + query + fragment), but we still want it to be rooted.
                    if (builder.Length == 0 || builder[0] != '/')
                    {
                        builder.Insert(0, '/');
                    }
                }
                else
                {
                    protocol = string.IsNullOrEmpty(protocol) ? "http" : protocol;
                    builder.Append(protocol);

                    builder.Append("://");

                    host = string.IsNullOrEmpty(host) ? HttpContext.Request.Host.Value : host;
                    builder.Append(host);
                    AppendPathAndFragment(builder, pathData, fragment);
                }

                var path = builder.ToString();
                return path;
            }
            finally
            {
                // Clear the StringBuilder so that it can reused for the next call.
                builder.Clear();
            }
        }

        private bool TryFastGenerateUrl(
            string protocol, 
            string host, 
            VirtualPathData pathData, 
            string fragment,
            out string url)
        {
            var pathBase = HttpContext.Request.PathBase;
            url = null;

            if (string.IsNullOrEmpty(protocol) 
                && string.IsNullOrEmpty(host) 
                && string.IsNullOrEmpty(fragment)
                && !pathBase.HasValue)
            {
                if (pathData.VirtualPath.Length == 0)
                {
                    url = "/";
                }
                else if (pathData.VirtualPath.StartsWith("/", StringComparison.Ordinal))
                {
                    url = pathData.VirtualPath;
                }
            }

            return (url != null);
        }
    }
}