// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.ModelBinding.Metadata
{
    public class DisplayMetadata
    {
        public IDictionary<object, object> AdditionalValues { get; } = new Dictionary<object, object>();

        public bool ConvertEmptyStringToNull { get; set; } = true;

        public string DataTypeName { get; set; }

        public string Description { get; set; }

        public string DisplayFormatString { get; set; }

        public string DisplayName { get; set; }

        public string EditFormatString { get; set; }

        public bool HasNonDefaultEditFormat { get; set; }

        public bool HideSurroundingHtml { get; set; }

        public bool HtmlEncode { get; set; } = true;

        public string NullDisplayText { get; set; }

        public int Order { get; set; } = 10000;

        public bool ShowForDisplay { get; set; } = true;

        public bool ShowForEdit { get; set; } = true;

        public string SimpleDisplayProperty { get; set; }

        public string TemplateHint { get; set; }
    }
}