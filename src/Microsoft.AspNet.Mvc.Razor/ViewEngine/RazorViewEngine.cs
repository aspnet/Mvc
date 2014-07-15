// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNet.Mvc.Rendering;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Represents a view engine that is used to render a Web page that uses the Razor syntax.
    /// </summary>
    public class RazorViewEngine : IViewEngine
    {
        private const string ViewExtension = ".cshtml";

        private static readonly string[] _viewLocationFormats =
        {
            "/Views/{1}/{0}" + ViewExtension,
            "/Views/Shared/{0}" + ViewExtension,
        };

        private static readonly string[] _areaViewLocationFormats =
        {
            "/Areas/{2}/Views/{1}/{0}" + ViewExtension,
            "/Areas/{2}/Views/Shared/{0}" + ViewExtension,
            "/Views/Shared/{0}" + ViewExtension,
        };

        private readonly IVirtualPathViewFactory _virtualPathFactory;

        /// <summary>
        /// Initializes a new instance of the RazorViewEngine class.
        /// </summary>
        /// <param name="virtualPathFactory">The view factory used for instantiating Razor views.</param>
        public RazorViewEngine(IVirtualPathViewFactory virtualPathFactory)
        {
            _virtualPathFactory = virtualPathFactory;
        }

        /// <summary>
        /// Gets the formats used for view locations.
        /// The mapping of format tokens to route values are as follows:
        /// {0} - Controller Name, {1} - Action name, {2} - Area name.
        /// </summary>
        public IEnumerable<string> ViewLocationFormats
        {
            get { return _viewLocationFormats; }
        }

        /// <inheritdoc />
        public ViewEngineResult FindView([NotNull] ActionContext context,
                                         [NotNull] string viewName)
        {
            var viewEngineResult = CreateViewEngineResult(context, viewName);
            return viewEngineResult;
        }

        /// <inheritdoc />
        public ViewEngineResult FindPartialView([NotNull] ActionContext context,
                                                [NotNull] string partialViewName)
        {
            return FindView(context, partialViewName);
        }

        private ViewEngineResult CreateViewEngineResult(ActionContext context, string viewName)
        {
            var nameRepresentsPath = IsSpecificPath(viewName);

            if (nameRepresentsPath)
            {
                if (!viewName.EndsWith(ViewExtension, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        Resources.FormatViewMustEndInExtension(viewName, ViewExtension));
                }

                var view = _virtualPathFactory.CreateInstance(viewName);
                return view != null ? ViewEngineResult.Found(viewName, view) :
                                      ViewEngineResult.NotFound(viewName, new[] { viewName });
            }
            else
            {
                var routeValues = context.RouteData.Values;
                var controllerName = routeValues.GetValueOrDefault<string>("controller");
                var areaName = routeValues.GetValueOrDefault<string>("area");
                var potentialPaths = GetViewSearchPaths(viewName, controllerName, areaName);

                foreach (var path in potentialPaths)
                {
                    var view = _virtualPathFactory.CreateInstance(path);
                    if (view != null)
                    {
                        return ViewEngineResult.Found(viewName, view);
                    }
                }

                return ViewEngineResult.NotFound(viewName, potentialPaths);
            }
        }

        private static bool IsSpecificPath(string name)
        {
            return name[0] == '~' || name[0] == '/';
        }

        private IEnumerable<string> GetViewSearchPaths(string viewName, string controllerName, string areaName)
        {
            IEnumerable<string> unformattedPaths;

            if (string.IsNullOrEmpty(areaName))
            {
                // If no areas then no need to search area locations.
                unformattedPaths = _viewLocationFormats;
            }
            else
            {
                // If there's an area provided only search area view locations
                unformattedPaths = _areaViewLocationFormats;
            }

            var formattedPaths = unformattedPaths.Select(path =>
                string.Format(CultureInfo.InvariantCulture, path, viewName, controllerName, areaName));

            return formattedPaths;
        }
    }
}
