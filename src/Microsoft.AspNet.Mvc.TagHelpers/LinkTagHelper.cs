// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.Framework.Logging;

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

        private ILogger _logger;

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

        [Activate]
        protected internal ILoggerFactory LoggerFactory { get; set; }        

        /// <inheritdoc />
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            _logger = LoggerFactory.Create<LinkTagHelper>();
            
            if (!ShouldProcess(context))
            {
                _logger.WriteVerbose("Skipping processing for {0} {1}", nameof(LinkTagHelper), context.UniqueId);
                return;
            }

            var content = new StringBuilder();

            // NOTE: Values in TagHelperOutput.Attributes are already HtmlEncoded

            // Rebuild the <link /> tag that loads the primary stylesheet
            content.Append("<link ");
            foreach (var a in output.Attributes)
            {
                content.AppendFormat(CultureInfo.InvariantCulture, "{0}=\"{1}\" ", a.Key, a.Value);
            }
            content.AppendLine("/>");

            // Build the <meta /> tag that's used to test for the presence of the stylesheet
            content.AppendLine(string.Format(CultureInfo.InvariantCulture, FallbackTestMetaTemplate,
                FallbackTestClass));

            // Build the <script /> tag that checks the effective style of <meta /> tag above and renders the extra
            // <link /> tag to load the fallback stylesheet if the test CSS property value is found to be false,
            // indicating that the primary stylesheet failed to load.
            content.Append("<script>");
            content.AppendFormat(CultureInfo.InvariantCulture,
                                 FallbackJavaScriptTemplate.Value,
                                 JavaScriptStringEncode(FallbackTestProperty),
                                 JavaScriptStringEncode(FallbackTestValue),
                                 JavaScriptStringEncode(FallbackHref));
            content.Append("</script>");

            output.TagName = null;
            output.Content = content.ToString();
        }

        private bool ShouldProcess(TagHelperContext context)
        {
            // Check for all attribute values & log warning if invalid combination found.
            // NOTE: All attributes are required for the LinkTagHelper to process.
            var attrNames = new []
            {
                FallbackHrefAttributeName,
                FallbackTestClassAttributeName,
                FallbackTestPropertyAttributeName,
                FallbackTestValueAttributeName
            };
            var presentAttrNames = new List<string>();
            var missingAttrNames = new List<string>();
            
            foreach (var attr in attrNames)
            {
                if (!context.AllAttributes.ContainsKey(attr)
                    || context.AllAttributes[attr] == null
                    || string.IsNullOrWhiteSpace(context.AllAttributes[attr].ToString()))
                {
                    // Missing attribute!
                    missingAttrNames.Add(attr);
                }
                else
                {
                    presentAttrNames.Add(attr);
                }
            }
            
            if (missingAttrNames.Any())
            {
                if (presentAttrNames.Any())
                {
                    // At least 1 attribute was present indicating the user intended to use the tag helper,
                    // but at least 1 was missing too, so log a warning with the details.
                    _logger.WriteWarning(new MissingAttributeStructure(context.UniqueId, missingAttrNames));
                }
                return false;
            }
            
            // All attributes present and valid
            return true;
        }

        private static string LoadJavaScriptTemplate()
        {
            // Load the fallback JavaScript template from embedded resource
            using (var resourceStream = ResourcesAssembly.GetManifestResourceStream(FallbackJavaScriptResourceName))
            {
                Debug.Assert(resourceStream != null, "Embedded resource missing. Ensure 'prebuild' script has run.");

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

        /// <summary>
        /// Encodes a .NET string for safe use as a JavaScript string literal, including inline in an HTML file.
        /// </summary>
        private static string JavaScriptStringEncode(string value)
        {
            var map = new Dictionary<char, string>
            {
                { '<', @"\u003c" },
                { '>', @"\u003e" },
                { '\'', @"\u0027" },
                { '"', @"\u0022" },
                { '\\', @"\\" },
                { '\r', "\\r" },
                { '\n', "\\n" }
            };
            var result = new StringBuilder();

            foreach (var c in value)
            {
                if (map.ContainsKey(c))
                {
                    result.Append(map[c]);
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }
    }
}