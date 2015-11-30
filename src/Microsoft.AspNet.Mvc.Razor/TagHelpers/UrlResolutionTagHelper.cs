// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text.Encodings.Web;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.TagHelpers;

namespace Microsoft.AspNet.Mvc.Razor.TagHelpers
{
    /// <summary>
    /// <see cref="ITagHelper"/> implementation targeting elements containing attributes with URL expected values.
    /// </summary>
    /// <remarks>Resolves URLs starting with '~/' (relative to the application's 'webroot' setting) that are not
    /// targeted by other <see cref="ITagHelper"/>s. Runs prior to other <see cref="ITagHelper"/>s to ensure
    /// application-relative URLs are resolved.</remarks>
    [HtmlTargetElement("*", Attributes = "itemid")]
    [HtmlTargetElement("a", Attributes = "href")]
    [HtmlTargetElement("applet", Attributes = "archive")]
    [HtmlTargetElement("area", Attributes = "href", TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("audio", Attributes = "src")]
    [HtmlTargetElement("base", Attributes = "href", TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("blockquote", Attributes = "cite")]
    [HtmlTargetElement("button", Attributes = "formaction")]
    [HtmlTargetElement("del", Attributes = "cite")]
    [HtmlTargetElement("embed", Attributes = "src", TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("form", Attributes = "action")]
    [HtmlTargetElement("html", Attributes = "manifest")]
    [HtmlTargetElement("iframe", Attributes = "src")]
    [HtmlTargetElement("img", Attributes = "src", TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("img", Attributes = "srcset", TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("input", Attributes = "src", TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("input", Attributes = "formaction", TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("ins", Attributes = "cite")]
    [HtmlTargetElement("link", Attributes = "href", TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("menuitem", Attributes = "icon")]
    [HtmlTargetElement("object", Attributes = "archive")]
    [HtmlTargetElement("object", Attributes = "data")]
    [HtmlTargetElement("q", Attributes = "cite")]
    [HtmlTargetElement("script", Attributes = "src")]
    [HtmlTargetElement("source", Attributes = "src", TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("source", Attributes = "srcset", TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("track", Attributes = "src", TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("video", Attributes = "src")]
    [HtmlTargetElement("video", Attributes = "poster")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class UrlResolutionTagHelper : TagHelper
    {
        // Valid whitespace characters defined by the HTML5 spec.
        private static readonly char[] ValidAttributeWhitespaceChars =
            new[] { '\t', '\n', '\u000C', '\r', ' ' };
        private static readonly IReadOnlyDictionary<string, IEnumerable<string>> ElementAttributeLookups =
            new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "a", new[] { "href" } },
                { "applet", new[] { "archive" } },
                { "area", new[] { "href" } },
                { "audio", new[] { "src" } },
                { "base", new[] { "href" } },
                { "blockquote", new[] { "cite" } },
                { "button", new[] { "formaction" } },
                { "del", new[] { "cite" } },
                { "embed", new[] { "src" } },
                { "form", new[] { "action" } },
                { "html", new[] { "manifest" } },
                { "iframe", new[] { "src" } },
                { "img", new[] { "src", "srcset" } },
                { "input", new[] { "src", "formaction" } },
                { "ins", new[] { "cite" } },
                { "link", new[] { "href" } },
                { "menuitem", new[] { "icon" } },
                { "object", new[] { "archive", "data" } },
                { "q", new[] { "cite" } },
                { "script", new[] { "src" } },
                { "source", new[] { "src", "srcset" } },
                { "track", new[] { "src" } },
                { "video", new[] { "poster", "src" } },
            };

        private static readonly ISet<string> MultipleUrlAttributes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "srcset"
        };

        /// <summary>
        /// Creates a new <see cref="UrlResolutionTagHelper"/>.
        /// </summary>
        /// <param name="urlHelper">The <see cref="IUrlHelper"/>.</param>
        /// <param name="htmlEncoder">The <see cref="HtmlEncoder"/>.</param>
        public UrlResolutionTagHelper(IUrlHelper urlHelper, HtmlEncoder htmlEncoder)
        {
            UrlHelper = urlHelper;
            HtmlEncoder = htmlEncoder;
        }

        /// <inheritdoc />
        public override int Order
        {
            get
            {
                return -1000 - 999;
            }
        }

        protected IUrlHelper UrlHelper { get; }

        protected HtmlEncoder HtmlEncoder { get; }

        /// <inheritdoc />
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            IEnumerable<string> attributeNames;
            if (ElementAttributeLookups.TryGetValue(output.TagName, out attributeNames))
            {
                foreach (var attributeName in attributeNames)
                {
                    ProcessUrlAttribute(attributeName, output);
                }
            }

            // itemid can be present on any HTML element.
            ProcessUrlAttribute("itemid", output);
        }

        /// <summary>
        /// Resolves and updates URL values starting with '~/' (relative to the application's 'webroot' setting) for
        /// <paramref name="output"/>'s <see cref="TagHelperOutput.Attributes"/> whose
        /// <see cref="TagHelperAttribute.Name"/> is <paramref name="attributeName"/>.
        /// </summary>
        /// <param name="attributeName">The attribute name used to lookup values to resolve.</param>
        /// <param name="output">The <see cref="TagHelperOutput"/>.</param>
        protected void ProcessUrlAttribute(string attributeName, TagHelperOutput output)
        {
            if (attributeName == null)
            {
                throw new ArgumentNullException(nameof(attributeName));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            IEnumerable<TagHelperAttribute> attributes;

            if (output.Attributes.TryGetAttributes(attributeName, out attributes))
            {
                var attributeCanAcceptMultipleUrls = MultipleUrlAttributes.Contains(attributeName);

                foreach (var attribute in attributes)
                {
                    if (attribute.Value is string)
                    {
                        var stringValue = attribute.Value as string;
                        attribute.Value = attributeCanAcceptMultipleUrls ? ResolveMultipleUrls(stringValue) : ResolveUrl(stringValue);
                    }
                    else if (attribute.Value is HtmlString)
                    {
                        var htmlStringValue = attribute.Value as HtmlString;
                        attribute.Value = attributeCanAcceptMultipleUrls ? ResolveMultipleUrls(htmlStringValue) : ResolveUrl(htmlStringValue);
                    }
                    
                }
            }
        }

        private string ResolveMultipleUrls(string stringOfUrls, bool encodeWebRoot = false)
        {
            var resolvedUrls = new List<string>();
            foreach (var item in stringOfUrls.Split(','))
            {
                resolvedUrls.Add(ResolveUrl(item, encodeWebRoot));
            }
            return string.Join(", ", resolvedUrls);
        }

        private HtmlString ResolveMultipleUrls(HtmlString stringOfUrls)
        {
            return new HtmlString(ResolveMultipleUrls(stringOfUrls.ToString(), encodeWebRoot: true));
        }

        private string ResolveUrl(string stringValue, bool encodeWebRoot = false)
        {
            string resolvedUrl;
            if (TryResolveUrl(stringValue, encodeWebRoot: encodeWebRoot, resolvedUrl: out resolvedUrl))
            {
                return resolvedUrl;
            }

            return stringValue;
        }

        private HtmlString ResolveUrl(HtmlString htmlStringValue)
        {
            string resolvedUrl;
            if (TryResolveUrl(htmlStringValue.ToString(), encodeWebRoot: true, resolvedUrl: out resolvedUrl))
            {
                return new HtmlString(resolvedUrl);
            }
            return htmlStringValue;

        }

        /// <summary>
        /// Tries to resolve the given <paramref name="url"/> value relative to the application's 'webroot' setting.
        /// </summary>
        /// <param name="url">The URL to resolve.</param>
        /// <param name="encodeWebRoot">If <c>true</c>, will HTML encode the expansion of '~/'.</param>
        /// <param name="resolvedUrl">Absolute URL beginning with the application's virtual root. <c>null</c> if
        /// <paramref name="url"/> could not be resolved.</param>
        /// <returns><c>true</c> if the <paramref name="url"/> could be resolved; <c>false</c> otherwise.</returns>
        protected bool TryResolveUrl(string url, bool encodeWebRoot, out string resolvedUrl)
        {
            resolvedUrl = null;

            if (url == null)
            {
                return false;
            }

            var trimmedUrl = url.Trim(ValidAttributeWhitespaceChars);

            // Before doing more work, ensure that the URL we're looking at is app relative.
            if (trimmedUrl.Length >= 2 && trimmedUrl[0] == '~' && trimmedUrl[1] == '/')
            {
                var appRelativeUrl = UrlHelper.Content(trimmedUrl);

                if (encodeWebRoot)
                {
                    var postTildeSlashUrlValue = trimmedUrl.Substring(2);

                    if (!appRelativeUrl.EndsWith(postTildeSlashUrlValue, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException(
                            Resources.FormatCouldNotResolveApplicationRelativeUrl_TagHelper(
                                url,
                                nameof(IUrlHelper),
                                nameof(IUrlHelper.Content),
                                "removeTagHelper",
                                typeof(UrlResolutionTagHelper).FullName,
                                typeof(UrlResolutionTagHelper).GetTypeInfo().Assembly.GetName().Name));
                    }

                    var applicationPath = appRelativeUrl.Substring(0, appRelativeUrl.Length - postTildeSlashUrlValue.Length);
                    var encodedApplicationPath = HtmlEncoder.Encode(applicationPath);

                    resolvedUrl = string.Concat(encodedApplicationPath, postTildeSlashUrlValue);
                }
                else
                {
                    resolvedUrl = appRelativeUrl;
                }

                return true;
            }

            return false;
        }
    }
}
