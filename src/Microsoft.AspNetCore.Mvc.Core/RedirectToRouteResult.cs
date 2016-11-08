// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Mvc
{
    public class RedirectToRouteResult : ActionResult, IKeepTempDataResult
    {
        public RedirectToRouteResult(object routeValues)
            : this(routeName: null, routeValues: routeValues)
        {
        }

        public RedirectToRouteResult(
            string routeName,
            object routeValues)
            : this(routeName, routeValues, permanent: false)
        {
        }

        public RedirectToRouteResult(
            string routeName,
            object routeValues,
            bool permanent)
            : this(routeName, routeValues, permanent, fragment: null)
        {
        }

        public RedirectToRouteResult(
            string routeName,
            object routeValues,
            string fragment)
            : this(routeName, routeValues, permanent: false, fragment: fragment)
        {
        }

        public RedirectToRouteResult(
            string routeName,
            object routeValues,
            bool permanent,
            string fragment)
        {
            RouteName = routeName;
            RouteValues = routeValues == null ? null : new RouteValueDictionary(routeValues);
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

        public bool Permanent { get; set; }

        /// <summary>
        /// Gets or sets the fragment to be added to the URL
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
