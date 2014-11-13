// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.AspNet.FileSystems;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.AspNet.Razor.TagHelpers;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
    /// <summary>
    /// <see cref="ITagHelper"/> implementation targeting &lt;script&gt; elements.
    /// </summary>
    public class ScriptTagHelper : TagHelper
    {
        // Protected to ensure subclasses are correctly activated. Internal for ease of use when testing.
        [Activate]
        protected internal ViewContext ViewContext { get; set; }

        // Protected to ensure subclasses are correctly activated. Internal for ease of use when testing.
        [Activate]
        protected internal IHostingEnvironment HostingEnvironment { get; set; }

        // Protected to ensure subclasses are correctly activated. Internal for ease of use when testing.
        [Activate]
        protected internal IOptions<ScriptTagHelperOptions> Options { get; set; }

        /// <summary>
        /// The file extension of minified JavaScript files.
        /// Set to <see cref="string.Empty"/> to disable min file replacement.
        /// Defaults to ".min.js".
        /// </summary>
        [HtmlAttributeName("min-extension")]
        public string MinExtension { get; set; }

        /// <inheritdoc />
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!output.Attributes.ContainsKey("src") || (MinExtension != null && MinExtension.Trim() == string.Empty))
            {
                // No src or minfied file replacement disabled, do nothing
                return;
            }

            var minFileSrc = MinFileSrc(output.Attributes["src"], MinExtension ?? Options.Options.MinExtension);
            if (!string.IsNullOrEmpty(minFileSrc))
            {
                output.Attributes["src"] = minFileSrc;
            }
        }

        private string MinFileSrc(string src, string minExtension)
        {
            if (string.IsNullOrWhiteSpace(minExtension))
            {
                return null;
            }

            src = src.Trim();

            string srcPath;

            if (src.StartsWith("/"))
            {
                // Site root absolute path, resolve from webroot
                var srcPathString = new Http.PathString(src);

                Http.PathString webRootRelative;
                if (srcPathString.StartsWithSegments(ViewContext.HttpContext.Request.PathBase, out webRootRelative))
                {
                    srcPath = webRootRelative.Value;
                }
                else
                {
                    // Src is outside of configured webroot/PathBase so we won't be able to detect a minified file either way
                    return null;
                }
            }
            else
            {
                // Request relative path, resolve from current request path
                var pathValue = ViewContext.HttpContext.Request.Path.Value;
                if (pathValue.EndsWith("/"))
                {
                    // Request is to a logical folder so just append the relative src path
                    srcPath = pathValue + src;
                }
                else
                {
                    // Strip the last segment and append relative src path to that
                    srcPath = pathValue.Substring(0, pathValue.LastIndexOf('/') + 1) + src;
                }
            }

            var srcMinFilePath = Path.ChangeExtension(srcPath, minExtension);

            var webRootFiles = new PhysicalFileSystem(HostingEnvironment.WebRoot);

            IFileInfo file;
            if (webRootFiles.TryGetFileInfo(srcMinFilePath, out file))
            {
                // The minifed file exists, return its path
                return Path.ChangeExtension(ViewContext.HttpContext.Request.PathBase + srcPath, minExtension);
            }

            // No minified file was found
            return null;
        }
    }
}