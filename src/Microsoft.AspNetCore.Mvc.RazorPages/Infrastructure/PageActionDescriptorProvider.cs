// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure
{
    public class PageActionDescriptorProvider : IActionDescriptorProvider
    {
        private readonly RazorProject _project;
        private readonly MvcOptions _mvcOptions;
        private readonly RazorPagesOptions _pagesOptions;

        public PageActionDescriptorProvider(
            RazorProject project,
            IOptions<MvcOptions> mvcOptionsAccessor,
            IOptions<RazorPagesOptions> pagesOptionsAccessor)
        {
            _project = project;
            _mvcOptions = mvcOptionsAccessor.Value;
            _pagesOptions = pagesOptionsAccessor.Value;
        }

        public int Order { get; set; }

        public void OnProvidersExecuting(ActionDescriptorProviderContext context)
        {
            foreach (var item in _project.EnumerateItems("/"))
            {
                if (item.Filename.StartsWith("_"))
                {
                    // Pages like _PageImports should not be routable.
                    continue;
                }

                var routeTemplates = GetRouteTemplates(item);
                if (string.IsNullOrEmpty(routeTemplates.Template))
                {
                    // .cshtml pages without @page are not RazorPages.
                    continue;
                }

                AddActionDescriptors(context.Results, item, routeTemplates);
            }
        }

        public void OnProvidersExecuted(ActionDescriptorProviderContext context)
        {
        }

        private void AddActionDescriptors(
            IList<ActionDescriptor> actions,
            RazorProjectItem item, 
            RouteTemplates templates)
        {
            var model = new PageModel(item.CombinedPath, item.PathWithoutExtension);
            model.Selectors.Add(new SelectorModel
            {
                AttributeRouteModel = new AttributeRouteModel
                {
                    Template = templates.Template,
                }
            });

            for (var i = 0; i < _pagesOptions.Conventions.Count; i++)
            {
                _pagesOptions.Conventions[i].Apply(model);
            }

            if (!string.IsNullOrEmpty(templates.AlternateTemplate))
            {
                model.Selectors.Add(new SelectorModel
                {
                    AttributeRouteModel = new AttributeRouteModel
                    {
                        Template = templates.AlternateTemplate,
                    }
                });
            }

            var filters = new List<FilterDescriptor>(_mvcOptions.Filters.Count + model.Filters.Count);
            for (var i = 0; i < _mvcOptions.Filters.Count; i++)
            {
                filters.Add(new FilterDescriptor(_mvcOptions.Filters[i], FilterScope.Global));
            }

            for (var i = 0; i < model.Filters.Count; i++)
            {
                filters.Add(new FilterDescriptor(model.Filters[i], FilterScope.Action));
            }

            foreach (var selector in model.Selectors)
            {
                actions.Add(new PageActionDescriptor()
                {
                    AttributeRouteInfo = new AttributeRouteInfo()
                    {
                        Name = selector.AttributeRouteModel.Name,
                        Order = selector.AttributeRouteModel.Order ?? 0,
                        Template = selector.AttributeRouteModel.Template,
                    },
                    DisplayName = $"Page: {item.Path}",
                    FilterDescriptors = filters,
                    Properties = new Dictionary<object, object>(model.Properties),
                    RelativePath = item.CombinedPath,
                    RouteValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "page", item.PathWithoutExtension },
                    },
                    ViewEnginePath = item.Path,
                });
            }
        }

        private RouteTemplates GetRouteTemplates(RazorProjectItem item)
        {
            string template;
            if (!PageDirectiveFeature.TryGetRouteTemplate(item, out template))
            {
                return default(RouteTemplates);
            }

            Debug.Assert(template != null);

            if (template.Length > 0 && template[0] == '/')
            {
                template = template.Substring(1);
            }
            else if (template.Length > 1 && template[0] == '~' && template[1] == '/')
            {
                template = template.Substring(2);
            }

            var filePath = item.CombinedPathWithoutExtension.Substring(1);
            var routeTemplate = GetRouteTemplate(filePath, template);

            if (string.Equals("Index.cshtml", item.Filename, StringComparison.OrdinalIgnoreCase))
            {
                return new RouteTemplates(routeTemplate, GetRouteTemplate(item.BasePath.Substring(1), template));
            }

            return new RouteTemplates(routeTemplate);
        }

        private static string GetRouteTemplate(string prefix, string template)
        {
            if (prefix == string.Empty && string.IsNullOrEmpty(template))
            {
                return string.Empty;
            }
            else if (string.IsNullOrEmpty(template))
            {
                return prefix;
            }
            else if (prefix == string.Empty)
            {
                return template;
            }
            else
            {
                return prefix + "/" + template;
            }
        }

        private struct RouteTemplates
        {
            public RouteTemplates(string template)
                : this(template, alternateTemplate: null)
            {
            }

            public RouteTemplates(string template, string alternateTemplate)
            {
                Template = template;
                AlternateTemplate = alternateTemplate;
            }

            public string Template { get; }

            public string AlternateTemplate { get; }
        }
    }
}