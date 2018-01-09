// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages.Internal;
using Microsoft.AspNetCore.Mvc.Routing;
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

            RemoveSupersededFallbackRoutes(context);

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
                var descriptor = new PageActionDescriptor()
                {
                    AttributeRouteInfo = new AttributeRouteInfo()
                    {
                        Name = selector.AttributeRouteModel.Name,
                        Order = selector.AttributeRouteModel.Order ?? 0,
                        Template = selector.AttributeRouteModel.Template,
                        SuppressLinkGeneration = selector.AttributeRouteModel.SuppressLinkGeneration,
                        SuppressPathMatching = selector.AttributeRouteModel.SuppressPathMatching,
                    },
                    DisplayName = $"Page: {model.PageName}",
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
                    descriptor.RouteValues.Add("page", model.PageName);
                }

                actions.Add(descriptor);
            }
        }

        private void RemoveSupersededFallbackRoutes(PageRouteModelProviderContext context)
        {
            if (!context.RouteModels.Any(m => m.IsFallbackRoute))
            {
                return;
            }

            var nonFallbackRouteLookup = new Dictionary<string, (PageRouteModel single, List<PageRouteModel> multiple)>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < context.RouteModels.Count; i++)
            {
                var routeModel = context.RouteModels[i];
                if (routeModel.IsFallbackRoute)
                {
                    continue;
                }

                var key = routeModel.PageName;
                if (nonFallbackRouteLookup.TryGetValue(key, out var value))
                {
                    var multiple = value.multiple;
                    if (multiple == null)
                    {
                        multiple = new List<PageRouteModel> { value.single };
                        nonFallbackRouteLookup[key] = (default, multiple);
                    }

                    multiple.Add(routeModel);
                }
                else
                {
                    nonFallbackRouteLookup[key] = (routeModel, default);
                }
            }

            for (var i = context.RouteModels.Count - 1; i >= 0; i--)
            {
                var fallbackRouteModel = context.RouteModels[i];
                if (!fallbackRouteModel.IsFallbackRoute)
                {
                    continue;
                }

                if (!nonFallbackRouteLookup.TryGetValue(fallbackRouteModel.PageName, out var value))
                {
                    continue;
                }

                if (value.multiple != null && value.multiple.Any(model => FallbackRazorPagesConventions.IsSuperseded(fallbackRouteModel, model)))
                {
                    context.RouteModels.RemoveAt(i);
                }
                else if (value.single != null && FallbackRazorPagesConventions.IsSuperseded(fallbackRouteModel, value.single))
                {
                    context.RouteModels.RemoveAt(i);
                }
            }
        }
    }
}