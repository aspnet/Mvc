// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.TestCommon;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.TagHelpers
{
    public class TagHelperAttributeStringComparer : IEqualityComparer<TagHelperAttribute>
    {
        private TagHelperAttributeStringComparer()
        {
        }

        public static TagHelperAttributeStringComparer Instance { get; } = new TagHelperAttributeStringComparer();

        public bool Equals(TagHelperAttribute x, TagHelperAttribute y)
        {
            if (x == null ^ y == null)
            {
                return false;
            }

            if (x == null)
            {
                return true;
            }

            if (!string.Equals(x.Name, y.Name, StringComparison.Ordinal))
            {
                return false;
            }

            if (x.ValueStyle != y.ValueStyle)
            {
                return false;
            }

            if (x.ValueStyle == HtmlAttributeValueStyle.Minimized)
            {
                return true;
            }

            var xString = GetValueString(x.Value);
            var yString = GetValueString(y.Value);
            return string.Equals(xString, yString, StringComparison.Ordinal);
        }

        private string GetValueString(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var stringValue = value as string;
            if (stringValue != null)
            {
                return stringValue;
            }

            var htmlContent = value as IHtmlContent;
            if (htmlContent != null)
            {
                // Use NullHtmlEncoder for consistency w/ `string` handling. `string`s will be encoded later.
                return HtmlContentUtilities.HtmlContentToString(htmlContent, NullHtmlEncoder.Default);
            }

            return value.ToString();
        }

        public int GetHashCode(TagHelperAttribute obj)
        {
            return obj.GetHashCode();
        }
    }
}
