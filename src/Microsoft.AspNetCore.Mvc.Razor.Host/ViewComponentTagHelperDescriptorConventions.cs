﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Evolution;

namespace Microsoft.AspNetCore.Mvc.Razor.Host
{
    /// <summary>
    /// A library of methods used to generate <see cref="TagHelperDescriptor"/>s for view components.
    /// </summary>
    public static class ViewComponentTagHelperDescriptorConventions
    {
        /// <summary>
        /// The key in a <see cref="TagHelperDescriptor.PropertyBag"/>  containing 
        /// the short name of a view component.
        /// </summary>
        public static readonly string ViewComponentNameKey = "ViewComponentName";

        /// <summary>
        /// Indicates whether a <see cref="TagHelperDescriptor"/> represents a view component.
        /// </summary>
        /// <param name="descriptor">The <see cref="TagHelperDescriptor"/> to check.</param>
        /// <returns>Whether a <see cref="TagHelperDescriptor"/> represents a view component.</returns>
        public static bool IsViewComponentDescriptor(TagHelperDescriptor descriptor) =>
            descriptor != null && descriptor.PropertyBag.ContainsKey(ViewComponentNameKey);
    }
}