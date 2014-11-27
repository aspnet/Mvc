// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    /// <summary>
    /// Represents the parameters of a <see cref="IViewEngine"/> when searching for a view. Contains the 
    /// requested view, whether it's a partial view, the view engine rendering the view, the controller of the action, 
    /// whether the view was found, the locations searched, and whether or not the view was cached.
    /// </summary>
    public class ViewEngineValues : LoggerStructureBase
    {
        public ViewEngineValues(string requestedView, bool partial, string viewEngine, 
            ActionContext actionContext, IEnumerable<string> searchedLocations, bool found, bool? cached = null)
        {
            RequestedView = requestedView;
            Partial = partial;
            ViewEngine = viewEngine;
            Found = found;
            Controller = actionContext.Controller?.ToString();
            Cached = cached;
        }

        public string RequestedView { get; }

        // TODO: uncomment when aspnet/Mvc#1600 is done
        // public ActionDescriptorValues ActionDescriptor { get; }

        // TODO: figure out what information to pick out from ActionContext

        public bool Partial { get; }

        public string Controller { get; }

        public string ViewEngine { get; }

        public IEnumerable<string> SearchedLocations { get; }

        public bool Found { get; }

        public bool? Cached { get; }

        public override string Format()
        {
            // TODO: uncomment when aspnet/Mvc#1600 is done
            // return LogFormatter.FormatStructure(this);
            return ViewEngine + " " + RequestedView + " " + Found;
        }
    }
}