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
    /// Targets a controller action.
    /// </summary>
    public class RedirectToActionResult : ActionResult, IKeepTempDataResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToActionResult"/> with the values
        /// provided.
        /// </summary>
        /// <param name="actionName">The name of the action to use for generating the URL.</param>
        /// <param name="controllerName">The name of the controller to use for generating the URL.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        public RedirectToActionResult(
            string actionName,
            string controllerName,
            object routeValues)
            : this(actionName, controllerName, routeValues, permanent: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToActionResult"/> with the values
        /// provided.
        /// </summary>
        /// <param name="actionName">The name of the action to use for generating the URL.</param>
        /// <param name="controllerName">The name of the controller to use for generating the URL.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <param name="fragment">The fragment to add to the URL.</param>
        public RedirectToActionResult(
            string actionName,
            string controllerName,
            object routeValues,
            string fragment)
            : this(actionName, controllerName, routeValues, permanent: false, fragment: fragment)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToActionResult"/> with the values
        /// provided.
        /// </summary>
        /// <param name="actionName">The name of the action to use for generating the URL.</param>
        /// <param name="controllerName">The name of the controller to use for generating the URL.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <param name="permanent">If set to true, makes the redirect permanent (301). Otherwise a temporary redirect is used (302).</param>
        public RedirectToActionResult(
            string actionName,
            string controllerName,
            object routeValues,
            bool permanent)
            : this(actionName, controllerName, routeValues, permanent, fragment: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToActionResult"/> with the values
        /// provided.
        /// </summary>
        /// <param name="actionName">The name of the action to use for generating the URL.</param>
        /// <param name="controllerName">The name of the controller to use for generating the URL.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <param name="permanent">If set to true, makes the redirect permanent (301). Otherwise a temporary redirect is used (302).</param>
        /// <param name="preserveMethod">If set to true, make the temporary redirect (307) or permanent redirect (308) preserve the intial request's method.</param>
        public RedirectToActionResult(
            string actionName,
            string controllerName,
            object routeValues,
            bool permanent,
            bool preserveMethod)
            : this(actionName, controllerName, routeValues, permanent, preserveMethod, fragment: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToActionResult"/> with the values
        /// provided.
        /// </summary>
        /// <param name="actionName">The name of the action to use for generating the URL.</param>
        /// <param name="controllerName">The name of the controller to use for generating the URL.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <param name="permanent">If set to true, makes the redirect permanent (301). Otherwise a temporary redirect is used (302).</param>
        /// <param name="fragment">The fragment to add to the URL.</param>
        public RedirectToActionResult(
            string actionName,
            string controllerName,
            object routeValues,
            bool permanent,
            string fragment)
            : this(actionName, controllerName, routeValues, permanent, preserveMethod: false, fragment: fragment)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToActionResult"/> with the values
        /// provided.
        /// </summary>
        /// <param name="actionName">The name of the action to use for generating the URL.</param>
        /// <param name="controllerName">The name of the controller to use for generating the URL.</param>
        /// <param name="routeValues">The route data to use for generating the URL.</param>
        /// <param name="permanent">If set to true, makes the redirect permanent (301). Otherwise a temporary redirect is used (302).</param>
        /// <param name="preserveMethod">If set to true, make the temporary redirect (307) and permanent redirect (308) preserve the intial request's method.</param>
        /// <param name="fragment">The fragment to add to the URL.</param>
        public RedirectToActionResult(
            string actionName,
            string controllerName,
            object routeValues,
            bool permanent,
            bool preserveMethod,
            string fragment)
        {
            ActionName = actionName;
            ControllerName = controllerName;
            RouteValues = routeValues == null ? null : new RouteValueDictionary(routeValues);
            Permanent = permanent;
            PreserveMethod = preserveMethod;
            Fragment = fragment;
        }

        /// <summary>
        /// Gets or sets the <see cref="IUrlHelper" /> used to generate URLs.
        /// </summary>
        public IUrlHelper UrlHelper { get; set; }

        /// <summary>
        /// Gets or sets the name of the action to use for generating the URL.
        /// </summary>
        public string ActionName { get; set; }

        /// <summary>
        /// Gets or sets the name of the controller to use for generating the URL.
        /// </summary>
        public string ControllerName { get; set; }

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

            var executor = context.HttpContext.RequestServices.GetRequiredService<RedirectToActionResultExecutor>();
            executor.Execute(context, this);
        }
    }
}
