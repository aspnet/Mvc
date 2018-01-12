﻿// Copyright (c) .NET Foundation. All rights reserved.
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

        public static void PopulateDefaults(PageRouteModel model, string pageRoute, string routeTemplate)
        {
            model.RouteValues.Add("page", model.ViewEnginePath);

            var selectorModel = CreateSelectorModel(pageRoute, routeTemplate);
            model.Selectors.Add(selectorModel);

            var fileName = Path.GetFileName(model.RelativePath);

            if (string.Equals(IndexFileName, fileName, StringComparison.OrdinalIgnoreCase) && 
                !AttributeRouteModel.IsOverridePattern(routeTemplate))
            {
                // For pages without an override route, and ending in /Index.cshtml, we want to allow 
                // incoming routing, but force outgoing routes to match to the path sans /Index.
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

            // 1. Parse the area name. This  will be the first token we encounter.
            var areaEndIndex = path.IndexOf('/', startIndex: 1);
            if (areaEndIndex == -1 || areaEndIndex == path.Length)
            {
                logger.UnsupportedAreaPath(razorPagesOptions, path);
                return false;
            }

            var areaName = path.Substring(1, areaEndIndex - 1);

            string pageName;
            if (razorPagesOptions.RootDirectory == "/")
            {
                // When RootDirectory is "/", every thing past the area name is the page path.
                Debug.Assert(path.EndsWith(RazorViewEngine.ViewExtension), $"{path} does not end in extension '{RazorViewEngine.ViewExtension}'.");
                pageName = path.Substring(areaEndIndex, path.Length - areaEndIndex - RazorViewEngine.ViewExtension.Length);
            }
            else
            {
                // Normalize the pages root directory so that it has a trailing slash. This ensures we're looking at a directory delimiter
                // and not just the area name occuring as part of a segment.
                Debug.Assert(razorPagesOptions.RootDirectory.StartsWith("/", StringComparison.Ordinal));
                var normalizedPagesRootDirectory = razorPagesOptions.RootDirectory.Substring(1);
                if (!normalizedPagesRootDirectory.EndsWith("/", StringComparison.Ordinal))
                {
                    normalizedPagesRootDirectory += "/";
                }

                Debug.Assert(normalizedPagesRootDirectory.Length > 0);
                // If the pages root has a value i.e. it's not the app root "/", ensure that the area path contains this value.
                if (string.Compare(path, areaEndIndex + 1, normalizedPagesRootDirectory, 0, normalizedPagesRootDirectory.Length, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    logger.UnsupportedAreaPath(razorPagesOptions, path);
                    return false;
                }

                var pageNameIndex = areaEndIndex + normalizedPagesRootDirectory.Length;
                pageName = path.Substring(pageNameIndex, path.Length - pageNameIndex - RazorViewEngine.ViewExtension.Length);
            }

            var builder = new InplaceStringBuilder(areaEndIndex + pageName.Length);
            builder.Append(path, 0, areaEndIndex);
            builder.Append(pageName);
            var pageRoute = builder.ToString();

            result = (areaName, pageName, pageRoute);
            return true;
        }

        private static SelectorModel CreateSelectorModel(string prefix, string routeTemplate)
        {
            return new SelectorModel
            {
                AttributeRouteModel = new AttributeRouteModel
                {
                    Template = AttributeRouteModel.CombineTemplates(prefix, routeTemplate),
                }
            };
        }
    }
}
