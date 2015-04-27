﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc.TagHelpers.Internal;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.Framework.Caching.Memory;
using Microsoft.Framework.Logging;
using Microsoft.Framework.WebEncoders;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
    /// <summary>
    /// <see cref="ITagHelper"/> implementation targeting &lt;script&gt; elements that supports fallback src paths.
    /// </summary>
    /// <remarks>
    /// The tag helper won't process for cases with just the 'src' attribute.
    /// </remarks>
    [TargetElement("script", Attributes = SrcIncludeAttributeName)]
    [TargetElement("script", Attributes = SrcExcludeAttributeName)]
    [TargetElement("script", Attributes = FallbackSrcAttributeName)]
    [TargetElement("script", Attributes = FallbackSrcIncludeAttributeName)]
    [TargetElement("script", Attributes = FallbackSrcExcludeAttributeName)]
    [TargetElement("script", Attributes = FallbackTestExpressionAttributeName)]
    [TargetElement("script", Attributes = FileVersionAttributeName)]
    public class ScriptTagHelper : TagHelper
    {
        private const string SrcIncludeAttributeName = "asp-src-include";
        private const string SrcExcludeAttributeName = "asp-src-exclude";
        private const string FallbackSrcAttributeName = "asp-fallback-src";
        private const string FallbackSrcIncludeAttributeName = "asp-fallback-src-include";
        private const string FallbackSrcExcludeAttributeName = "asp-fallback-src-exclude";
        private const string FallbackTestExpressionAttributeName = "asp-fallback-test";
        private const string SrcAttributeName = "src";
        private const string FileVersionAttributeName = "asp-file-version";

        private FileVersionProvider _fileVersionProvider;

        private static readonly ModeAttributes<Mode>[] ModeDetails = new[] {
            // Regular src with file version alone
            ModeAttributes.Create(Mode.FileVersion, new[] { FileVersionAttributeName }),
            // Globbed src (include only)
            ModeAttributes.Create(Mode.GlobbedSrc, new [] { SrcIncludeAttributeName }),
            // Globbed src (include & exclude)
            ModeAttributes.Create(Mode.GlobbedSrc, new [] { SrcIncludeAttributeName, SrcExcludeAttributeName }),
            // Fallback with static src
            ModeAttributes.Create(
                Mode.Fallback, new[]
                {
                    FallbackSrcAttributeName,
                    FallbackTestExpressionAttributeName
                }),
            // Fallback with globbed src (include only)
            ModeAttributes.Create(
                Mode.Fallback, new[] {
                    FallbackSrcIncludeAttributeName,
                    FallbackTestExpressionAttributeName
                }),
            // Fallback with globbed src (include & exclude)
            ModeAttributes.Create(
                Mode.Fallback, new[] {
                    FallbackSrcIncludeAttributeName,
                    FallbackSrcExcludeAttributeName,
                    FallbackTestExpressionAttributeName
                }),
        };

        private enum Mode
        {
            /// <summary>
            /// Just adding a file version for the generated urls.
            /// </summary>
            FileVersion = 0,
            /// <summary>
            /// Just performing file globbing search for the src, rendering a separate &lt;script&gt; for each match.
            /// </summary>
            GlobbedSrc = 1,
            /// <summary>
            /// Rendering a fallback block if primary javascript fails to load. Will also do globbing for both the
            /// primary and fallback srcs if the appropriate properties are set.
            /// </summary>
            Fallback = 2
        }

        /// <summary>
        /// Address of the external script to use.
        /// </summary>
        /// <remarks>
        /// Passed through to the generated HTML in all cases.
        /// </remarks>
        [HtmlAttributeName(SrcAttributeName)]
        public string Src { get; set; }

        /// <summary>
        /// A comma separated list of globbed file patterns of JavaScript scripts to load.
        /// The glob patterns are assessed relative to the application's 'webroot' setting.
        /// </summary>
        [HtmlAttributeName(SrcIncludeAttributeName)]
        public string SrcInclude { get; set; }

        /// <summary>
        /// A comma separated list of globbed file patterns of JavaScript scripts to exclude from loading.
        /// The glob patterns are assessed relative to the application's 'webroot' setting.
        /// Must be used in conjunction with <see cref="SrcInclude"/>.
        /// </summary>
        [HtmlAttributeName(SrcExcludeAttributeName)]
        public string SrcExclude { get; set; }

        /// <summary>
        /// The URL of a Script tag to fallback to in the case the primary one fails.
        /// </summary>
        [HtmlAttributeName(FallbackSrcAttributeName)]
        public string FallbackSrc { get; set; }

        /// <summary>
        /// Value indicating if file version should be appended to src urls.
        /// </summary>
        /// <remarks>
        /// A query string "v" with the encoded content of the file is added.
        /// </remarks>
        [HtmlAttributeName(FileVersionAttributeName)]
        public bool? FileVersion { get; set; }

        /// <summary>
        /// A comma separated list of globbed file patterns of JavaScript scripts to fallback to in the case the
        /// primary one fails.
        /// The glob patterns are assessed relative to the application's 'webroot' setting.
        /// </summary>
        [HtmlAttributeName(FallbackSrcIncludeAttributeName)]
        public string FallbackSrcInclude { get; set; }

        /// <summary>
        /// A comma separated list of globbed file patterns of JavaScript scripts to exclude from the fallback list, in
        /// the case the primary one fails.
        /// The glob patterns are assessed relative to the application's 'webroot' setting.
        /// Must be used in conjunction with <see cref="FallbackSrcInclude"/>.
        /// </summary>
        [HtmlAttributeName(FallbackSrcExcludeAttributeName)]
        public string FallbackSrcExclude { get; set; }

        /// <summary>
        /// The script method defined in the primary script to use for the fallback test.
        /// </summary>
        [HtmlAttributeName(FallbackTestExpressionAttributeName)]
        public string FallbackTestExpression { get; set; }

        [Activate, HtmlAttributeNotBound]
        public ILoggerFactory LoggerFactory { get; set; }

        // TODO: will remove LoggerFactory and activate logger once DI/hosting bug is fixed
        [HtmlAttributeNotBound]
        public ILogger<ScriptTagHelper> Logger { get; set; }

        [Activate, HtmlAttributeNotBound]
        public IHostingEnvironment HostingEnvironment { get; set; }

        [Activate, HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        [Activate, HtmlAttributeNotBound]
        public IMemoryCache Cache { get; set; }

        [Activate, HtmlAttributeNotBound]
        public IHtmlEncoder HtmlEncoder { get; set; }

        [Activate, HtmlAttributeNotBound]
        public IJavaScriptStringEncoder JavaScriptEncoder { get; set; }

        // Internal for ease of use when testing.
        protected internal GlobbingUrlBuilder GlobbingUrlBuilder { get; set; }

        /// <inheritdoc />
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            // Pass through attribute that is also a well-known HTML attribute.
            if (Src != null)
            {
                output.CopyHtmlAttribute(SrcAttributeName, context);
            }

            var modeResult = AttributeMatcher.DetermineMode(context, ModeDetails);

            var logger = Logger ?? LoggerFactory.CreateLogger<ScriptTagHelper>();

            modeResult.LogDetails(logger, this, context.UniqueId, ViewContext.View.Path);

            if (!modeResult.FullMatches.Any())
            {
                // No attributes matched so we have nothing to do
                return;
            }

            // Get the highest matched mode
            var mode = modeResult.FullMatches.Select(match => match.Mode).Max();

            // NOTE: Values in TagHelperOutput.Attributes may already be HTML-encoded.
            var attributes = new Dictionary<string, object>(output.Attributes);

            var builder = new DefaultTagHelperContent();
            var originalContent = await context.GetChildContentAsync();

            if (mode == Mode.Fallback && string.IsNullOrEmpty(SrcInclude) || mode == Mode.FileVersion)
            {
                // No globbing to do, just build a <script /> tag to match the original one in the source file
                // Or just add file version to the script tag.
                BuildScriptTag(originalContent, attributes, builder);
            }
            else
            {
                BuildGlobbedScriptTags(originalContent, attributes, builder);
            }

            if (mode == Mode.Fallback)
            {
                BuildFallbackBlock(attributes, builder);
            }

            // We've taken over tag rendering, so prevent rendering the outer tag
            output.TagName = null;
            output.Content.SetContent(builder);
        }

        private void BuildGlobbedScriptTags(
            TagHelperContent originalContent,
            IDictionary<string, object> attributes,
            TagHelperContent builder)
        {
            EnsureGlobbingUrlBuilder();

            // Build a <script> tag for each matched src as well as the original one in the source file
            var urls = GlobbingUrlBuilder.BuildUrlList(Src, SrcInclude, SrcExclude);
            foreach (var url in urls)
            {
                // "url" values come from bound attributes and globbing. Must always be non-null.
                Debug.Assert(url != null);

                var content = originalContent;
                if (!string.Equals(url, Src, StringComparison.OrdinalIgnoreCase))
                {
                    // Do not copy content into added <script/> elements.
                    content = null;
                }

                attributes[SrcAttributeName] = url;
                BuildScriptTag(content, attributes, builder);
            }
        }

        private void BuildFallbackBlock(IDictionary<string, object> attributes, DefaultTagHelperContent builder)
        {
            EnsureGlobbingUrlBuilder();
            EnsureFileVersionProvider();

            var fallbackSrcs = GlobbingUrlBuilder.BuildUrlList(FallbackSrc, FallbackSrcInclude, FallbackSrcExclude);
            if (fallbackSrcs.Any())
            {
                // Build the <script> tag that checks the test method and if it fails, renders the extra script.
                builder.Append(Environment.NewLine)
                       .Append("<script>(")
                       .Append(FallbackTestExpression)
                       .Append("||document.write(\"");

                // May have no "src" attribute in the dictionary e.g. if Src and SrcInclude were not bound.
                if (!attributes.ContainsKey(SrcAttributeName))
                {
                    // Need this entry to place each fallback source.
                    attributes.Add(SrcAttributeName, null);
                }

                foreach (var src in fallbackSrcs)
                {
                    // Fallback "src" values come from bound attributes and globbing. Must always be non-null.
                    Debug.Assert(src != null);

                    builder.Append("<script");

                    foreach (var attribute in attributes)
                    {
                        if (!attribute.Key.Equals(SrcAttributeName, StringComparison.OrdinalIgnoreCase))
                        {
                            var encodedKey = JavaScriptEncoder.JavaScriptStringEncode(attribute.Key);
                            var attributeValue = attribute.Value.ToString();
                            var encodedValue = JavaScriptEncoder.JavaScriptStringEncode(attributeValue);

                            AppendAttribute(builder, encodedKey, encodedValue, escapeQuotes: true);
                        }
                        else
                        {
                            // Ignore attribute.Value; use src instead.
                            var attributeValue = src;
                            if (FileVersion == true)
                            {
                                attributeValue = _fileVersionProvider.AddFileVersionToPath(attributeValue);
                            }

                            // attribute.Key ("src") does not need to be JavaScript-encoded.
                            var encodedValue = JavaScriptEncoder.JavaScriptStringEncode(attributeValue);

                            AppendAttribute(builder, attribute.Key, encodedValue, escapeQuotes: true);
                        }
                    }

                    builder.Append("><\\/script>");
                }

                builder.Append("\"));</script>");
            }
        }

        private void EnsureGlobbingUrlBuilder()
        {
            if (GlobbingUrlBuilder == null)
            {
                GlobbingUrlBuilder = new GlobbingUrlBuilder(
                    HostingEnvironment.WebRootFileProvider,
                    Cache,
                    ViewContext.HttpContext.Request.PathBase);
            }
        }

        private void EnsureFileVersionProvider()
        {
            if (_fileVersionProvider == null)
            {
                _fileVersionProvider = new FileVersionProvider(
                    HostingEnvironment.WebRootFileProvider,
                    Cache,
                    ViewContext.HttpContext.Request.PathBase);
            }
        }

        private void BuildScriptTag(
            TagHelperContent content,
            IDictionary<string, object> attributes,
            TagHelperContent builder)
        {
            EnsureFileVersionProvider();
            builder.Append("<script");

            foreach (var attribute in attributes)
            {
                var attributeValue = attribute.Value;
                if (FileVersion == true &&
                    string.Equals(attribute.Key, SrcAttributeName, StringComparison.OrdinalIgnoreCase))
                {
                    // "src" values come from bound attributes and globbing. So anything but a non-null string is
                    // unexpected but could happen if another helper targeting the same element does something odd.
                    // Pass through existing value in that case.
                    var attributeStringValue = attributeValue as string;
                    if (attributeStringValue != null)
                    {
                        attributeValue = _fileVersionProvider.AddFileVersionToPath(attributeStringValue);
                    }
                }

                AppendAttribute(builder, attribute.Key, attributeValue, escapeQuotes: false);
            }

            builder.Append(">")
                   .Append(content)
                   .Append("</script>");
        }

        private void AppendAttribute(TagHelperContent content, string key, object value, bool escapeQuotes)
        {
            content
                .Append(" ")
                .Append(key);
            if (escapeQuotes)
            {
                // Passed only JavaScript-encoded strings in this case. Do not perform HTML-encoding as well.
                content
                    .Append("=\\\"")
                    .Append((string)value)
                    .Append("\\\"");
            }
            else
            {
                // HTML-encoded the given value if necessary.
                content
                    .Append("=\"")
                    .Append(HtmlEncoder, ViewContext.Writer.Encoding, value)
                    .Append("\"");
            }
        }
    }
}
