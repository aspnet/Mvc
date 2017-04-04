﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.RazorPages.Internal;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Mvc.RazorPages
{
    /// <summary>
    /// An <see cref="ActionResult"/> that returns a Found (302)
    /// or Moved Permanently (301) response with a Location header.
    /// Targets a registered route.
    /// </summary>
    public class RedirectToPageResult : ActionResult, IKeepTempDataResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToPageResult"/> with the values
        /// provided.
        /// </summary>
        /// <param name="pageName">The page to redirect to.</param>
        public RedirectToPageResult(string pageName)
            : this(pageName, routeValues: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToPageResult"/> with the values
        /// provided.
        /// </summary>
        /// <param name="pageName">The page to redirect to.</param>
        /// <param name="routeValues">The parameters for the route.</param>
        public RedirectToPageResult(string pageName, object routeValues)
            : this(pageName, routeValues, permanent: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToPageResult"/> with the values
        /// provided.
        /// </summary>
        /// <param name="pageName">The name of the route.</param>
        /// <param name="routeValues">The parameters for the route.</param>
        /// <param name="permanent">If set to true, makes the redirect permanent (301). Otherwise a temporary redirect is used (302).</param>
        public RedirectToPageResult(
            string pageName,
            object routeValues,
            bool permanent)
            : this(pageName, routeValues, permanent, fragment: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToPageResult"/> with the values
        /// provided.
        /// </summary>
        /// <param name="pageName">The name of the route.</param>
        /// <param name="routeValues">The parameters for the route.</param>
        /// <param name="fragment">The fragment to add to the URL.</param>
        public RedirectToPageResult(
            string pageName,
            object routeValues,
            string fragment)
            : this(pageName, routeValues, permanent: false, fragment: fragment)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToPageResult"/> with the values
        /// provided.
        /// </summary>
        /// <param name="pageName">The name of the route.</param>
        /// <param name="routeValues">The parameters for the route.</param>
        /// <param name="permanent">If set to true, makes the redirect permanent (301). Otherwise a temporary redirect is used (302).</param>
        /// <param name="fragment">The fragment to add to the URL.</param>
        public RedirectToPageResult(
            string pageName,
            object routeValues,
            bool permanent,
            string fragment)
        {
            PageName = pageName;
            RouteValues = routeValues == null ? new RouteValueDictionary() : new RouteValueDictionary(routeValues);
            Permanent = permanent;
            Fragment = fragment;
        }

        /// <summary>
        /// Gets or sets the <see cref="IUrlHelper" /> used to generate URLs.
        /// </summary>
        public IUrlHelper UrlHelper { get; set; }

        /// <summary>
        /// Gets or sets the name of the page to route to.
        /// </summary>
        public string PageName { get; set; }

        /// <summary>
        /// Gets or sets the route data to use for generating the URL.
        /// </summary>
        public RouteValueDictionary RouteValues { get; set; }

        /// <summary>
        /// Gets or sets an indication that the redirect is permanent.
        /// </summary>
        public bool Permanent { get; set; }

        /// <summary>
        /// Gets or sets the fragment to add to the URL.
        /// </summary>
        public string Fragment { get; set; }

        /// <summary>
        /// Gets or sets the protocol for the URL, such as "http" or "https".
        /// </summary>
        public string Protocol { get; set; }

        /// <summary>
        /// Gets os sets the host name of the URL.
        /// </summary>
        public string Host { get; set; }

        /// <inheritdoc />
        public override void ExecuteResult(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (string.IsNullOrEmpty(PageName))
            {
                throw new InvalidOperationException(
                    Resources.FormatPropertyOfTypeCannotBeNull(nameof(PageName), nameof(RedirectToPageResult)));
            }

            var executor = context.HttpContext.RequestServices.GetRequiredService<RedirectToPageResultExecutor>();
            executor.Execute(context, this);
        }
    }
}
