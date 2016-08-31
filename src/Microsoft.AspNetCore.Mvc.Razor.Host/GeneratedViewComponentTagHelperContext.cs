// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Mvc.Razor
{
    /// <summary>
    /// Contains information for generating view component <see cref="AspNetCore.Razor.TagHelpers.TagHelper"/> classes.
    /// </summary>
    public class GeneratedViewComponentTagHelperContext
    {
        /// <summary>
        /// Sets the names.
        /// </summary>
        public GeneratedViewComponentTagHelperContext()
        {
            ContextualizeMethodName = "Contextualize";
            InvokeAsyncMethodName = "InvokeAsync";
            IViewComponentHelperTypeName = "Microsoft.AspNetCore.Mvc.IViewComponentHelper";
            IViewContextAwareTypeName = "Microsoft.AspNetCore.Mvc.ViewFeatures.IViewContextAware";
            ViewContextTypeName = "Microsoft.AspNetCore.Mvc.Rendering.ViewContext";
        }

        /// <summary>
        /// Name of the Contextualize method.
        /// </summary>
        public string ContextualizeMethodName { get; set; }

        /// <summary>
        /// Name of the InvokeAsync method.
        /// </summary>
        public string InvokeAsyncMethodName { get; set; }

        /// <summary>
        /// Name of the IViewComponentHelper type. 
        /// </summary>
        public string IViewComponentHelperTypeName { get; set; }

        /// <summary>
        /// Name of the IViewContextAware type.
        /// </summary>
        public string IViewContextAwareTypeName { get; set; }

        /// <summary>
        /// Name of the ViewContext type.
        /// </summary>
        public string ViewContextTypeName { get; set; }
    }
}