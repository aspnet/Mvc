// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Razor Page specific extensions for <see cref="IUrlHelper"/>.
    /// </summary>
    public static class PageUrlHelperExtensions
    {
        /// <summary>
        /// Generates a URL with an absolute path for the specified <paramref name="page"/>.
        /// </summary>
        /// <param name="urlHelper">The <see cref="IUrlHelper"/>.</param>
        /// <param name="page">The page to generate the url for.</param>
        /// <returns>The generated URL.</returns>
        public static string Page(this IUrlHelper urlHelper, string page)
            => Page(urlHelper, page, values: null);

        /// <summary>
        /// Generates a URL with an absolute path for the specified <paramref name="page"/>.
        /// </summary>
        /// <param name="urlHelper">The <see cref="IUrlHelper"/>.</param>
        /// <param name="page">The page to generate the url for.</param>
        /// <param name="values">An object that contains route values.</param>
        /// <returns>The generated URL.</returns>
        public static string Page(
            this IUrlHelper urlHelper,
            string page,
            object values)
            => Page(urlHelper, page, values, protocol: null);

        /// <summary>
        /// Generates a URL with an absolute path for the specified <paramref name="page"/>.
        /// </summary>
        /// <param name="urlHelper">The <see cref="IUrlHelper"/>.</param>
        /// <param name="page">The page to generate the url for.</param>
        /// <param name="values">An object that contains route values.</param>
        /// <param name="protocol">The protocol for the URL, such as "http" or "https".</param>
        /// <returns>The generated URL.</returns>
        public static string Page(
            this IUrlHelper urlHelper,
            string page,
            object values,
            string protocol)
            => Page(urlHelper, page, values, protocol, host: null, fragment: null);

        /// <summary>
        /// Generates a URL with an absolute path for the specified <paramref name="page"/>.
        /// </summary>
        /// <param name="urlHelper">The <see cref="IUrlHelper"/>.</param>
        /// <param name="page">The page to generate the url for.</param>
        /// <param name="values">An object that contains route values.</param>
        /// <param name="protocol">The protocol for the URL, such as "http" or "https".</param>
        /// <param name="host">The host name for the URL.</param>
        /// <returns>The generated URL.</returns>
        public static string Page(
            this IUrlHelper urlHelper,
            string page,
            object values,
            string protocol,
            string host)
            => Page(urlHelper, page, values, protocol, host, fragment: null);

        /// <summary>
        /// Generates a URL with an absolute path for the specified <paramref name="page"/>.
        /// </summary>
        /// <param name="urlHelper">The <see cref="IUrlHelper"/>.</param>
        /// <param name="page">The page to generate the url for.</param>
        /// <param name="values">An object that contains route values.</param>
        /// <param name="protocol">The protocol for the URL, such as "http" or "https".</param>
        /// <param name="host">The host name for the URL.</param>
        /// <param name="fragment">The fragment for the URL.</param>
        /// <returns>The generated URL.</returns>
        public static string Page(
            this IUrlHelper urlHelper,
            string page,
            object values,
            string protocol,
            string host,
            string fragment)
        {
            if (urlHelper == null)
            {
                throw new ArgumentNullException(nameof(urlHelper));
            }

            var routeValues = new RouteValueDictionary(values);
            if (page == null)
            {
                var ambientValues = urlHelper.ActionContext.RouteData.Values;
                if (!routeValues.ContainsKey("page") &&
                    ambientValues.TryGetValue("page", out var value))
                {
                    routeValues["page"] = value;
                }
            }
            else
            {
                routeValues["page"] = page;
            }

            return urlHelper.RouteUrl(
                routeName: null,
                values: routeValues,
                protocol: protocol,
                host: host,
                fragment: fragment);
        }
    }
}
