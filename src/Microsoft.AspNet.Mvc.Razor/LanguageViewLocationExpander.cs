// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// A <see cref="IViewLocationExpander"/> that adds the language as an extension prefix to view names. Language
    /// that is getting added as extension prefix comes from <see cref="Microsoft.AspNet.Http.HttpContext"/>.
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
        private const string ValueKey = "language";

        /// <inheritdoc />
        public void PopulateValues([NotNull] ViewLocationExpanderContext context)
        {
            // Using CurrentUICulture so it loads the locale specific resources for the views.
#if DNX451
            context.Values[ValueKey] = Thread.CurrentThread.CurrentUICulture.Name;
#else
            context.Values[ValueKey] = CultureInfo.CurrentUICulture.Name;
#endif
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> ExpandViewLocations([NotNull] ViewLocationExpanderContext context,
                                                               [NotNull] IEnumerable<string> viewLocations)
        {
            string value;
            context.Values.TryGetValue(ValueKey, out value);

            try
            {
                if (!string.IsNullOrEmpty(value))
                {
                    return ExpandViewLocationsCore(viewLocations, new CultureInfo(value));
                }
            }
            catch(CultureNotFoundException)
            {
                // Do nothing. Just returns viewLocations at the end of this method.
            }

            return viewLocations;
        }

        private IEnumerable<string> ExpandViewLocationsCore(IEnumerable<string> viewLocations, CultureInfo cultureInfo)
        {
            foreach (var location in viewLocations)
            {
                var temporaryCultureInfo = cultureInfo;

                while (temporaryCultureInfo != temporaryCultureInfo.Parent)
                {
                    yield return location.Replace("{0}", temporaryCultureInfo.Name + "/{0}");

                    temporaryCultureInfo = temporaryCultureInfo.Parent;
                }

                yield return location;
            }
        }
    }
}