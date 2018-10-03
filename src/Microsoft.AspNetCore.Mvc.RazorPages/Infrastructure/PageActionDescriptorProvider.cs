// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure
{
    public class PageActionDescriptorProvider : IActionDescriptorProvider
    {
        private readonly IPageRouteModelProvider[] _routeModelProviders;
        private readonly MvcOptions _mvcOptions;
        private readonly IPageRouteModelConvention[] _conventions;

        public PageActionDescriptorProvider(
            IEnumerable<IPageRouteModelProvider> pageRouteModelProviders,
            IOptions<MvcOptions> mvcOptionsAccessor,
            IOptions<RazorPagesOptions> pagesOptionsAccessor)
        {
            _routeModelProviders = pageRouteModelProviders.OrderBy(p => p.Order).ToArray();
            _mvcOptions = mvcOptionsAccessor.Value;

            _conventions = pagesOptionsAccessor.Value.Conventions
                .OfType<IPageRouteModelConvention>()
                .ToArray();
        }

        public int Order { get; set; } = -900; // Run after the default MVC provider, but before others.

        public void OnProvidersExecuting(ActionDescriptorProviderContext context)
        {
            var pageRouteModels = BuildModel();

            for (var i = 0; i < pageRouteModels.Count; i++)
            {
                AddActionDescriptors(context.Results, pageRouteModels[i]);
            }
        }

        protected IList<PageRouteModel> BuildModel()
        {
            var context = new PageRouteModelProviderContext();

            for (var i = 0; i < _routeModelProviders.Length; i++)
            {
                _routeModelProviders[i].OnProvidersExecuting(context);
            }

            for (var i = _routeModelProviders.Length - 1; i >= 0; i--)
            {
                _routeModelProviders[i].OnProvidersExecuted(context);
            }

            return context.RouteModels;
        }

        public void OnProvidersExecuted(ActionDescriptorProviderContext context)
        {
        }

        private void AddActionDescriptors(IList<ActionDescriptor> actions, PageRouteModel model)
        {
            for (var i = 0; i < _conventions.Length; i++)
            {
                _conventions[i].Apply(model);
            }

            foreach (var selector in model.Selectors)
            {
                var descriptor = new PageActionDescriptor
                {
                    ActionConstraints = selector.ActionConstraints.ToList(),
                    AreaName = model.AreaName,
                    AttributeRouteInfo = new AttributeRouteInfo
                    {
                        Name = selector.AttributeRouteModel.Name,
                        Order = selector.AttributeRouteModel.Order ?? 0,
                        Template = TransformPageRoute(model, selector.AttributeRouteModel.Template),
                        SuppressLinkGeneration = selector.AttributeRouteModel.SuppressLinkGeneration,
                        SuppressPathMatching = selector.AttributeRouteModel.SuppressPathMatching,
                    },
                    DisplayName = $"Page: {model.ViewEnginePath}",
                    EndpointMetadata = selector.EndpointMetadata.ToList(),
                    FilterDescriptors = Array.Empty<FilterDescriptor>(),
                    Properties = new Dictionary<object, object>(model.Properties),
                    RelativePath = model.RelativePath,
                    ViewEnginePath = model.ViewEnginePath,
                };

                foreach (var kvp in model.RouteValues)
                {
                    if (!descriptor.RouteValues.ContainsKey(kvp.Key))
                    {
                        descriptor.RouteValues.Add(kvp.Key, kvp.Value);
                    }
                }

                if (!descriptor.RouteValues.ContainsKey("page"))
                {
                    descriptor.RouteValues.Add("page", model.ViewEnginePath);
                }

                actions.Add(descriptor);
            }
        }

        private static string TransformPageRoute(PageRouteModel model, string pageRoute)
        {
            model.Properties.TryGetValue(typeof(IOutboundParameterTransformer), out var transformer);
            var pageRouteTransformer = transformer as IOutboundParameterTransformer;

            if (pageRouteTransformer == null)
            {
                return pageRoute;
            }

            var routePattern = RoutePatternFactory.Parse(pageRoute);
            var segments = new RoutePatternPathSegment[routePattern.PathSegments.Count];

            // Read through the route's segments and transform literal segments
            // e.g. using a slugify transformer...
            // Before: /MyTestRoute/{ExtraArguments}
            // After: /my-test-route/{ExtraArguments}
            for (int i = 0; i < routePattern.PathSegments.Count; i++)
            {
                var segment = routePattern.PathSegments[i];
                if (segment.IsSimple && segment.Parts[0] is RoutePatternLiteralPart literalPart)
                {
                    var transformedContent = pageRouteTransformer.TransformOutbound(literalPart.Content);
                    segments[i] = RoutePatternFactory.Segment(RoutePatternFactory.LiteralPart(transformedContent));
                }
                else
                {
                    segments[i] = segment;
                }
            }

            var sb = new StringBuilder();
            RoutePatternWriter.WriteString(sb, segments);

            return sb.ToString();
        }
    }
}