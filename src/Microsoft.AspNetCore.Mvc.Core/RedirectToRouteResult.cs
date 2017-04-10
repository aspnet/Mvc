// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// An <see cref="ActionResult"/> that returns a Found (302)
    /// or Moved Permanently (301) response with a Location header.
    /// Targets a registered route.
    /// </summary>
    public class RedirectToRouteResult : ActionResult, IKeepTempDataResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToRouteResult"/> with the values
        /// provided.
        /// </summary>
        /// <param name="routeValues">The parameters for the route.</param>
        public RedirectToRouteResult(object routeValues)
            : this(routeName: null, routeValues: routeValues)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToRouteResult"/> with the values
        /// provided.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="routeValues">The parameters for the route.</param>
        public RedirectToRouteResult(
            string routeName,
            object routeValues)
            : this(routeName, routeValues, permanent: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToRouteResult"/> with the values
        /// provided.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="routeValues">The parameters for the route.</param>
        /// <param name="permanent">If set to true, makes the redirect permanent (301). Otherwise a temporary redirect is used (302).</param>
        public RedirectToRouteResult(
            string routeName,
            object routeValues,
            bool permanent)
            : this(routeName, routeValues, permanent, fragment: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToRouteResult"/> with the values
        /// provided.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="routeValues">The parameters for the route.</param>
        /// <param name="permanent">If set to true, makes the redirect permanent (301). Otherwise a temporary redirect is used (302).</param>
        /// <param name="preserveMethod">If set to true, make the temporary redirect (307) or permanent redirect (308) preserve the intial request's method.</param>
        public RedirectToRouteResult(
            string routeName,
            object routeValues,
            bool permanent,
            bool preserveMethod)
            : this(routeName, routeValues, permanent, preserveMethod, fragment: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToRouteResult"/> with the values
        /// provided.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="routeValues">The parameters for the route.</param>
        /// <param name="fragment">The fragment to add to the URL.</param>
        public RedirectToRouteResult(
            string routeName,
            object routeValues,
            string fragment)
            : this(routeName, routeValues, permanent: false, fragment: fragment)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToRouteResult"/> with the values
        /// provided.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="routeValues">The parameters for the route.</param>
        /// <param name="permanent">If set to true, makes the redirect permanent (301). Otherwise a temporary redirect is used (302).</param>
        /// <param name="fragment">The fragment to add to the URL.</param>
        public RedirectToRouteResult(
            string routeName,
            object routeValues,
            bool permanent,
            string fragment)
            : this(routeName, routeValues, permanent, preserveMethod: false, fragment: fragment)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToRouteResult"/> with the values
        /// provided.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="routeValues">The parameters for the route.</param>
        /// <param name="permanent">If set to true, makes the redirect permanent (301). Otherwise a temporary redirect is used (302).</param>
        /// <param name="preserveMethod">If set to true, make the temporary redirect (307) or permanent redirect (308) preserve the intial request's method.</param>
        /// <param name="fragment">The fragment to add to the URL.</param>
        public RedirectToRouteResult(
            string routeName,
            object routeValues,
            bool permanent,
            bool preserveMethod,
            string fragment)
        {
            RouteName = routeName;
            RouteValues = routeValues == null ? null : new RouteValueDictionary(routeValues);
            PreserveMethod = preserveMethod;
            Permanent = permanent;
            Fragment = fragment;
        }

        /// <summary>
        /// Gets or sets the <see cref="IUrlHelper" /> used to generate URLs.
        /// </summary>
        public IUrlHelper UrlHelper { get; set; }

        /// <summary>
        /// Gets or sets the name of the route to use for generating the URL.
        /// </summary>
        public string RouteName { get; set; }

        /// <summary>
        /// Gets or sets the route data to use for generating the URL.
        /// </summary>
        public RouteValueDictionary RouteValues { get; set; }

        /// <summary>
        /// Gets or sets an indication that the redirect is permanent.
        /// </summary>
        public bool Permanent { get; set; }

        /// <summary>
        /// Gets or sets an indication that the redirect preserves initial request method.
        /// </summary>
        public bool PreserveMethod { get; set; }

        /// <summary>
        /// Gets or sets the fragment to add to the URL.
        /// </summary>
        public string Fragment { get; set; }

        /// <inheritdoc />
        public override void ExecuteResult(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var executor = context.HttpContext.RequestServices.GetRequiredService<RedirectToRouteResultExecutor>();
            executor.Execute(context, this);
        }
    }
}
