// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Mvc.Razor
{
    /// <summary>
    /// Contains necessary information for the tag helper code generation process.
    /// </summary>
    public class GeneratedViewComponentTagHelperContext
    {
        public GeneratedViewComponentTagHelperContext()
        {
            TagHelpersNamespace = "Microsoft.AspNetCore.Razor.TagHelpers";
            IViewComponentHelperType = "IViewComponentHelper";
            IViewContextAwareType = "IViewContextAware";
            TagStructureType = "TagStructure";
            ViewContextType = "Microsoft.AspNetCore.Mvc.Rendering.ViewContext";

            ContextualizeMethod = "Contextualize";
            InvokeAsyncMethod = "InvokeAsync";
        }

        public string ContextualizeMethod { get; set; }
        public string InvokeAsyncMethod { get; set; }
        public string TagHelpersNamespace { get; set; }
        public string IViewComponentHelperType { get; set; }
        public string IViewContextAwareType { get; set; }
        public string TagStructureType { get; set; }
        public string ViewContextType { get; set; }
    }
}