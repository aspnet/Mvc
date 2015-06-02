// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// A <see cref="IViewLocationExpander"/> that adds the language as an extension prefix to view names.
    /// </summary>
    /// <example>
    /// For the default case with no areas, views are generated with the following patterns (assuming controller is
    /// "Home", action is "Index" and language is "en")
    /// Views/Home/en/Action
    /// Views/Home/Action
    /// Views/Shared/en/Action
    /// Views/Shared/Action
    /// </example>
    public class LanguageViewLocationExpander : IViewLocationExpander
    {
        /// <inheritdoc />
        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context,
                                                               IEnumerable<string> viewLocations)
        {
#if DNX451
            var cultureInfo = Thread.CurrentThread.CurrentUICulture;
#else
            var cultureInfo = CultureInfo.CurrentUICulture;
#endif

            var updatedViewLocations = new List<string>();
            while (cultureInfo != cultureInfo.Parent)
            {
                updatedViewLocations.AddRange(ExpandViewLocationsCore(viewLocations, cultureInfo.Name));
                cultureInfo = cultureInfo.Parent;
            }

            if (updatedViewLocations.Any())
            {
                updatedViewLocations.AddRange(viewLocations);
                return updatedViewLocations;
            }

            return viewLocations;
        }

        private IEnumerable<string> ExpandViewLocationsCore(IEnumerable<string> viewLocations,
                                                            string value)
        {
            foreach (var location in viewLocations)
            {
                yield return location.Replace("{0}", value + "/{0}");
            }
        }
    }
}