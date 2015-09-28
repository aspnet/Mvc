// Copyright (c) .NET Foundation. All rights reserved.
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
        /// Copies a user-provided attribute from <paramref name="context"/>'s
        /// <see cref="TagHelperContext.AllAttributes"/> to <paramref name="tagHelperOutput"/>'s
        /// <see cref="TagHelperOutput.Attributes"/>.
        /// </summary>
        /// <param name="tagHelperOutput">The <see cref="TagHelperOutput"/> this method extends.</param>
        /// <param name="attributeName">The name of the bound attribute.</param>
        /// <param name="context">The <see cref="TagHelperContext"/>.</param>
        /// <remarks>
        /// <para>
        /// Only copies the attribute if <paramref name="tagHelperOutput"/>'s
        /// <see cref="TagHelperOutput.Attributes"/> does not contain an attribute with the given
        /// <paramref name="attributeName"/>.
        /// </para>
        /// <para>
        /// Duplicate attributes same name in <paramref name="context"/>'s <see cref="TagHelperContext.AllAttributes"/>
        /// or <paramref name="tagHelperOutput"/>'s <see cref="TagHelperOutput.Attributes"/> may result in copied
        /// attribute order not being maintained.
        /// </para></remarks>
        public static void CopyHtmlAttribute(
            this TagHelperOutput tagHelperOutput,
            string attributeName,
            TagHelperContext context)
        {
            if (tagHelperOutput == null)
            {
                throw new ArgumentNullException(nameof(tagHelperOutput));
            }

            if (attributeName == null)
            {
                throw new ArgumentNullException(nameof(attributeName));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!tagHelperOutput.Attributes.ContainsName(attributeName))
            {
                var copiedAttribute = false;

                // We iterate context.AllAttributes backwards since we prioritize TagHelperOutput values occurring
                // before the current context.AllAttribtes[i].
                for (var i = context.AllAttributes.Count - 1; i >= 0; i--)
                {
                    // We look for the original attribute so we can restore the exact attribute name the user typed in
                    // approximately the same position where the user wrote it in the Razor source.
                    if (string.Equals(
                        attributeName,
                        context.AllAttributes[i].Name,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        CopyHtmlAttribute(i, tagHelperOutput, context);
                        copiedAttribute = true;
                    }
                }

                if (!copiedAttribute)
                {
                    throw new ArgumentException(
                        Resources.FormatTagHelperOutput_AttributeDoesNotExist(attributeName, nameof(TagHelperContext)),
                        nameof(attributeName));
                }
            }
        }

        /// <summary>
        /// Merges the given <paramref name="tagBuilder"/>'s <see cref="TagBuilder.Attributes"/> into the
        /// <paramref name="tagHelperOutput"/>.
        /// </summary>
        /// <param name="tagHelperOutput">The <see cref="TagHelperOutput"/> this method extends.</param>
        /// <param name="tagBuilder">The <see cref="TagBuilder"/> to merge attributes from.</param>
        /// <remarks>Existing <see cref="TagHelperOutput.Attributes"/> on the given <paramref name="tagHelperOutput"/>
        /// are not overridden; "class" attributes are merged with spaces.</remarks>
        public static void MergeAttributes(
            this TagHelperOutput tagHelperOutput,
            TagBuilder tagBuilder)
        {
            if (tagHelperOutput == null)
            {
                throw new ArgumentNullException(nameof(tagHelperOutput));
            }

            if (tagBuilder == null)
            {
                throw new ArgumentNullException(nameof(tagBuilder));
            }

            foreach (var attribute in tagBuilder.Attributes)
            {
                if (!tagHelperOutput.Attributes.ContainsName(attribute.Key))
                {
                    tagHelperOutput.Attributes.Add(attribute.Key, attribute.Value);
                }
                else if (attribute.Key.Equals("class", StringComparison.OrdinalIgnoreCase))
                {
                    TagHelperAttribute classAttribute;

                    if (tagHelperOutput.Attributes.TryGetAttribute("class", out classAttribute))
                    {
                        tagHelperOutput.Attributes["class"] = classAttribute.Value + " " + attribute.Value;
                    }
                    else
                    {
                        tagHelperOutput.Attributes.Add("class", attribute.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Removes the given <paramref name="attributes"/> from <paramref name="tagHelperOutput"/>'s
        /// <see cref="TagHelperOutput.Attributes"/>.
        /// </summary>
        /// <param name="tagHelperOutput">The <see cref="TagHelperOutput"/> this method extends.</param>
        /// <param name="attributes">Attributes to remove.</param>
        public static void RemoveRange(
            this TagHelperOutput tagHelperOutput,
            IEnumerable<TagHelperAttribute> attributes)
        {
            if (tagHelperOutput == null)
            {
                throw new ArgumentNullException(nameof(tagHelperOutput));
            }

            if (attributes == null)
            {
                throw new ArgumentNullException(nameof(attributes));
            }

            foreach (var attribute in attributes.ToArray())
            {
                tagHelperOutput.Attributes.Remove(attribute);
            }
        }

        private static void CopyHtmlAttribute(
            int allAttributeIndex,
            TagHelperOutput tagHelperOutput,
            TagHelperContext context)
        {
            var existingAttribute = context.AllAttributes[allAttributeIndex];
            var copiedAttribute = new TagHelperAttribute
            {
                Name = existingAttribute.Name,
                Value = existingAttribute.Value,
                Minimized = existingAttribute.Minimized
            };

            // Move backwards through context.AllAttributes from the provided index until we find a familiar attribute
            // in tagHelperOutput where we can insert the copied value after the familiar one.
            for (var i = allAttributeIndex - 1; i >= 0; i--)
            {
                var previousName = context.AllAttributes[i].Name;
                var index = IndexOfFirstMatch(previousName, tagHelperOutput.Attributes);
                if (index != -1)
                {
                    tagHelperOutput.Attributes.Insert(index + 1, copiedAttribute);
                    return;
                }
            }

            // Move forward through context.AllAttributes from the provided index until we find a familiar attribute in
            // tagHelperOutput where we can insert the copied value.
            for (var i = allAttributeIndex + 1; i < context.AllAttributes.Count; i++)
            {
                var nextName = context.AllAttributes[i].Name;
                var index = IndexOfFirstMatch(nextName, tagHelperOutput.Attributes);
                if (index != -1)
                {
                    tagHelperOutput.Attributes.Insert(index, copiedAttribute);
                    return;
                }
            }

            // Couldn't determine the attribute's location, add it to the end.
            tagHelperOutput.Attributes.Add(copiedAttribute);
        }

        private static int IndexOfFirstMatch(string name, TagHelperAttributeList attributes)
        {
            for (var i = 0; i < attributes.Count; i++)
            {
                if (string.Equals(name, attributes[i].Name, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}