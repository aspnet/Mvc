// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    public class RazorProjectPageApplicationModelProvider : IPageApplicationModelProvider
    {
        private readonly RazorProject _project;
        private readonly RazorPagesOptions _pagesOptions;

        public RazorProjectPageApplicationModelProvider(
            RazorProject razorProject,
            IOptions<RazorPagesOptions> pagesOptionsAccessor)
        {
            _project = razorProject;
            _pagesOptions = pagesOptionsAccessor.Value;
        }

        public int Order => -1000;

        public void OnProvidersExecuted(PageApplicationModelProviderContext context)
        {
        }

        public void OnProvidersExecuting(PageApplicationModelProviderContext context)
        {
            foreach (var item in _project.EnumerateItems(_pagesOptions.RootDirectory))
            {
                if (item.FileName.StartsWith("_"))
                {
                    // Pages like _ViewImports should not be routable.
                    continue;
                }

                if (!PageDirectiveFeature.TryGetPageDirective(item, out var directive))
                {
                    // .cshtml pages without @page are not RazorPages.
                    continue;
                }

                var pageApplicationModel = new PageApplicationModel(
                    relativePath: item.CombinedPath,
                    viewEnginePath: item.PathWithoutExtension);

                pageApplicationModel.Name = directive.Name;

                PageSelectorModel.PopulateDefaults(pageApplicationModel, directive.RouteTemplate);

                context.Results.Add(pageApplicationModel);
            }
        }
    }
}
