// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matchers;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class MvcEndpointDataSource : EndpointDataSource
    {
        private readonly IActionDescriptorCollectionProvider _actions;
        private readonly IActionInvokerFactory _invokerFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly List<Endpoint> _endpoints;

        public MvcEndpointDataSource(
            IActionDescriptorCollectionProvider actions,
            IActionInvokerFactory invokerFactory)
            : this(actions, invokerFactory, actionContextAccessor: null)
        {
        }

        public MvcEndpointDataSource(
            IActionDescriptorCollectionProvider actions,
            IActionInvokerFactory invokerFactory,
            IActionContextAccessor actionContextAccessor)
        {
            if (actions == null)
            {
                throw new ArgumentNullException(nameof(actions));
            }

            if (invokerFactory == null)
            {
                throw new ArgumentNullException(nameof(invokerFactory));
            }

            _actions = actions;
            _invokerFactory = invokerFactory;

            // The IActionContextAccessor is optional. We want to avoid the overhead of using CallContext
            // if possible.
            _actionContextAccessor = actionContextAccessor;

            _endpoints = new List<Endpoint>();

            InitializeEndpoints();
        }

        private void InitializeEndpoints()
        {
            // note: this code has haxxx. This will only work in some constrained scenarios
            foreach (var action in _actions.ActionDescriptors.Items)
            {
                if (action.AttributeRouteInfo == null)
                {
                    // Action does not have an attribute route
                    continue;
                }

                RequestDelegate invokerDelegate = (context) =>
                {
                    var values = context.Features.Get<IEndpointFeature>().Values;
                    var routeData = new RouteData();
                    foreach (var kvp in values)
                    {
                        routeData.Values.Add(kvp.Key, kvp.Value);
                    }

                    var actionContext = new ActionContext(context, routeData, action);
                    if (_actionContextAccessor != null)
                    {
                        _actionContextAccessor.ActionContext = actionContext;
                    }

                    var invoker = _invokerFactory.CreateInvoker(actionContext);
                    return invoker.InvokeAsync();
                };

                var descriptors = action.FilterDescriptors.OrderBy(f => f, FilterDescriptorOrderComparer.Comparer).Select(f => f.Filter).ToArray();
                var metadataCollection = new EndpointMetadataCollection(descriptors);

                _endpoints.Add(new MatcherEndpoint(
                    next => invokerDelegate,
                    "/" + action.AttributeRouteInfo.Template,
                    action.RouteValues,
                    action.AttributeRouteInfo.Order,
                    metadataCollection,
                    action.DisplayName));
            }
        }

        public override IChangeToken ChangeToken { get; } = NullChangeToken.Singleton;
        public override IReadOnlyList<Endpoint> Endpoints => _endpoints;
    }
}