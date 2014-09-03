// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNet.Http;

namespace Microsoft.AspNet.Mvc.Rendering
{
    /// <summary>
    /// A <see cref="IViewLocationExpander"/> that replaces adds the language as an extension prefix to view names.
    /// </summary>
    /// <remarks>
    /// For the default case with no areas, views are generated with the following patterns (assuming controller is
    /// "Home", action is "Index" and language is "en")
    /// Views/Home/Action.en
    /// Views/Home/Action
    /// Views/Shared/Action.en
    /// Views/Shared/Action
    /// </remarks>
    public class LanguageViewLocationExpander : IViewLocationExpander
    {
        private const string ValueKey = "language";
        private readonly Func<ActionContext, string> _valueFactory;

        /// <summary>
        /// Initailizes a new instance of <see cref="LanguageViewLocationExpander"/>.
        /// </summary>
        /// <param name="valueFactory">A factory that provides</param>
        public LanguageViewLocationExpander(Func<ActionContext, string> valueFactory)
        {
            _valueFactory = valueFactory;
        }

        /// <inheritdoc />
        public void PopulateValues([NotNull] ViewLocationExpanderContext context)
        {
            var value = _valueFactory(context.ActionContext);
            if (!string.IsNullOrEmpty(value))
            {
                context.Values[ValueKey] = value;
            }
        }

        /// <inheritdoc />
        public virtual IEnumerable<string> ExpandViewLocations([NotNull] ViewLocationExpanderContext context,
                                                               [NotNull] IEnumerable<string> viewLocations)
        {
            if (context.Values.TryGetValue(ValueKey, out var value))
            {
                return ExpandViewLocationsCore(viewLocations, value);
            }

            return viewLocations;
        }

        private IEnumerable<string> ExpandViewLocationsCore(IEnumerable<string> viewLocations,
                                                            string value)
        {
            foreach (var location in viewLocations)
            {
                yield return location.Replace("{0}", value + "/{0}");
                yield return location;
            }
        }
    }
}