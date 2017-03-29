// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Mvc.Rendering
{
    /// <summary>
    /// Razor Page specific extensions for <see cref="IHtmlHelper"/>.
    /// </summary>
    public static class PageHtmlHelperExtensions
    {
        /// <summary>
        /// Returns an anchor (&lt;a&gt;) element that contains a URL path to the specified route.
        /// </summary>
        /// <param name="htmlHelper">The <see cref="IHtmlHelper"/> instance this method extends.</param>
        /// <param name="linkText">The inner text of the anchor element..</param>
        /// <param name="page">The name of the page to generate the link for.</param>
        /// <returns>A new <see cref="IHtmlContent"/> containing the anchor element.</returns>
        public static IHtmlContent PageLink(this IHtmlHelper htmlHelper, string linkText, string page)
            => PageLink(htmlHelper, linkText, page, routeValues: null);

        /// <summary>
        /// Returns an anchor (&lt;a&gt;) element that contains a URL path to the specified route.
        /// </summary>
        /// <param name="htmlHelper">The <see cref="IHtmlHelper"/> instance this method extends.</param>
        /// <param name="linkText">The inner text of the anchor element..</param>
        /// <param name="page">The name of the page to generate the link for.</param>
        /// <param name="routeValues">
        /// An <see cref="object"/> that contains the parameters for a route. The parameters are retrieved through
        /// reflection by examining the properties of the <see cref="object"/>. This <see cref="object"/> is typically
        /// created using <see cref="object"/> initializer syntax. Alternatively, an
        /// <see cref="System.Collections.Generic.IDictionary{String, Object}"/> instance containing the route
        /// parameters.
        /// </param>
        /// <returns>A new <see cref="IHtmlContent"/> containing the anchor element.</returns>
        public static IHtmlContent PageLink(this IHtmlHelper htmlHelper, string linkText, string page, object routeValues)
            => PageLink(htmlHelper, linkText, page, routeValues, htmlAttributes: null);

        /// <summary>
        /// Returns an anchor (&lt;a&gt;) element that contains a URL path to the specified route.
        /// </summary>
        /// <param name="htmlHelper">The <see cref="IHtmlHelper"/> instance this method extends.</param>
        /// <param name="linkText">The inner text of the anchor element..</param>
        /// <param name="page">The name of the page to generate the link for.</param>
        /// <param name="routeValues">
        /// An <see cref="object"/> that contains the parameters for a route. The parameters are retrieved through
        /// reflection by examining the properties of the <see cref="object"/>. This <see cref="object"/> is typically
        /// created using <see cref="object"/> initializer syntax. Alternatively, an
        /// <see cref="System.Collections.Generic.IDictionary{String, Object}"/> instance containing the route
        /// parameters.
        /// </param>
        /// <param name="htmlAttributes">
        /// An <see cref="object"/> that contains the HTML attributes for the element. Alternatively, an
        /// <see cref="System.Collections.Generic.IDictionary{String, Object}"/> instance containing the HTML
        /// attributes.
        /// </param>
        /// <returns>A new <see cref="IHtmlContent"/> containing the anchor element.</returns>
        public static IHtmlContent PageLink(
            this IHtmlHelper htmlHelper,
            string linkText,
            string page,
            object routeValues,
            object htmlAttributes)
        {
            if (htmlHelper == null)
            {
                throw new ArgumentNullException(nameof(htmlHelper));
            }

            var routeValueDictionary = new RouteValueDictionary(routeValues)
            {
                { "page", page }
            };

            return htmlHelper.RouteLink(linkText, routeValueDictionary, htmlAttributes);
        }
    }
}
