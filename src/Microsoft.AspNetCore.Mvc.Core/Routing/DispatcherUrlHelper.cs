// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc.Routing
{
    /// <summary>
    /// An implementation of <see cref="IUrlHelper"/> that uses <see cref="ILinkGenerator"/> to build URLs 
    /// for ASP.NET MVC within an application.
    /// </summary>
    internal class DispatcherUrlHelper : UrlHelperBase
    {
        private readonly ILogger<DispatcherUrlHelper> _logger;
        private readonly ILinkGenerator _linkGenerator;
        private readonly IEndpointFinder _endpointFinder;

        /// <summary>
        /// Initializes a new instance of the <see cref="DispatcherUrlHelper"/> class using the specified
        /// <paramref name="actionContext"/>.
        /// </summary>
        /// <param name="actionContext">The <see cref="Mvc.ActionContext"/> for the current request.</param>
        /// <param name="linkGenerator">The <see cref="ILinkGenerator"/> used to generate the link.</param>
        /// <param name="endpointFinder"></param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public DispatcherUrlHelper(
            ActionContext actionContext,
            ILinkGenerator linkGenerator,
            MvcEndpointFinder endpointFinder,
            ILogger<DispatcherUrlHelper> logger)
            : base(actionContext)
        {
            if (linkGenerator == null)
            {
                throw new ArgumentNullException(nameof(linkGenerator));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _linkGenerator = linkGenerator;
            _endpointFinder = endpointFinder;
            _logger = logger;
        }

        /// <inheritdoc />
        public override string Action(UrlActionContext urlActionContext)
        {
            if (urlActionContext == null)
            {
                throw new ArgumentNullException(nameof(urlActionContext));
            }

            var explicitRouteValues = GetValuesDictionary(urlActionContext.Values);

            UpdateExplicitRouteValues("action", urlActionContext.Action, explicitRouteValues, out var actionName);
            UpdateExplicitRouteValues("controller", urlActionContext.Controller, explicitRouteValues, out var controllerName);

            var endpoints = _endpointFinder.FindEndpoints(
                new MvcAddress()
                {
                    Name = null,
                    CurrentActionContext = ActionContext,
                    TargetControllerName = controllerName?.ToString(),
                    TargetActionName = actionName?.ToString(),
                });

            var successfullyGeneratedLink = _linkGenerator.TryGetLink(
                new LinkGeneratorContext()
                {
                    Endpoints = endpoints,
                    SuppliedValues = explicitRouteValues,
                    AmbientValues = AmbientValues
                },
                out var link);

            if (!successfullyGeneratedLink)
            {
                //TODO: log here

                return null;
            }

            return GenerateUrl(urlActionContext.Protocol, urlActionContext.Host, link, urlActionContext.Fragment);
        }

        /// <inheritdoc />
        public override string RouteUrl(UrlRouteContext routeContext)
        {
            if (routeContext == null)
            {
                throw new ArgumentNullException(nameof(routeContext));
            }

            var valuesDictionary = routeContext.Values as RouteValueDictionary ?? GetValuesDictionary(routeContext.Values);
            valuesDictionary.TryGetValue("controller", out var controllerName);
            valuesDictionary.TryGetValue("action", out var actionName);
            valuesDictionary.TryGetValue("page", out var pageName);
            valuesDictionary.TryGetValue("handler", out var pageHandlerName);

            var endpoints = _endpointFinder.FindEndpoints(
                new MvcAddress
                {
                    Name = routeContext.RouteName,
                    CurrentActionContext = ActionContext,
                    TargetControllerName = controllerName?.ToString(),
                    TargetActionName = actionName?.ToString(),
                    TargetPageName = pageName?.ToString(),
                    TargetHandlerName = pageHandlerName?.ToString(),
                });

            var successfullyGeneratedLink = _linkGenerator.TryGetLink(
                new LinkGeneratorContext()
                {
                    SuppliedValues = valuesDictionary,
                    AmbientValues = AmbientValues,
                    Endpoints = endpoints,
                },
                out var link);

            if (!successfullyGeneratedLink)
            {
                return null;
            }

            return GenerateUrl(routeContext.Protocol, routeContext.Host, link, routeContext.Fragment);
        }

        public string Page(UrlPageContext urlPageContext)
        {
            if (urlPageContext == null)
            {
                throw new ArgumentNullException(nameof(urlPageContext));
            }

            var explicitRouteValues = GetValuesDictionary(urlPageContext.Values);

            UpdateExplicitRouteValues("page", urlPageContext.PageName, explicitRouteValues, out var pageName);

            if (string.IsNullOrEmpty(urlPageContext.PageHandlerName))
            {
                if (!explicitRouteValues.ContainsKey("handler") &&
                    AmbientValues.TryGetValue("handler", out var handler))
                {
                    // Clear out formaction unless it's explicitly specified in the routeValues.
                    explicitRouteValues["handler"] = null;
                }
            }
            else
            {
                explicitRouteValues["handler"] = urlPageContext.PageHandlerName;
            }

            var endpoints = _endpointFinder.FindEndpoints(
                new MvcAddress()
                {
                    Name = null,
                    CurrentActionContext = ActionContext,
                    TargetPageName = pageName?.ToString(),
                    TargetHandlerName = urlPageContext.PageHandlerName
                });

            var successfullyGeneratedLink = _linkGenerator.TryGetLink(
                new LinkGeneratorContext()
                {
                    Endpoints = endpoints,
                    SuppliedValues = explicitRouteValues,
                    AmbientValues = AmbientValues
                },
                out var link);

            if (!successfullyGeneratedLink)
            {
                //TODO: log here

                return null;
            }

            return GenerateUrl(urlPageContext.Protocol, urlPageContext.Host, link, urlPageContext.Fragment);
        }

        private void UpdateExplicitRouteValues(
            string key,
            string value,
            RouteValueDictionary explicitValues,
            out object routeValue)
        {
            routeValue = null;
            if (string.IsNullOrEmpty(value))
            {
                if (!explicitValues.ContainsKey(key) &&
                    AmbientValues.TryGetValue(key, out var ambientValue))
                {
                    explicitValues[key] = ambientValue;
                    routeValue = ambientValue;
                }
            }
            else
            {
                explicitValues[key] = value;
                routeValue = value;
            }
        }
    }
}