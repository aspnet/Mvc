// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Rendering
{
    public static class HtmlHelperLinkExtensions
    {
        public static HtmlString ActionLink(
            [NotNull] this IHtmlHelper helper,
            [NotNull] string linkText,
            string actionName)
        {
            return helper.ActionLink(
                linkText,
                actionName,
                controllerName: null,
                protocol: null,
                hostname: null,
                fragment: null,
                routeValues: null,
                htmlAttributes: null);
        }

        public static HtmlString ActionLink(
            [NotNull] this IHtmlHelper helper,
            [NotNull] string linkText,
            string actionName,
            object routeValues)
        {
            return helper.ActionLink(
                linkText,
                actionName,
                controllerName: null,
                protocol: null,
                hostname: null,
                fragment: null,
                routeValues: routeValues,
                htmlAttributes: null);
        }

        public static HtmlString ActionLink(
            [NotNull] this IHtmlHelper helper,
            [NotNull] string linkText,
            string actionName,
            object routeValues,
            object htmlAttributes)
        {
            return helper.ActionLink(
                linkText,
                actionName,
                controllerName: null,
                protocol: null,
                hostname: null,
                fragment: null,
                routeValues: routeValues,
                htmlAttributes: htmlAttributes);
        }

        public static HtmlString ActionLink(
            [NotNull] this IHtmlHelper helper,
            [NotNull] string linkText,
            string actionName,
            string controllerName)
        {
            return helper.ActionLink(
                linkText,
                actionName,
                controllerName,
                protocol: null,
                hostname: null,
                fragment: null,
                routeValues: null,
                htmlAttributes: null);
        }

        public static HtmlString ActionLink(
            [NotNull] this IHtmlHelper helper,
            [NotNull] string linkText,
            string actionName,
            string controllerName,
            object routeValues)
        {
            return helper.ActionLink(
                linkText,
                actionName,
                controllerName,
                protocol: null,
                hostname: null,
                fragment: null,
                routeValues: routeValues,
                htmlAttributes: null);
        }

        public static HtmlString ActionLink(
            [NotNull] this IHtmlHelper helper,
            [NotNull] string linkText,
            string actionName,
            string controllerName,
            object routeValues,
            object htmlAttributes)
        {
            return helper.ActionLink(
                linkText,
                actionName,
                controllerName,
                protocol: null,
                hostname: null,
                fragment: null,
                routeValues: routeValues,
                htmlAttributes: htmlAttributes);
        }

        public static HtmlString RouteLink(
            [NotNull] this IHtmlHelper htmlHelper,
            [NotNull] string linkText,
            object routeValues)
        {
            return htmlHelper.RouteLink(
                                linkText,
                                routeName: null,
                                protocol: null,
                                hostName: null,
                                fragment: null,
                                routeValues: routeValues,
                                htmlAttributes: null);
        }

        public static HtmlString RouteLink(
            [NotNull] this IHtmlHelper htmlHelper,
            [NotNull] string linkText,
            string routeName)
        {
            return htmlHelper.RouteLink(
                                linkText,
                                routeName,
                                protocol: null,
                                hostName: null,
                                fragment: null,
                                routeValues: null,
                                htmlAttributes: null);
        }

        public static HtmlString RouteLink(
            [NotNull] this IHtmlHelper htmlHelper,
            [NotNull] string linkText,
            string routeName,
            object routeValues)
        {
            return htmlHelper.RouteLink(
                                linkText,
                                routeName,
                                protocol: null,
                                hostName: null,
                                fragment: null,
                                routeValues: routeValues,
                                htmlAttributes: null);
        }

        public static HtmlString RouteLink(
            [NotNull] this IHtmlHelper htmlHelper,
            [NotNull] string linkText,
            object routeValues,
            object htmlAttributes)
        {
            return htmlHelper.RouteLink(
                                linkText,
                                routeName: null,
                                protocol: null,
                                hostName: null,
                                fragment: null,
                                routeValues: routeValues,
                                htmlAttributes: htmlAttributes);
        }

        public static HtmlString RouteLink(
            [NotNull] this IHtmlHelper htmlHelper,
            [NotNull] string linkText,
            string routeName,
            object routeValues,
            object htmlAttributes)
        {
            return htmlHelper.RouteLink(
                                 linkText,
                                 routeName,
                                 protocol: null,
                                 hostName: null,
                                 fragment: null,
                                 routeValues: routeValues,
                                 htmlAttributes: htmlAttributes);
        }
    }
}
