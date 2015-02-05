// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.Framework.FileSystemGlobbing;
using Microsoft.Framework.FileSystemGlobbing.Abstractions;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Runtime;
using System.IO;
using System.Linq;
using Microsoft.AspNet.Hosting;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
    /// <summary>
    /// <see cref="ITagHelper"/> implementation targeting &lt;link&gt; elements that supports fallback href paths.
    /// </summary>
    public class LinkTagHelper : TagHelper
    {
        private const string HrefAttributeName = "asp-href";
        private const string FallbackHrefAttributeName = "asp-fallback-href";
        private const string FallbackTestClassAttributeName = "asp-fallback-test-class";
        private const string FallbackTestPropertyAttributeName = "asp-fallback-test-property";
        private const string FallbackTestValueAttributeName = "asp-fallback-test-value";
        private const string FallbackTestMetaTemplate = "<meta name=\"x-stylesheet-fallback-test\" class=\"{0}\" />";
        private const string FallbackJavaScriptResourceName = "compiler/resources/LinkTagHelper_FallbackJavaScript.js";

        private static readonly IDictionary<Mode, IEnumerable<string>> ModeAttributeSets =
            new Dictionary<Mode, IEnumerable<string>>
            {
                { Mode.Fallback, new[]
                    {
                        FallbackHrefAttributeName,
                        FallbackTestClassAttributeName,
                        FallbackTestPropertyAttributeName,
                        FallbackTestValueAttributeName
                    }
                },
                { Mode.GlobbedHref, new [] { HrefAttributeName } }
            };

        private enum Mode
        {
            GlobbedHref,
            Fallback
        }

        /// <summary>
        /// A comma separated list of globbed file patterns of CSS stylesheets to load.
        /// Patterns starting with a "!" will be added as excludes. All other patterns will be added as includes.
        /// </summary>
        [HtmlAttributeName(HrefAttributeName)]
        public string Href { get; set; }

        /// <summary>
        /// The URL of a CSS stylesheet to fallback to in the case the primary one fails (as specified in the href
        /// attribute).
        /// </summary>
        [HtmlAttributeName(FallbackHrefAttributeName)]
        public string FallbackHref { get; set; }

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

        // Protected to ensure subclasses are correctly activated. Internal for ease of use when testing.
        [Activate]
        protected internal ILogger<LinkTagHelper> Logger { get; set; }

        // Protected to ensure subclasses are correctly activated. Internal for ease of use when testing.
        [Activate]
        protected internal IHostingEnvironment HostingEnvironment { get; set; }

        private DirectoryInfoBase _webRoot;

        /// <inheritdoc />
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            _webRoot = new DirectoryInfoWrapper(new DirectoryInfo(HostingEnvironment.WebRoot));

            var modeResult = context.DetermineMode(ModeAttributeSets);

            if (!modeResult.Matched)
            {
                if (Logger.IsEnabled(LogLevel.Verbose))
                {
                    Logger.WriteVerbose("Skipping processing for {0} {1}", nameof(LinkTagHelper), context.UniqueId);
                }
                return;
            }

            var content = new StringBuilder();

            // NOTE: Values in TagHelperOutput.Attributes are already HtmlEncoded

            // We've taken over rendering here so prevent the element rendering the outer tag
            output.TagName = null;

            if (string.IsNullOrEmpty(Href))
            {
                // Just build a <link /> tag to match the original one in the source file
                content.Append("<link ");
                foreach (var attribute in output.Attributes)
                {
                    content.AppendFormat(CultureInfo.InvariantCulture, "{0}=\"{1}\" ", attribute.Key, attribute.Value);
                }
                content.AppendLine("/>");
            }
            else
            {
                var hrefs = new HashSet<string>(StringComparer.Ordinal);

                // Add the standard href if present
                string plainHref;
                if (output.Attributes.TryGetValue("href", out plainHref))
                {
                    hrefs.Add(plainHref);
                }

                // Add hrefs that match the globbing pattern specified
                var matchedHrefs = ExpandGlobbedHref(Href);
                if (matchedHrefs.Any())
                {
                    foreach (var matchedHref in matchedHrefs)
                    {
                        hrefs.Add(matchedHref);
                    }
                }

                foreach (var href in hrefs)
                {
                    // Build a <link /> tag for each matched href
                    content.AppendFormat("<link href=\"{0}\" ", href);
                    foreach (var attribute in output.Attributes)
                    {
                        if (!string.Equals(attribute.Key, "href", StringComparison.Ordinal))
                        {
                            content.AppendFormat(CultureInfo.InvariantCulture, "{0}=\"{1}\" ",
                                attribute.Key,
                                attribute.Value);
                        }
                    }
                    content.AppendLine("/>");
                }
            }

            if (modeResult.Mode == Mode.Fallback)
            {
                // Build the <meta /> tag that's used to test for the presence of the stylesheet
                content.AppendLine(string.Format(CultureInfo.InvariantCulture, FallbackTestMetaTemplate, FallbackTestClass));

                var matchedFallbackHrefs = ExpandGlobbedHref(FallbackHref);

                // Build the <script /> tag that checks the effective style of <meta /> tag above and renders the extra
                // <link /> tag to load the fallback stylesheet if the test CSS property value is found to be false,
                // indicating that the primary stylesheet failed to load.
                content.Append("<script>");
                content.AppendFormat(CultureInfo.InvariantCulture,
                                     JavaScriptUtility.GetEmbeddedJavaScript(FallbackJavaScriptResourceName),
                                     JavaScriptUtility.JavaScriptStringEncode(FallbackTestProperty),
                                     JavaScriptUtility.JavaScriptStringEncode(FallbackTestValue),
                                     JavaScriptUtility.JavaScriptArrayEncode(matchedFallbackHrefs));
                content.Append("</script>");
            }

            output.Content = content.ToString();
        }

        private IEnumerable<string> ExpandGlobbedHref(string href)
        {
            // TODO: Should we cache these so we don't have to look it up every time?
            var patterns = href.Split(',');

            if (patterns.Length == 0)
            {
                return Enumerable.Empty<string>();
            }

            var matcher = new Matcher();

            if (patterns.Length == 1 && !matcher.IsGlobbingPattern(patterns[0]))
            {
                // This isn't a set of globbing patterns so just return the original href
                return new[] { href };
            }

            matcher.AddPatterns(patterns);
            var matches = matcher.Execute(_webRoot);

            return matches.Files;
        }
    }
}