// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
// for doc comments
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    /// <summary>
    /// Represents the parameters of an <see cref="IViewEngine"/> when searching for a view.
    /// </summary>
    public class ViewEngineValues : LoggerStructureBase
    {
        public ViewEngineValues(
            string requestedView, 
            bool partial, 
            string viewEngine, 
            ActionContext actionContext, 
            IEnumerable<string> searchedLocations, 
            bool found, 
            bool? cached = null)
        {
            RequestedView = requestedView;
            IsPartial = partial;
            ViewEngineTypeName = viewEngine;
            IsFound = found;
            ControllerType = actionContext.Controller?.GetType();
            IsCached = cached;
            ActionDescriptor = new ActionDescriptorValues(actionContext.ActionDescriptor);
        }

        /// <summary>
        /// The name or full path to the view the view engine is searching for.
        /// </summary>
        public string RequestedView { get; }

        /// <summary>
        /// The <see cref="ActionDescriptorValues"/> representing the action requesting the view.
        /// </summary>
        public ActionDescriptorValues ActionDescriptor { get; }

        /// <summary>
        /// Indicates whether the requested view is a partial view.
        /// </summary>
        public bool IsPartial { get; }

        /// <summary>
        /// The <see cref="Type"/> of the controller for the ActionDescriptor requesting the view.
        /// </summary>
        public Type ControllerType { get; }

        /// <summary>
        /// The type name of the view engine finding the view.
        /// </summary>
        public string ViewEngineTypeName { get; }

        /// <summary>
        /// An enumerable of all the locations that were searched to find the requested view.
        /// </summary>
        public IEnumerable<string> SearchedLocations { get; }

        /// <summary>
        /// Indicates whether the requested view was found.
        /// </summary>
        public bool IsFound { get; }

        /// <summary>
        /// Indicates whether the requested view was found in a cached location. Null if IsFound is false.
        /// </summary>
        public bool? IsCached { get; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}