﻿using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.Rendering
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

        public static ViewEngineResult NotFound([NotNull] string viewName, [NotNull] IEnumerable<string> searchedLocations)
        {
            return new ViewEngineResult
            {
                SearchedLocations = searchedLocations,
                ViewName = viewName,
            };
        }

        public static ViewEngineResult Found([NotNull] string viewName, [NotNull] IView view)
        {
            return new ViewEngineResult
            {
                View = view,
                ViewName = viewName,
            };
        }

        public void ThrowIfViewNotFound()
        {
            if (Success)
            {
                return;
            }

            var locationsText = Environment.NewLine + string.Join(Environment.NewLine, SearchedLocations);
            throw new InvalidOperationException(Resources.FormatViewEngine_ViewNotFound(
                ViewName,
                 locationsText));
        }
    }
}
