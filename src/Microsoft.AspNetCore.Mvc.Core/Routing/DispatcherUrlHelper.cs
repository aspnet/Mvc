// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc.Routing
{
    /// <summary>
    /// An implementation of <see cref="IUrlHelper"/> that uses <see cref="ILinkGenerator"/> to build URLs 
    /// for ASP.NET MVC within an application.
    /// </summary>
    public class DispatcherUrlHelper : IUrlHelper
    {
        private readonly UrlHelperCommon _urlHelperCommon;
        private readonly ILogger<DispatcherUrlHelper> _logger;
        private readonly ILinkGenerator _linkGenerator;
        private readonly RouteValueDictionary _ambientValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlHelper"/> class using the specified
        /// <paramref name="actionContext"/>.
        /// </summary>
        /// <param name="actionContext">The <see cref="Mvc.ActionContext"/> for the current request.</param>
        /// <param name="linkGenerator">The <see cref="ILinkGenerator"/> used to generate the link.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public DispatcherUrlHelper(
            ActionContext actionContext,
            ILinkGenerator linkGenerator,
            ILogger<DispatcherUrlHelper> logger)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }

            if (linkGenerator == null)
            {
                throw new ArgumentNullException(nameof(linkGenerator));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            ActionContext = actionContext;
            _ambientValues = ActionContext.RouteData.Values;
            _linkGenerator = linkGenerator;
            _urlHelperCommon = new UrlHelperCommon(actionContext.HttpContext);
            _logger = logger;
        }

        /// <inheritdoc />
        public ActionContext ActionContext { get; }

        /// <inheritdoc />
        public virtual string Action(UrlActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }

            var valuesDictionary = _urlHelperCommon.GetValuesDictionary(actionContext.Values);

            if (actionContext.Action == null)
            {
                object action;
                if (!valuesDictionary.ContainsKey("action") &&
                    _ambientValues.TryGetValue("action", out action))
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
                    _ambientValues.TryGetValue("controller", out controller))
                {
                    valuesDictionary["controller"] = controller;
                }
            }
            else
            {
                valuesDictionary["controller"] = actionContext.Controller;
            }

            var successfullyGeneratedLink = _linkGenerator.TryGetLink(
                new LinkGeneratorContext()
                {
                    SuppliedValues = valuesDictionary,
                    AmbientValues = _ambientValues
                },
                out var link);

            if (!successfullyGeneratedLink)
            {
                //TODO: log here

                return null;
            }

            return _urlHelperCommon.GenerateUrl(actionContext.Protocol, actionContext.Host, link, actionContext.Fragment);
        }

        /// <inheritdoc />
        public string Content(string contentPath)
        {
            return _urlHelperCommon.Content(contentPath);
        }

        /// <inheritdoc />
        public bool IsLocalUrl(string url)
        {
            return _urlHelperCommon.IsLocalUrl(url);
        }

        /// <inheritdoc />
        public string RouteUrl(UrlRouteContext routeContext)
        {
            if (routeContext == null)
            {
                throw new ArgumentNullException(nameof(routeContext));
            }

            var valuesDictionary = routeContext.Values as RouteValueDictionary ?? _urlHelperCommon.GetValuesDictionary(routeContext.Values);

            var successfullyGeneratedLink = _linkGenerator.TryGetLink(
                new LinkGeneratorContext()
                {
                    Address = new Address(routeContext.RouteName),
                    SuppliedValues = valuesDictionary,
                    AmbientValues = _ambientValues
                },
                out var link);

            if (!successfullyGeneratedLink)
            {
                return null;
            }

            return _urlHelperCommon.GenerateUrl(routeContext.Protocol, routeContext.Host, link, routeContext.Fragment);
        }

        /// <inheritdoc />
        public string Link(string routeName, object values)
        {
            return RouteUrl(new UrlRouteContext()
            {
                RouteName = routeName,
                Values = values,
                Protocol = ActionContext.HttpContext.Request.Scheme,
                Host = ActionContext.HttpContext.Request.Host.ToUriComponent()
            });
        }
    }
}