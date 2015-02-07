// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc.TagHelpers.Internal;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.Framework.Cache.Memory;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
    /// <summary>
    /// <see cref="ITagHelper"/> implementation targeting &lt;link&gt; elements that supports fallback href paths.
    /// </summary>
    public class LinkTagHelper : TagHelper
    {
        private const string HrefIncludeAttributeName = "asp-href-include";
        private const string HrefExcludeAttributeName = "asp-href-exclude";
        private const string FallbackHrefAttributeName = "asp-fallback-href";
        private const string FallbackHrefIncludeAttributeName = "asp-fallback-href-include";
        private const string FallbackHrefExcludeAttributeName = "asp-fallback-href-exclude";
        private const string FallbackTestClassAttributeName = "asp-fallback-test-class";
        private const string FallbackTestPropertyAttributeName = "asp-fallback-test-property";
        private const string FallbackTestValueAttributeName = "asp-fallback-test-value";
        private const string FallbackJavaScriptResourceName = "compiler/resources/LinkTagHelper_FallbackJavaScript.js";

        private static readonly Tuple<Mode, string[]>[] ModeInfo =
            new []
            {
                Tuple.Create(Mode.Fallback, new[]
                {
                    // Static fallback only
                    FallbackHrefAttributeName,
                    FallbackTestClassAttributeName,
                    FallbackTestPropertyAttributeName,
                    FallbackTestValueAttributeName
                }),
                Tuple.Create(Mode.Fallback, new[]
                {
                    // Globbed fallback, include pattern only (static fallback optional)
                    FallbackHrefIncludeAttributeName,
                    FallbackTestClassAttributeName,
                    FallbackTestPropertyAttributeName,
                    FallbackTestValueAttributeName
                }),
                Tuple.Create(Mode.Fallback, new[]
                {
                    // Globbed fallback, include & exclude patterns (static fallback optional)
                    FallbackHrefIncludeAttributeName,
                    FallbackHrefExcludeAttributeName,
                    FallbackTestClassAttributeName,
                    FallbackTestPropertyAttributeName,
                    FallbackTestValueAttributeName
                }),
                Tuple.Create(Mode.GlobbedHref, new [] { HrefIncludeAttributeName }),
                Tuple.Create(Mode.GlobbedHref, new [] { HrefIncludeAttributeName, HrefExcludeAttributeName })
            };

        private enum Mode
        {
            Fallback,
            GlobbedHref
        }

        /// <summary>
        /// A comma separated list of globbed file patterns of CSS stylesheets to load.
        /// The glob patterns are assessed relevant to the application's 'webroot' setting.
        /// </summary>
        [HtmlAttributeName(HrefIncludeAttributeName)]
        public string HrefInclude { get; set; }

        /// <summary>
        /// A comma separated list of globbed file patterns of CSS stylesheets to exclude from loading.
        /// The glob patterns are assessed relevant to the application's 'webroot' setting.
        /// Must be used in conjunction with <see cref="HrefInclude"/>.
        /// </summary>
        [HtmlAttributeName(HrefExcludeAttributeName)]
        public string HrefExclude { get; set; }

        /// The URL of a CSS stylesheet to fallback to in the case the primary one fails (as specified in the href
        /// attribute).
        /// </summary>
        [HtmlAttributeName(FallbackHrefAttributeName)]
        public string FallbackHref { get; set; }

        /// <summary>
        /// A comma separated list of globbed file patterns of CSS stylesheets to fallback to in the case the primary
        /// one fails (as specified in the href attribute).
        /// The glob patterns are assessed relevant to the application's 'webroot' setting.
        /// </summary>
        [HtmlAttributeName(FallbackHrefIncludeAttributeName)]
        public string FallbackHrefInclude { get; set; }

        /// <summary>
        /// A comma separated list of globbed file patterns of CSS stylesheets to exclude from the fallback list, in
        /// the case the primary one fails (as specified in the href attribute).
        /// The glob patterns are assessed relevant to the application's 'webroot' setting.
        /// Must be used in conjunction with <see cref="FallbackHrefInclude"/>.
        /// </summary>
        [HtmlAttributeName(FallbackHrefExcludeAttributeName)]
        public string FallbackHrefExclude { get; set; }

        /// <summary>
        /// The class name defined in the stylesheet to use for the fallback test.
        /// </summary>
        [HtmlAttributeName(FallbackTestClassAttributeName)]
        public string FallbackTestClass { get; set; }

        /// <summary>
        /// The CSS property name to use for the fallback test.
        /// </summary>
        [HtmlAttributeName(FallbackTestPropertyAttributeName)]
        public string FallbackTestProperty { get; set; }

        /// <summary>
        /// The CSS property value to use for the fallback test.
        /// </summary>
        [HtmlAttributeName(FallbackTestValueAttributeName)]
        public string FallbackTestValue { get; set; }

        // Properties are protected to ensure subclasses are correctly activated. Internal for ease of use when testing.
        [Activate]
        protected internal ILogger<LinkTagHelper> Logger { get; set; }
        
        [Activate]
        protected internal IHostingEnvironment HostingEnvironment { get; set; }

        [Activate]
        protected internal ViewContext ViewContext { get; set; }
        
        [Activate]
        protected internal IMemoryCache Cache { get; set; }

        // Internal for ease of use when testing.
        protected internal GlobbingUtility GlobbingUtil { get; set; }

        /// <inheritdoc />
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (GlobbingUtil == null)
            {
                GlobbingUtil = new GlobbingUtility(
                    Cache,
                    HostingEnvironment.WebRootFileProvider,
                    ViewContext.HttpContext.Request.PathBase);
            }
            
            var modeResult = context.DetermineMode(ModeInfo, Logger);

            if (!modeResult.Matched)
            {
                if (Logger.IsEnabled(LogLevel.Verbose))
                {
                    Logger.WriteVerbose("Skipping processing for {0} {1}", nameof(LinkTagHelper), context.UniqueId);
                }
                return;
            }

            var attributes = new Dictionary<string, string>(output.Attributes);
            
            // NOTE: Values in TagHelperOutput.Attributes are already HtmlEncoded

            var builder = new StringBuilder();
            
            if (modeResult.Mode == Mode.Fallback && string.IsNullOrEmpty(HrefInclude))
            {
                // No globbing to do, just build a <link /> tag to match the original one in the source file
                BuildLinkTag(attributes, builder);
            }
            else
            {
                BuildGlobbedLinkTags(attributes, builder);
            }

            if (modeResult.Mode == Mode.Fallback)
            {
                BuildFallbackBlock(builder);
            }

            // We've taken over rendering so prevent the element rendering the outer tag
            output.TagName = null;
            output.Content = builder.ToString();
        }

        private void BuildGlobbedLinkTags(IDictionary<string, string> attributes, StringBuilder builder)
        {
            // Build a <link /> tag for each matched href as well as the original one in the source file
            string staticHref;
            attributes.TryGetValue("href", out staticHref);

            var hrefs = GlobbingUtil.BuildUrlList(staticHref, HrefInclude, HrefExclude);

            foreach (var href in hrefs)
            {
                attributes["href"] = WebUtility.HtmlEncode(href);
                BuildLinkTag(attributes, builder);
            }
        }
        
        private void BuildFallbackBlock(StringBuilder builder)
        {
            builder.AppendLine();

            // Build the <meta /> tag that's used to test for the presence of the stylesheet
            builder.AppendFormat(CultureInfo.InvariantCulture,
                "<meta name=\"x-stylesheet-fallback-test\" class=\"{0}\" />", WebUtility.HtmlEncode(FallbackTestClass));

            var fallbackHrefs = GlobbingUtil.BuildUrlList(FallbackHref, FallbackHrefInclude, FallbackHrefExclude);

            // Build the <script /> tag that checks the effective style of <meta /> tag above and renders the extra
            // <link /> tag to load the fallback stylesheet if the test CSS property value is found to be false,
            // indicating that the primary stylesheet failed to load.
            builder.Append("<script>")
                   .AppendFormat(CultureInfo.InvariantCulture,
                        JavaScriptUtility.GetEmbeddedJavaScript(FallbackJavaScriptResourceName),
                        JavaScriptUtility.JavaScriptStringEncode(FallbackTestProperty),
                        JavaScriptUtility.JavaScriptStringEncode(FallbackTestValue),
                        JavaScriptUtility.JavaScriptArrayEncode(fallbackHrefs))
                   .Append("</script>");
        }

        private static void BuildLinkTag(IDictionary<string, string> attributes, StringBuilder builder)
        {
            builder.Append("<link ");

            foreach (var attribute in attributes)
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, "{0}=\"{1}\" ", attribute.Key, attribute.Value);
            }

            builder.Append("/>");
        }
    }
}