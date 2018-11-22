﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    internal class CompiledPageRouteModelProvider : IPageRouteModelProvider
    {
        private static readonly string RazorPageDocumentKind = "mvc.1.0.razor-page";
        private static readonly string RouteTemplateKey = "RouteTemplate";
        private readonly ApplicationPartManager _applicationManager;
        private readonly RazorPagesOptions _pagesOptions;
        private readonly PageRouteModelFactory _routeModelFactory;

        public CompiledPageRouteModelProvider(
            ApplicationPartManager applicationManager,
            IOptions<RazorPagesOptions> pagesOptionsAccessor,
            ILogger<CompiledPageRouteModelProvider> logger)
        {
            _applicationManager = applicationManager ?? throw new ArgumentNullException(nameof(applicationManager));
            _pagesOptions = pagesOptionsAccessor?.Value ?? throw new ArgumentNullException(nameof(pagesOptionsAccessor));
            _routeModelFactory = new PageRouteModelFactory(_pagesOptions, logger);
        }

        public int Order => -1000;

        public void OnProvidersExecuting(PageRouteModelProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            CreateModels(context);
        }

        public void OnProvidersExecuted(PageRouteModelProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
        }

        private IEnumerable<CompiledViewDescriptor> GetViewDescriptors(ApplicationPartManager applicationManager)
        {
            if (applicationManager == null)
            {
                throw new ArgumentNullException(nameof(applicationManager));
            }

            var viewsFeature = GetViewFeature(applicationManager);

            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var viewDescriptor in viewsFeature.ViewDescriptors)
            {
                if (!visited.Add(viewDescriptor.RelativePath))
                {
                    // Already seen an descriptor with a higher "order"
                    continue;
                }

                if (IsRazorPage(viewDescriptor))
                {
                    yield return viewDescriptor;
                }
            }

            bool IsRazorPage(CompiledViewDescriptor viewDescriptor)
            {
                if (viewDescriptor.Item != null)
                {
                    return viewDescriptor.Item.Kind == RazorPageDocumentKind;
                }
                else if (viewDescriptor.ViewAttribute != null)
                {
                    return viewDescriptor.ViewAttribute is RazorPageAttribute;
                }

                return false;
            }
        }

        protected virtual ViewsFeature GetViewFeature(ApplicationPartManager applicationManager)
        {
            var viewsFeature = new ViewsFeature();
            applicationManager.PopulateFeature(viewsFeature);
            return viewsFeature;
        }

        private void CreateModels(PageRouteModelProviderContext context)
        {
            var rootDirectory = _pagesOptions.RootDirectory;
            if (!rootDirectory.EndsWith("/", StringComparison.Ordinal))
            {
                rootDirectory = rootDirectory + "/";
            }

            var areaRootDirectory = "/Areas/";
            foreach (var viewDescriptor in GetViewDescriptors(_applicationManager))
            {
                var relativePath = viewDescriptor.RelativePath;
                var routeTemplate = GetRouteTemplate(viewDescriptor);
                PageRouteModel routeModel = null;

                // When RootDirectory and AreaRootDirectory overlap (e.g. RootDirectory = '/', AreaRootDirectory = '/Areas'), we
                // only want to allow a page to be associated with the area route.
                if (_pagesOptions.AllowAreas && relativePath.StartsWith(areaRootDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    routeModel = _routeModelFactory.CreateAreaRouteModel(relativePath, routeTemplate);
                }
                else if (relativePath.StartsWith(rootDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    routeModel = _routeModelFactory.CreateRouteModel(relativePath, routeTemplate);
                }

                if (routeModel != null)
                {
                    context.RouteModels.Add(routeModel);
                }
            }
        }

        internal static string GetRouteTemplate(CompiledViewDescriptor viewDescriptor)
        {
            if (viewDescriptor.ViewAttribute != null)
            {
                return ((RazorPageAttribute)viewDescriptor.ViewAttribute).RouteTemplate;
            }

            if (viewDescriptor.Item != null)
            {
                return viewDescriptor.Item.Metadata
                    .OfType<RazorCompiledItemMetadataAttribute>()
                    .FirstOrDefault(f => f.Key == RouteTemplateKey)
                    ?.Value;
            }

            return null;
        }
    }
}
