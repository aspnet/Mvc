// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
    /// <summary>
    /// Utility related extensions for <see cref="TagHelperOutput"/>.
    /// </summary>
    public static class TagHelperOutputExtensions
    {
        /// <summary>
        /// Merges the given <paramref name="tagBuilder"/> into the <paramref name="tagHelperOutput"/>.
        /// </summary>
        /// <param name="tagHelperOutput">The <see cref="TagHelperOutput"/>.</param>
        /// <param name="tagBuilder">The <see cref="TagBuilder"/> to merge.</param>
        public static void Merge(this TagHelperOutput tagHelperOutput, TagBuilder tagBuilder)
        {
            tagHelperOutput.TagName = tagBuilder.TagName;
            tagHelperOutput.Content += tagBuilder.InnerHtml;

            MergeAttributes(tagHelperOutput, tagBuilder);
        }

        /// <summary>
        /// Merges the given <see cref="tagBuilder"/>'s <see cref="TagBuilder.Attributes"/> into the 
        /// <paramref name="tagHelperOutput"/>.
        /// </summary>
        /// <param name="tagHelperOutput">The <see cref="TagHelperOutput"/>.</param>
        /// <param name="tagBuilder">The <see cref="TagBuilder"/> to merge attributes from.</param>
        public static void MergeAttributes(this TagHelperOutput tagHelperOutput, TagBuilder tagBuilder)
        {
            foreach (var attribute in tagBuilder.Attributes)
            {
                if (!tagHelperOutput.Attributes.ContainsKey(attribute.Key))
                {
                    tagHelperOutput.Attributes.Add(attribute.Key, attribute.Value);
                }
                else if (attribute.Key.Equals("class", StringComparison.Ordinal))
                {
                    tagHelperOutput.Attributes["class"] += " " + attribute.Value;
                }
            }
        }

        /// <summary>
        /// Returns and removes all attributes from <paramref name="tagHelperOutput"/>'s 
        /// <see cref="TagHelperOutput.Attributes"/> that have the given <paramref name="prefix"/>.
        /// </summary>
        /// <param name="tagHelperOutput">The <see cref="TagHelperOutput"/>.</param>
        /// <param name="prefix">The prefix to </param>
        /// <returns><see cref="KeyValuePair{string, string}"/>s whos <see cref="KeyValuePair{string, string}.Key"/>
        /// starts with the given <paramref name="prefix"/>.</returns>
        public static IEnumerable<KeyValuePair<string, string>> PullPrefixedAttributes(
            this TagHelperOutput tagHelperOutput, string prefix)
        {
            // TODO: We will not need this method once https://github.com/aspnet/Razor/issues/89 is completed.

            var htmlAttributes = tagHelperOutput.Attributes;

            // We're only interested in HTML attributes that have the desired prefix.
            var prefixedAttributes = htmlAttributes.Where(attribute =>
                attribute.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToArray();

            // Since we're "pulling" the prefixed attribute values, we need to remove them.
            foreach (var attribute in prefixedAttributes)
            {
                htmlAttributes.Remove(attribute.Key);
            }

            return prefixedAttributes;
        }

        /// <summary>
        /// Restores a user provided bound attribute to the given <paramref name="tagHelperOutput"/>.
        /// </summary>
        /// <param name="tagHelperOutput">The <see cref="TagHelperOutput"/>.</param>
        /// <param name="boundAttributeName">The name of the bound attribute.</param>
        /// <param name="context">The <see cref="TagHelperContext"/>.</param>
        public static void RestoreBoundHtmlAttribute(this TagHelperOutput tagHelperOutput, 
                                                     string boundAttributeName, 
                                                     TagHelperContext context)
        {
            // We look for the original attribute so we can restore the exact attribute name the user typed.
            var entry = context.AllAttributes.Single(attribute =>
                attribute.Key.Equals(boundAttributeName, StringComparison.OrdinalIgnoreCase));
            var originalAttribute = new KeyValuePair<string, string>(entry.Key, entry.Value.ToString());

            tagHelperOutput.Attributes.Add(originalAttribute);
        }
    }
}