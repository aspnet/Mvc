// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Mvc
{
    public class SelectListItem
    {
        /// <summary>
        /// Gets or sets a value that indicates whether this <see cref="SelectListItem"/> is disabled.
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Represents the optgroup HTML element this item is wrapped into.
        /// In a select list, multiple groups with the same name are supported.
        /// They are compared with reference equality.
        /// </summary>
        public SelectListGroup Group { get; set; }

        public bool Selected { get; set; }

        public string Text { get; set; }

        public string Value { get; set; }
    }
}
