﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.ModelBinding.Metadata
{
    /// <summary>
    /// Display metadata details for a <see cref="ModelMetadata"/>.
    /// </summary>
    public class DisplayMetadata
    {
        /// <summary>
        /// Gets a set of additional values. See <see cref="ModelMetadata.AdditionalValues"/>
        /// </summary>
        public IDictionary<object, object> AdditionalValues { get; } = new Dictionary<object, object>();

        /// <summary>
        /// Gets or sets a value indicating whether or not empty strings should be treated as <c>null</c>.
        /// See <see cref="ModelMetadata.ConvertEmptyStringToNull"/>
        /// </summary>
        public bool ConvertEmptyStringToNull { get; set; } = true;

        /// <summary>
        /// Gets or sets the name of the data type.
        /// See <see cref="ModelMetadata.DataTypeName"/>
        /// </summary>
        public string DataTypeName { get; set; }

        /// <summary>
        /// Gets or sets a model description.
        /// See <see cref="ModelMetadata.Description"/>
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a display format string for the model.
        /// See <see cref="ModelMetadata.DisplayFormatString"/>
        /// </summary>
        public string DisplayFormatString { get; set; }

        /// <summary>
        /// Gets or sets a display name for the model.
        /// See <see cref="ModelMetadata.DisplayName"/>
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets an edit format string for the model.
        /// See <see cref="ModelMetadata.EditFormatString"/>
        /// </summary>
        /// <see cref="IDisplayMetadataProvider"/> instances that set this property to a non-<c>null</c>, non-empty,
        /// non-default value should also set <see cref="HasNonDefaultEditFormat"/> to <c>true</c>.
        /// </remarks>
        public string EditFormatString { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the model has a non-default edit format.
        /// See <see cref="ModelMetadata.HasNonDefaultEditFormat"/>
        /// </summary>
        public bool HasNonDefaultEditFormat { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the surrounding HTML should be hidden.
        /// See <see cref="ModelMetadata.HideSurroundingHtml"/>
        /// </summary>
        public bool HideSurroundingHtml { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the model value should be HTML encoded.
        /// See <see cref="ModelMetadata.HtmlEncode"/>
        /// </summary>
        public bool HtmlEncode { get; set; } = true;

        /// <summary>
        /// Gets or sets the text to display when the model value is null.
        /// See <see cref="ModelMetadata.NullDisplayText"/>
        /// </summary>
        public string NullDisplayText { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// See <see cref="ModelMetadata.Order"/>
        /// </summary>
        public int Order { get; set; } = 10000;

        /// <summary>
        /// Gets or sets a value indicating whether or not to include in the model value in display.
        /// See <see cref="ModelMetadata.ShowForDisplay"/>
        /// </summary>
        public bool ShowForDisplay { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not to include in the model value in an editor.
        /// See <see cref="ModelMetadata.ShowForEdit"/>
        /// </summary>
        public bool ShowForEdit { get; set; } = true;

        /// <summary>
        /// Gets or sets a the property name of a model property to use for display.
        /// See <see cref="ModelMetadata.SimpleDisplayProperty"/>
        /// </summary>
        public string SimpleDisplayProperty { get; set; }

        /// <summary>
        /// Gets or sets a hint for location of a display or editor template.
        /// See <see cref="ModelMetadata.TemplateHint"/>
        /// </summary>
        public string TemplateHint { get; set; }
    }
}