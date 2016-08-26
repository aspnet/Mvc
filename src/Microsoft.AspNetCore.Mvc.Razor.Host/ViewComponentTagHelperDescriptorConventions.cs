﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.Razor.Host
{
    /// <summary>
    /// A library of methods used to generate <see cref="TagHelperDescriptor"/>s for view components.
    /// </summary>
    public static class ViewComponentTagHelperDescriptorConventions
    {
        /// <summary>
        /// The key in a <see cref="TagHelperDescriptor"/> property bag containing 
        /// the short name of a view component.
        /// </summary>
        public static readonly string ViewComponentNameKey = "ViewComponentName";

        /// <summary>
        /// The key in a <see cref="TagHelperDescriptor"/> property bag containing
        /// a custom type name for a view component's tag helper representation.
        /// </summary>
        public static readonly string ViewComponentTagHelperNameKey = "ViewComponentTagHelperName";

        /// <summary>
        /// Each custom type name for a view component's tag helper representation will begin with this header.
        /// </summary>
        public static readonly string ViewComponentTagHelperNameHeader = "__Generated__";

        /// <summary>
        /// Each custom type name for a view component's tag helper representation will end with this footer.
        /// </summary>
        public static readonly string ViewComponentTagHelperNameFooter = "ViewComponentTagHelper";

        /// <summary>
        /// Verifies whether a <see cref="TagHelperDescriptor"/> represents a view component.
        /// </summary>
        /// <param name="descriptor">The <see cref="TagHelperDescriptor"/> to check.</param>
        /// <returns>Whether a <see cref="TagHelperDescriptor"/> represents a view component.</returns>
        public static bool IsViewComponentDescriptor(TagHelperDescriptor descriptor)
        {
            return (descriptor != null &&
                descriptor.PropertyBag.ContainsKey(ViewComponentNameKey)
                && descriptor.PropertyBag.ContainsKey(ViewComponentTagHelperNameKey));
        }

        /// <summary>
        /// Retrieves the view component name from the tag helper property bag.
        /// </summary>
        /// <param name="descriptor">The <see cref="TagHelperDescriptor"/>.</param>
        /// <returns>The short name of the view component represented by the <see cref="TagHelperDescriptor"/>,
        /// or null if the tag helper does not represent a view component.</returns>
        public static string GetViewComponentName(TagHelperDescriptor descriptor)
        {
            if (!IsViewComponentDescriptor(descriptor))
            {
                return null;
            }

            var viewComponentName = descriptor.PropertyBag[ViewComponentNameKey];
            return viewComponentName;
        }

        /// <summary>
        /// Retrieves the view component tag helper name from the tag helper property bag.
        /// </summary>
        /// <param name="descriptor">The <see cref="TagHelperDescriptor"/>.</param>
        /// <returns>The custom type name for a view component represented by the <see cref="TagHelperDescriptor"/>,
        /// or null if the tag helper does not represent a view component.</returns>
        public static string GetViewComponentTagHelperName(TagHelperDescriptor descriptor)
        {
            if (!IsViewComponentDescriptor(descriptor))
            {
                return null;
            }

            var viewComponentTagHelperName = descriptor.PropertyBag[ViewComponentTagHelperNameKey];
            return viewComponentTagHelperName;
        }
    }
}