// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
    /// <summary>
    /// <see cref="ITagHelper"/> implementation targeting &lt;link&gt; elements that supports fallback href paths.
    /// </summary>
    public class LinkTagHelper : TagHelper
    {
        private const string FallbackHrefAttributeName = "asp-fallback-href";
        private const string FallbackTestClassAttributeName = "asp-fallback-test-class";
        private const string FallbackTestPropertyAttributeName = "asp-fallback-test-property";
        private const string FallbackTestValueAttributeName = "asp-fallback-test-value";
        private const string FallbackTestMetaTemplate = "<meta name=\"x-stylesheet-fallback-test\" class=\"{0}\" />";
        private const string FallbackJavaScriptResourceName = "compiler/resources/LinkTagHelper_FallbackJavaScript.js";

        private static readonly Assembly ResourcesAssembly = typeof(LinkTagHelper).GetTypeInfo().Assembly;

        private static Lazy<string> FallbackJavaScriptTemplate = new Lazy<string>(LoadJavaScriptTemplate);

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

        /// <inheritdoc />
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!ShouldProcess(context))
            {
                return;
            }

            var postContent = new StringBuilder();

            // Build the <meta /> tag that's used to test for the presence of the stylesheet
            postContent.AppendLine();
            postContent.AppendLine(string.Format(CultureInfo.InvariantCulture, FallbackTestMetaTemplate,
                WebUtility.HtmlEncode(FallbackTestClass)));

            // Build the <script /> tag that checks the effective style of <meta /> tag above and renders the extra
            // <link /> tag to load the fallback stylesheet if the test CSS property value is found to be false,
            // indicating that the primary stylesheet failed to load.
            // TODO: Encode values as JS strings
            postContent.AppendLine("<script>");
            postContent.AppendFormat(CultureInfo.InvariantCulture,
                                     FallbackJavaScriptTemplate.Value,
                                     FallbackTestProperty,
                                     FallbackTestValue,
                                     FallbackHref);
            postContent.AppendLine();
            postContent.AppendLine("</script>");

            output.PostContent = postContent.ToString();
        }

        private bool ShouldProcess(TagHelperContext context)
        {
            // TODO: Check for all attribute values & log warning if invalid combination found
            return context.AllAttributes.ContainsKey(FallbackHrefAttributeName)
                   && context.AllAttributes[FallbackHrefAttributeName] != null
                   && !string.IsNullOrWhiteSpace(context.AllAttributes[FallbackHrefAttributeName].ToString());
        }

        private static string LoadJavaScriptTemplate()
        {
            // Load the fallback JavaScript template from embedded resource
            using (var resourceStream = ResourcesAssembly.GetManifestResourceStream(FallbackJavaScriptResourceName))
            {
                using (var streamReader = new StreamReader(resourceStream))
                {
                    return streamReader.ReadToEndAsync().Result
                        .Replace("{", "{{")
                        .Replace("}", "}}")
                        .Replace("[[[", "{")
                        .Replace("]]]", "}");
                }
            }
        }
    }
}