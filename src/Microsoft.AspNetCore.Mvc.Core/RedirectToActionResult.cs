// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// An <see cref="ActionResult"/> that returns a Found (302), Moved Permanently (301), Temporary Redirect (307),
    /// or Permanent Redirect (308) response with a Location header.
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
        /// <param name="preserveMethod">If set to true, make the temporary redirect (307) or permanent redirect (308) preserve the initial request method.</param>
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
        /// <param name="preserveMethod">If set to true, make the temporary redirect (307) and permanent redirect (308) preserve the initial request method.</param>
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
        /// Gets or sets an indication that the redirect preserves the initial request method.
        /// </summary>
        public bool PreserveMethod { get; set; }

        /// <summary>
        /// Gets or sets the fragment to add to the URL.
        /// </summary>
        public string Fragment { get; set; }

        /// <inheritdoc />
        public override Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var executor = context.HttpContext.RequestServices.GetRequiredService<IActionResultExecutor<RedirectToActionResult>>();
            return executor.ExecuteAsync(context, this);
        }

#pragma warning disable CS0809
        [Obsolete("This implementation will be removed in a future release, use ExecuteResultAsync.")]
        public override void ExecuteResult(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var services = context.HttpContext.RequestServices;
            var urlHelperFactory = services.GetRequiredService<IUrlHelperFactory>();
            var logger = services.GetRequiredService<ILogger<RedirectToActionResultExecutor>>();

            var urlHelper = UrlHelper ?? urlHelperFactory.GetUrlHelper(context);

            var destinationUrl = urlHelper.Action(
                ActionName,
                ControllerName,
                RouteValues,
                protocol: null,
                host: null,
                fragment: Fragment);
            if (string.IsNullOrEmpty(destinationUrl))
            {
                throw new InvalidOperationException(Resources.NoRoutesMatched);
            }

            logger.RedirectToActionResultExecuting(destinationUrl);

            if (PreserveMethod)
            {
                context.HttpContext.Response.StatusCode = Permanent ?
                    StatusCodes.Status308PermanentRedirect : StatusCodes.Status307TemporaryRedirect;
                context.HttpContext.Response.Headers[HeaderNames.Location] = destinationUrl;
            }
            else
            {
                context.HttpContext.Response.Redirect(destinationUrl, Permanent);
            }
        }
#pragma warning restore CS0809
    }
}
