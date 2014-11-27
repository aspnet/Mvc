// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    /// <summary>
    /// Represents the parameters of a <see cref="IViewEngine"/> when searching for a view. Contains the 
    /// requested view, whether it's a partial view, the view engine rendering the view, the action, 
    /// the controller of the action,  whether the view was found, the locations searched, and whether 
    /// or not the view was cached.
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
            Partial = partial;
            ViewEngineTypeName = viewEngine;
            Found = found;
            ControllerName = actionContext.Controller?.ToString();
            Cached = cached;
            ActionDescriptor = new ActionDescriptorValues(actionContext.ActionDescriptor);
        }

        public string RequestedView { get; }

        public ActionDescriptorValues ActionDescriptor { get; }

        public bool Partial { get; }

        public string ControllerName { get; }

        public string ViewEngineTypeName { get; }

        public IEnumerable<string> SearchedLocations { get; }

        public bool Found { get; }

        public bool? Cached { get; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}