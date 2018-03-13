// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Properties decorated with <see cref="ViewDataAttribute"/> will have their values stored in
    /// and loaded from the <see cref="ViewFeatures.ViewDataDictionary"/>.
    /// <para>
    /// <see cref="ViewDataAttribute"/> is supported on properties of Controllers, and Razor Page Models.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class ViewDataAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the key for the current property when storing or reading from <see cref="ViewFeatures.TempDataDictionary"/>.
        /// </summary>
        /// <remarks>
        /// When <c>null</c>, the default value of the key is the property name.
        /// </remarks>
        public string Key { get; set; }
    }
}
