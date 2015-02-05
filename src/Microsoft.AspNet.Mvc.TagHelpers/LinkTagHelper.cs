// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.Framework.FileSystemGlobbing;
using Microsoft.Framework.FileSystemGlobbing.Abstractions;
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
        private const string FallbackTestMetaTemplate = "<meta name=\"x-stylesheet-fallback-test\" class=\"{0}\" />";
        private const string FallbackJavaScriptResourceName = "compiler/resources/LinkTagHelper_FallbackJavaScript.js";

        private static readonly IEnumerable<Tuple<Mode, string[]>> ModeAttributeSets =
            new List<Tuple<Mode, string[]>>
            {
                Tuple.Create(Mode.Fallback, new[]
                {
                    FallbackHrefAttributeName,
                    FallbackTestClassAttributeName,
                    FallbackTestPropertyAttributeName,
                    FallbackTestValueAttributeName
                }),
                Tuple.Create(Mode.Fallback, new[]
                {
                    FallbackHrefIncludeAttributeName,
                    FallbackTestClassAttributeName,
                    FallbackTestPropertyAttributeName,
                    FallbackTestValueAttributeName
                }),
                Tuple.Create(Mode.Fallback, new[]
                {
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

        // Protected to ensure subclasses are correctly activated. Internal for ease of use when testing.
        [Activate]
        protected internal ILogger<LinkTagHelper> Logger { get; set; }

        // Protected to ensure subclasses are correctly activated. Internal for ease of use when testing.
        [Activate]
        protected internal IHostingEnvironment HostingEnvironment { get; set; }

        // Protected to ensure subclasses are correctly activated. Internal for ease of use when testing.
        [Activate]
        protected internal ViewContext ViewContext { get; set; }

        private DirectoryInfoBase _webRoot;

        /// <inheritdoc />
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            _webRoot = new DirectoryInfoWrapper(new DirectoryInfo(HostingEnvironment.WebRoot));

            var modeResult = context.DetermineMode(ModeAttributeSets, Logger);

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

            if (modeResult.Mode == Mode.Fallback && string.IsNullOrEmpty(HrefInclude))
            {
                // No globbing to do, just build a <link /> tag to match the original one in the source file
                content.Append("<link ");
                foreach (var attribute in output.Attributes)
                {
                    content.AppendFormat(CultureInfo.InvariantCulture, "{0}=\"{1}\" ", attribute.Key, attribute.Value);
                }
                content.Append("/>");
            }
            else
            {
                // Process href globbing include/excludes
                
                string plainHref;
                output.Attributes.TryGetValue("href", out plainHref);

                var hrefs = BuildHrefList(plainHref, HrefInclude, HrefExclude);

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
                    content.Append("/>");
                }
            }

            if (modeResult.Mode == Mode.Fallback)
            {
                content.AppendLine();

                // Build the <meta /> tag that's used to test for the presence of the stylesheet
                content.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    FallbackTestMetaTemplate,
                    FallbackTestClass));

                var fallbackHrefs = BuildHrefList(FallbackHref, FallbackHrefInclude, FallbackHrefExclude);

                // Build the <script /> tag that checks the effective style of <meta /> tag above and renders the extra
                // <link /> tag to load the fallback stylesheet if the test CSS property value is found to be false,
                // indicating that the primary stylesheet failed to load.
                content.Append("<script>");
                content.AppendFormat(CultureInfo.InvariantCulture,
                                     JavaScriptUtility.GetEmbeddedJavaScript(FallbackJavaScriptResourceName),
                                     JavaScriptUtility.JavaScriptStringEncode(FallbackTestProperty),
                                     JavaScriptUtility.JavaScriptStringEncode(FallbackTestValue),
                                     JavaScriptUtility.JavaScriptArrayEncode(fallbackHrefs));
                content.Append("</script>");
            }

            output.Content = content.ToString();
        }

        private IEnumerable<string> BuildHrefList(string href, string includePattern, string excludePattern)
        {
            var hrefs = new HashSet<string>(StringComparer.Ordinal);

            // Add the standard fallback href if present
            if (!string.IsNullOrWhiteSpace(href))
            {
                hrefs.Add(href);
            }

            // Add fallback hrefs that match the globbing patterns specified
            var matchedFallbackHrefs = ExpandGlobbedHref(includePattern, excludePattern);
            foreach (var matchedHref in matchedFallbackHrefs)
            {
                hrefs.Add(matchedHref);
            }

            return hrefs;
        }

        private IEnumerable<string> ExpandGlobbedHref(string include, string exclude = null)
        {
            if (string.IsNullOrEmpty(include))
            {
                return Enumerable.Empty<string>();
            }

            var includePatterns = include.Split(',');

            if (includePatterns.Length == 0)
            {
                return Enumerable.Empty<string>();
            }

            var matcher = new Matcher();

            if (includePatterns.Length == 1 && !matcher.IsGlobbingPattern(includePatterns[0]))
            {
                // This isn't a set of globbing patterns so just return the original include string
                return new[] { include };
            }

            var excludePatterns = exclude?.Split(',');

            matcher.AddPatterns(includePatterns, excludePatterns);
            var matches = matcher.Execute(_webRoot);
            
            return matches.Files.Select(path => ResolveMatchedPath(path, ViewContext.HttpContext.Request.PathBase));
        }

        private static string ResolveMatchedPath(string matchedPath, PathString basePath)
        {
            // TODO: This needs to resolve each to webRoot based on the request path.
            //       See example at https://github.com/aspnet/Mvc/blob/614dbccaf8d32fd6e0fbebd0b88e0831fb3e1313/src/Microsoft.AspNet.Mvc.TagHelpers/ScriptTagHelper%20.cs#L66

            var relativePath = new PathString("/" + matchedPath);
            return basePath.Add(relativePath).ToString();
        }
    }
}