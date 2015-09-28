// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.ViewFeatures;

namespace Microsoft.AspNet.Mvc.ViewEngines
{
    public class ViewEngineResult
    {
        private ViewEngineResult()
        {
        }

        public IEnumerable<string> SearchedLocations { get; private set; }

        public IView View { get; private set; }

        public string ViewName { get; private set; }

        public bool Success
        {
            get { return View != null; }
        }

        public static ViewEngineResult NotFound(
            string viewName,
            IEnumerable<string> searchedLocations)
        {
            if (viewName == null)
            {
                throw new ArgumentNullException(nameof(viewName));
            }

            if (searchedLocations == null)
            {
                throw new ArgumentNullException(nameof(searchedLocations));
            }

            return new ViewEngineResult
            {
                SearchedLocations = searchedLocations,
                ViewName = viewName,
            };
        }

        public static ViewEngineResult Found(string viewName, IView view)
        {
            if (viewName == null)
            {
                throw new ArgumentNullException(nameof(viewName));
            }

            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            return new ViewEngineResult
            {
                View = view,
                ViewName = viewName,
            };
        }

        public ViewEngineResult EnsureSuccessful()
        {
            if (!Success)
            {
                var locations = string.Empty;
                if (SearchedLocations != null)
                {
                    locations = Environment.NewLine + string.Join(Environment.NewLine, SearchedLocations);
                }

                throw new InvalidOperationException(Resources.FormatViewEngine_ViewNotFound(ViewName, locations));
            }

            return this;
        }
    }
}
