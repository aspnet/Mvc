// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Microsoft.AspNetCore.Mvc.Rendering
{
    /// <summary>
    /// Contains methods and properties that are used to create HTML elements. This class is often used to write HTML
    /// helpers and tag helpers.
    /// </summary>
    [DebuggerDisplay("{DebuggerToString()}")]
    public class TagBuilder : IHtmlContent
    {
        private AttributeDictionary _attributes;
        private HtmlContentBuilder _innerHtml;

        /// <summary>
        /// Creates a new HTML tag that has the specified tag name.
        /// </summary>
        /// <param name="tagName">An HTML tag name.</param>
        public TagBuilder(string tagName)
        {
            if (string.IsNullOrEmpty(tagName))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(tagName));
            }

            TagName = tagName;
        }

        /// <summary>
        /// Gets the set of attributes that will be written to the tag.
        /// </summary>
        public AttributeDictionary Attributes
        {
            get
            {
                // Perf: Avoid allocating `_attributes` if possible
                if (_attributes == null)
                {
                    _attributes = new AttributeDictionary();
                }

                return _attributes;
            }
        }

        /// <summary>
        /// Gets the inner HTML content of the element.
        /// </summary>
        public IHtmlContentBuilder InnerHtml
        {
            get
            {
                if (_innerHtml == null)
                {
                    _innerHtml = new HtmlContentBuilder();
                }

                return _innerHtml;
            }
        }

        /// <summary>
        /// Gets an indication <see cref="InnerHtml"/> is not empty.
        /// </summary>
        public bool HasInnerHtml => _innerHtml?.Count > 0;

        /// <summary>
        /// Gets the tag name for this tag.
        /// </summary>
        public string TagName { get; }

        /// <summary>
        /// The <see cref="Rendering.TagRenderMode"/> with which the tag is written.
        /// </summary>
        /// <remarks>Defaults to <see cref="TagRenderMode.Normal"/>.</remarks>
        public TagRenderMode TagRenderMode { get; set; } = TagRenderMode.Normal;

        /// <summary>
        /// Adds a CSS class to the list of CSS classes in the tag.
        /// If there are already CSS classes on the tag then a space character and the new class will be appended to
        /// the existing list.
        /// </summary>
        /// <param name="value">The CSS class name to add.</param>
        public void AddCssClass(string value)
        {
            string currentValue;

            if (Attributes.TryGetValue("class", out currentValue))
            {
                Attributes["class"] = value + " " + currentValue;
            }
            else
            {
                Attributes["class"] = value;
            }
        }

        /// <summary>
        /// Returns a valid HTML 4.01 "id" attribute value for an element with the given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">
        /// The fully-qualified expression name, ignoring the current model. Also the original HTML element name.
        /// </param>
        /// <param name="invalidCharReplacement">
        /// The <see cref="string"/> (normally a single <see cref="char"/>) to substitute for invalid characters in
        /// <paramref name="name"/>.
        /// </param>
        /// <returns>
        /// Valid HTML 4.01 "id" attribute value for an element with the given <paramref name="name"/>.
        /// </returns>
        /// <remarks>
        /// Valid "id" attributes are defined in https://www.w3.org/TR/html401/types.html#type-id.
        /// </remarks>
        public static string CreateSanitizedId(string name, string invalidCharReplacement)
        {
            if (invalidCharReplacement == null)
            {
                throw new ArgumentNullException(nameof(invalidCharReplacement));
            }

            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            // If there are no invalid characters in the string, then we don't have to create the buffer.
            var firstIndexOfInvalidCharacter = 1;
            for (; firstIndexOfInvalidCharacter < name.Length; firstIndexOfInvalidCharacter++)
            {
                if (!Html401IdUtil.IsValidIdCharacter(name[firstIndexOfInvalidCharacter]))
                {
                    break;
                }
            }

            var firstChar = name[0];
            var startsWithAsciiLetter = Html401IdUtil.IsAsciiLetter(firstChar);
            if (!startsWithAsciiLetter)
            {
                // The first character must be a letter according to the HTML 4.01 specification.
                firstChar = 'z';
            }

            if (firstIndexOfInvalidCharacter == name.Length && startsWithAsciiLetter)
            {
                return name;
            }

            var stringBuffer = new StringBuilder(name.Length);
            stringBuffer.Append(firstChar);

            // Characters until 'firstIndexOfInvalidCharacter' have already been checked for validity.
            // So just copy them. This avoids running them through Html401IdUtil.IsValidIdCharacter again.
            for (var index = 1; index < firstIndexOfInvalidCharacter; index++)
            {
                stringBuffer.Append(name[index]);
            }

            for (var index = firstIndexOfInvalidCharacter; index < name.Length; index++)
            {
                var thisChar = name[index];
                if (Html401IdUtil.IsValidIdCharacter(thisChar))
                {
                    stringBuffer.Append(thisChar);
                }
                else
                {
                    stringBuffer.Append(invalidCharReplacement);
                }
            }

            return stringBuffer.ToString();
        }

        /// <summary>
        /// Adds a valid HTML 4.01 "id" attribute for an element with the given <paramref name="name"/>. Does
        /// nothing if <see cref="Attributes"/> already contains an "id" attribute or the <paramref name="name"/>
        /// is <c>null</c> or empty.
        /// </summary>
        /// <param name="name">
        /// The fully-qualified expression name, ignoring the current model. Also the original HTML element name.
        /// </param>
        /// <param name="invalidCharReplacement">
        /// The <see cref="string"/> (normally a single <see cref="char"/>) to substitute for invalid characters in
        /// <paramref name="name"/>.
        /// </param>
        /// <seealso cref="CreateSanitizedId(string, string)"/>
        public void GenerateId(string name, string invalidCharReplacement)
        {
            if (invalidCharReplacement == null)
            {
                throw new ArgumentNullException(nameof(invalidCharReplacement));
            }

            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            if (!Attributes.ContainsKey("id"))
            {
                var sanitizedId = CreateSanitizedId(name, invalidCharReplacement);

                // Duplicate check for null or empty to cover the corner case where name contains only invalid
                // characters and invalidCharReplacement is empty.
                if (!string.IsNullOrEmpty(sanitizedId))
                {
                    Attributes["id"] = sanitizedId;
                }
            }
        }

        private void AppendAttributes(TextWriter writer, HtmlEncoder encoder)
        {
            // Perf: Avoid allocating enumerator for `_attributes` if possible
            if (_attributes != null && _attributes.Count > 0)
            {
                foreach (var attribute in Attributes)
                {
                    var key = attribute.Key;
                    if (string.Equals(key, "id", StringComparison.OrdinalIgnoreCase) &&
                        string.IsNullOrEmpty(attribute.Value))
                    {
                        continue;
                    }

                    writer.Write(" ");
                    writer.Write(key);
                    writer.Write("=\"");
                    if (attribute.Value != null)
                    {
                        encoder.Encode(writer, attribute.Value);
                    }

                    writer.Write("\"");
                }
            }
        }

        public void MergeAttribute(string key, string value)
        {
            MergeAttribute(key, value, replaceExisting: false);
        }

        public void MergeAttribute(string key, string value, bool replaceExisting)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(key));
            }

            if (replaceExisting || !Attributes.ContainsKey(key))
            {
                Attributes[key] = value;
            }
        }

        public void MergeAttributes<TKey, TValue>(IDictionary<TKey, TValue> attributes)
        {
            MergeAttributes(attributes, replaceExisting: false);
        }

        public void MergeAttributes<TKey, TValue>(IDictionary<TKey, TValue> attributes, bool replaceExisting)
        {
            // Perf: Avoid allocating enumerator for `attributes` if possible
            if (attributes != null && attributes.Count > 0)
            {
                foreach (var entry in attributes)
                {
                    var key = Convert.ToString(entry.Key, CultureInfo.InvariantCulture);
                    var value = Convert.ToString(entry.Value, CultureInfo.InvariantCulture);
                    MergeAttribute(key, value, replaceExisting);
                }
            }
        }

        /// <inheritdoc />
        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (encoder == null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }

            WriteTo(this, writer, encoder, TagRenderMode);
        }

        /// <summary>
        /// Returns an <see cref="IHtmlContent"/> that renders the body.
        /// </summary>
        /// <returns>An <see cref="IHtmlContent"/> that renders the body.</returns>
        public IHtmlContent RenderBody() => _innerHtml;

        /// <summary>
        /// Returns an <see cref="IHtmlContent"/> that renders the start tag.
        /// </summary>
        /// <returns>An <see cref="IHtmlContent"/> that renders the start tag.</returns>
        public IHtmlContent RenderStartTag() => new RenderTagHtmlContent(this, TagRenderMode.StartTag);

        /// <summary>
        /// Returns an <see cref="IHtmlContent"/> that renders the end tag.
        /// </summary>
        /// <returns>An <see cref="IHtmlContent"/> that renders the end tag.</returns>
        public IHtmlContent RenderEndTag() => new RenderTagHtmlContent(this, TagRenderMode.EndTag);

        /// <summary>
        /// Returns an <see cref="IHtmlContent"/> that renders the self-closing tag.
        /// </summary>
        /// <returns>An <see cref="IHtmlContent"/> that renders the self-closing tag.</returns>
        public IHtmlContent RenderSelfClosingTag() => new RenderTagHtmlContent(this, TagRenderMode.SelfClosing);

        private static void WriteTo(
            TagBuilder tagBuilder,
            TextWriter writer,
            HtmlEncoder encoder,
            TagRenderMode tagRenderMode)
        {
            switch (tagRenderMode)
            {
                case TagRenderMode.StartTag:
                    writer.Write("<");
                    writer.Write(tagBuilder.TagName);
                    tagBuilder.AppendAttributes(writer, encoder);
                    writer.Write(">");
                    break;
                case TagRenderMode.EndTag:
                    writer.Write("</");
                    writer.Write(tagBuilder.TagName);
                    writer.Write(">");
                    break;
                case TagRenderMode.SelfClosing:
                    writer.Write("<");
                    writer.Write(tagBuilder.TagName);
                    tagBuilder.AppendAttributes(writer, encoder);
                    writer.Write(" />");
                    break;
                default:
                    writer.Write("<");
                    writer.Write(tagBuilder.TagName);
                    tagBuilder.AppendAttributes(writer, encoder);
                    writer.Write(">");
                    if (tagBuilder._innerHtml != null)
                    {
                        tagBuilder._innerHtml.WriteTo(writer, encoder);
                    }
                    writer.Write("</");
                    writer.Write(tagBuilder.TagName);
                    writer.Write(">");
                    break;
            }
        }

        private string DebuggerToString()
        {
            using (var writer = new StringWriter())
            {
                WriteTo(writer, HtmlEncoder.Default);
                return writer.ToString();
            }
        }

        private class RenderTagHtmlContent : IHtmlContent
        {
            private readonly TagBuilder _tagBuilder;
            private readonly TagRenderMode _tagRenderMode;

            public RenderTagHtmlContent(TagBuilder tagBuilder, TagRenderMode tagRenderMode)
            {
                _tagBuilder = tagBuilder;
                _tagRenderMode = tagRenderMode;
            }

            public void WriteTo(TextWriter writer, HtmlEncoder encoder)
            {
                TagBuilder.WriteTo(_tagBuilder, writer, encoder, _tagRenderMode);
            }
        }

        private static class Html401IdUtil
        {
            public static bool IsAsciiLetter(char testChar)
            {
                return (('A' <= testChar && testChar <= 'Z') || ('a' <= testChar && testChar <= 'z'));
            }

            public static bool IsValidIdCharacter(char testChar)
            {
                return (IsAsciiLetter(testChar) || IsAsciiDigit(testChar) || IsAllowableSpecialCharacter(testChar));
            }

            private static bool IsAsciiDigit(char testChar)
            {
                return ('0' <= testChar && testChar <= '9');
            }

            private static bool IsAllowableSpecialCharacter(char testChar)
            {
                switch (testChar)
                {
                    case '-':
                    case '_':
                    case ':':
                        // Note '.' is valid according to the HTML 4.01 specification. Disallowed here to avoid
                        // confusion with CSS class selectors or when using jQuery.
                        return true;

                    default:
                        return false;
                }
            }
        }
    }
}
