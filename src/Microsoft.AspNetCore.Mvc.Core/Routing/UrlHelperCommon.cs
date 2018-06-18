// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Mvc.Routing
{
    internal class UrlHelperCommon
    {
        // Perf: Share the StringBuilder object across multiple calls of GenerateURL for this UrlHelper
        private StringBuilder _stringBuilder;

        // Perf: Reuse the RouteValueDictionary across multiple calls of Action for this UrlHelper
        private readonly RouteValueDictionary _routeValueDictionary;

        private readonly HttpContext _httpContext;

        public UrlHelperCommon(HttpContext httpContext)
        {
            _httpContext = httpContext;
            _routeValueDictionary = new RouteValueDictionary();
        }

        public bool IsLocalUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            // Allows "/" or "/foo" but not "//" or "/\".
            if (url[0] == '/')
            {
                // url is exactly "/"
                if (url.Length == 1)
                {
                    return true;
                }

                // url doesn't start with "//" or "/\"
                if (url[1] != '/' && url[1] != '\\')
                {
                    return true;
                }

                return false;
            }

            // Allows "~/" or "~/foo" but not "~//" or "~/\".
            if (url[0] == '~' && url.Length > 1 && url[1] == '/')
            {
                // url is exactly "~/"
                if (url.Length == 2)
                {
                    return true;
                }

                // url doesn't start with "~//" or "~/\"
                if (url[2] != '/' && url[2] != '\\')
                {
                    return true;
                }

                return false;
            }

            return false;
        }

        public void AppendPathAndFragment(StringBuilder builder, string virtualPath, string fragment)
        {
            var pathBase = _httpContext.Request.PathBase;
            if (!pathBase.HasValue)
            {
                if (virtualPath.Length == 0)
                {
                    builder.Append("/");
                }
                else
                {
                    if (!virtualPath.StartsWith("/", StringComparison.Ordinal))
                    {
                        builder.Append("/");
                    }

                    builder.Append(virtualPath);
                }
            }
            else
            {
                if (virtualPath.Length == 0)
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

                    if (!virtualPath.StartsWith("/", StringComparison.Ordinal))
                    {
                        builder.Append("/");
                    }

                    builder.Append(virtualPath);
                }
            }

            if (!string.IsNullOrEmpty(fragment))
            {
                builder.Append("#").Append(fragment);
            }
        }

        public string Content(string contentPath)
        {
            if (string.IsNullOrEmpty(contentPath))
            {
                return null;
            }
            else if (contentPath[0] == '~')
            {
                var segment = new PathString(contentPath.Substring(1));
                var applicationPath = _httpContext.Request.PathBase;

                return applicationPath.Add(segment).Value;
            }

            return contentPath;
        }

        public RouteValueDictionary GetValuesDictionary(object values)
        {
            // Perf: RouteValueDictionary can be cast to IDictionary<string, object>, but it is
            // special cased to avoid allocating boxed Enumerator.
            if (values is RouteValueDictionary routeValuesDictionary)
            {
                _routeValueDictionary.Clear();
                foreach (var kvp in routeValuesDictionary)
                {
                    _routeValueDictionary.Add(kvp.Key, kvp.Value);
                }

                return _routeValueDictionary;
            }

            if (values is IDictionary<string, object> dictionaryValues)
            {
                _routeValueDictionary.Clear();
                foreach (var kvp in dictionaryValues)
                {
                    _routeValueDictionary.Add(kvp.Key, kvp.Value);
                }

                return _routeValueDictionary;
            }

            return new RouteValueDictionary(values);
        }

        public string GenerateUrl(string protocol, string host, string virtualPath, string fragment)
        {
            if (virtualPath == null)
            {
                return null;
            }

            // Perf: In most of the common cases, GenerateUrl is called with a null protocol, host and fragment.
            // In such cases, we might not need to build any URL as the url generated is mostly same as the virtual path available in pathData.
            // For such common cases, this FastGenerateUrl method saves a string allocation per GenerateUrl call.
            string url;
            if (TryFastGenerateUrl(protocol, host, virtualPath, fragment, out url))
            {
                return url;
            }

            var builder = GetStringBuilder();
            try
            {
                if (string.IsNullOrEmpty(protocol) && string.IsNullOrEmpty(host))
                {
                    AppendPathAndFragment(builder, virtualPath, fragment);
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

                    host = string.IsNullOrEmpty(host) ? _httpContext.Request.Host.Value : host;
                    builder.Append(host);
                    AppendPathAndFragment(builder, virtualPath, fragment);
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
            string virtualPath,
            string fragment,
            out string url)
        {
            var pathBase = _httpContext.Request.PathBase;
            url = null;

            if (string.IsNullOrEmpty(protocol)
                && string.IsNullOrEmpty(host)
                && string.IsNullOrEmpty(fragment)
                && !pathBase.HasValue)
            {
                if (virtualPath.Length == 0)
                {
                    url = "/";
                    return true;
                }
                else if (virtualPath.StartsWith("/", StringComparison.Ordinal))
                {
                    url = virtualPath;
                    return true;
                }
            }

            return false;
        }

        private StringBuilder GetStringBuilder()
        {
            if (_stringBuilder == null)
            {
                _stringBuilder = new StringBuilder();
            }

            return _stringBuilder;
        }
    }
}
