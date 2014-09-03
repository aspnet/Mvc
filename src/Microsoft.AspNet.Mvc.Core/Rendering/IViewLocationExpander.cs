// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.Rendering
{
    /// <summary>
    /// Specifies the contracts for a view location expander that is used by <see cref="IViewEngine"/> instances to
    /// determine search paths for a view.
    /// </summary>
    /// <remarks>
    /// Individual <see cref="IViewLocationExpander"/>s are invoked in two steps:
    /// (1) <see cref="PopulateValuesAsync(ActionContext, IDictionary{string, string})"/> is invoked and each expander
    /// adds values that it would later consume as part of 
    /// <see cref="ExpandViewLocations(IImmutableList{string}, IDictionary{string, string})"/>.
    /// If the populated values are used to determine a cache key - if all values are identical to the last time 
    /// <see cref="PopulateValuesAsync(ActionContext, IDictionary{string, string})"/> was invoked, the cached result
    /// is used as the view location.
    /// (2) If no result was found in the cache or if a view was not found at the cached location, 
    /// <see cref="ExpandViewLocations(IImmutableList{string}, IDictionary{string, string})"/> is invoked to determine 
    /// all potential paths for a view.
    /// </remarks>
    public interface IViewLocationExpander
    {
        /// <summary>
        /// Invoked by a <see cref="IViewEngine"/> to determine the values that would be consumed by this instance of
        /// <see cref="IViewLocationExpander"/>. The calculated values are used to determine if the view location has
        /// changed since the last time it was located.
        /// </summary>
        /// <param name="actionContext">The <see cref="ActionContext"/> for the current executing action.</param>
        /// <param name="values">The <see cref="IDictionary{TKey, TValue}"/> to add interesting values to.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the action of populating the <paramref name="values"/> 
        /// dictionary.
        /// </returns>
        Task PopulateValuesAsync(ActionContext actionContext, IDictionary<string, string> values);

        /// <summary>
        /// Invoked by a <see cref="IViewEngine"/> to determine potential locations for a view.
        /// </summary>
        /// <param name="viewLocations">The list of view locations to use as seed for generating additional locations.
        /// </param>
        /// <param name="values">The dictionary with values populated during 
        /// <see cref="PopulateValuesAsync(ActionContext, IDictionary{string, string})"/></param>
        /// <returns>A list of expanded view locations.</returns>
        IImmutableList<string> ExpandViewLocations(IImmutableList<string> viewLocations,
                                                   IDictionary<string, string> values);
    }
}