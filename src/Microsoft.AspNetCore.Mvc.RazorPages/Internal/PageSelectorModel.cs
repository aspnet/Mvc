// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    public static class PageSelectorModel
    {
        private static readonly string IndexFileName = "Index" + RazorViewEngine.ViewExtension;
        private static readonly char[] PathSeparator = new[] { '/' };

        public static void PopulateDefaults(PageRouteModel model, string pageRoute, string routeTemplate)
        {
            model.RouteValues.Add("page", model.ViewEnginePath);

            if (AttributeRouteModel.IsOverridePattern(routeTemplate))
            {
                throw new InvalidOperationException(string.Format(
                    Resources.PageActionDescriptorProvider_RouteTemplateCannotBeOverrideable,
                    model.RelativePath));
            }

            var selectorModel = CreateSelectorModel(pageRoute, routeTemplate);
            model.Selectors.Add(selectorModel);

            var fileName = Path.GetFileName(model.RelativePath);
            if (string.Equals(IndexFileName, fileName, StringComparison.OrdinalIgnoreCase))
            {
                // For pages ending in /Index.cshtml, we want to allow incoming routing, but
                // force outgoing routes to match to the path sans /Index.
                selectorModel.AttributeRouteModel.SuppressLinkGeneration = true;

                var index = pageRoute.LastIndexOf('/');
                var parentDirectoryPath = index == -1 ?
                    string.Empty :
                    pageRoute.Substring(0, index);
                model.Selectors.Add(CreateSelectorModel(parentDirectoryPath, routeTemplate));
            }
        }

        public static bool TryParseAreaPath(
            RazorPagesOptions razorPagesOptions,
            string path,
            ILogger logger,
            out (string areaName, string viewEnginePath, string pageRoute) result)
        {
            // path = "/Products/Pages/Manage/Home.cshtml"
            // Result = ("Products", "/Manage/Home", "/Products/Manage/Home")

            result = default;
            Debug.Assert(path.StartsWith("/", StringComparison.Ordinal));

            var stringTokenizer = new StringTokenizer(path, PathSeparator);
            var enumerator = stringTokenizer.GetEnumerator();
            var next = enumerator.MoveNext();
            Debug.Assert(next && enumerator.Current.Length == 0, "Paths must start with leading slash");

            if (!enumerator.MoveNext() || !enumerator.Current.HasValue)
            {
                logger.UnsupportedAreaPath(razorPagesOptions, path);
                return false;
            }

            var areaName = enumerator.Current;
            if (razorPagesOptions.RootDirectory != "/")
            {
                Debug.Assert(razorPagesOptions.RootDirectory.StartsWith("/", StringComparison.Ordinal));
                var rootDirectory = razorPagesOptions.RootDirectory;
                var normalizedRootDirectory = new StringSegment(rootDirectory, 1, rootDirectory.Length - 1);
                var rootDirectoryTokenizer = new StringTokenizer(normalizedRootDirectory, PathSeparator);
                foreach (var segment in rootDirectoryTokenizer)
                {
                    if (!enumerator.MoveNext() || !segment.Equals(enumerator.Current, StringComparison.OrdinalIgnoreCase))
                    {
                        logger.UnsupportedAreaPath(razorPagesOptions, path);
                        return false;
                    }
                }
            }

            Debug.Assert(path.EndsWith(RazorViewEngine.ViewExtension), $"{path} does not end in extension '{RazorViewEngine.ViewExtension}'.");
            // Include the "/" in the pageName. We want it both to append after the area name and for the view engine path which this value represents.
            var pagePathIndex = enumerator.Index - 1;
            var pageName = path.Substring(pagePathIndex, path.Length - pagePathIndex - RazorViewEngine.ViewExtension.Length);

            var builder = new InplaceStringBuilder(1 + areaName.Length + pageName.Length);
            builder.Append('/');
            builder.Append(areaName);
            builder.Append(pageName);
            var pageRoute = builder.ToString();

            result = (areaName.ToString(), pageName, pageRoute);
            return true;
        }

        private static SelectorModel CreateSelectorModel(string prefix, string template)
        {
            return new SelectorModel
            {
                AttributeRouteModel = new AttributeRouteModel
                {
                    Template = AttributeRouteModel.CombineTemplates(prefix, template),
                }
            };
        }
    }
}
