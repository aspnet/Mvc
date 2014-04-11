// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    /// <summary>
    /// Represents the optgroup HTML element and its attributes.
    /// In a select list, multiple groups with the same name are supported.
    /// They are compared with reference equality.
    /// </summary>
    public class SelectListGroup
    {
        /// <summary>
        /// Gets or sets a value that indicates whether this <see cref="SelectListGroup"/> is disabled. 
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Represents the value of the optgroup's label.
        /// </summary>
        public string Name { get; set; }
    }
}
