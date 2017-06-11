// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers.Internal;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.AspNetCore.Mvc.TagHelpers
{
    /// <summary>
    /// <see cref="ITagHelper"/> implementation targeting &lt;img&gt; elements that supports file versioning.
    /// </summary>
    /// <remarks>
    /// The tag helper won't process for cases with just the 'src' attribute.
    /// </remarks>
    [HtmlTargetElement(
        "img",
        Attributes = AppendVersionAttributeName + "," + SrcAttributeName,
        TagStructure = TagStructure.WithoutEndTag)]
    public class ImageTagHelper : UrlResolutionTagHelper
    {
        private const string AppendVersionAttributeName = "asp-append-version";
        private const string SrcAttributeName = "src";

        private FileVersionProvider _fileVersionProvider;

        /// <summary>
        /// Creates a new <see cref="ImageTagHelper"/>.
        /// </summary>
        /// <param name="hostingEnvironment">The <see cref="IHostingEnvironment"/>.</param>
        /// <param name="cache">The <see cref="IMemoryCache"/>.</param>
        /// <param name="htmlEncoder">The <see cref="HtmlEncoder"/> to use.</param>
        /// <param name="urlHelperFactory">The <see cref="IUrlHelperFactory"/>.</param>
        public ImageTagHelper(
            IHostingEnvironment hostingEnvironment,
            IMemoryCache cache,
            HtmlEncoder htmlEncoder,
            IUrlHelperFactory urlHelperFactory)
            : base(urlHelperFactory, htmlEncoder)
        {
            HostingEnvironment = hostingEnvironment;
            Cache = cache;
        }

        /// <inheritdoc />
        public override int Order => -1000;

        /// <summary>
        /// Source of the image.
        /// </summary>
        /// <remarks>
        /// Passed through to the generated HTML in all cases.
        /// </remarks>
        [HtmlAttributeName(SrcAttributeName)]
        public string Src { get; set; }

        /// <summary>
        /// Value indicating if file version should be appended to the src urls.
        /// </summary>
        /// <remarks>
        /// If <c>true</c> then a query string "v" with the encoded content of the file is added.
        /// </remarks>
        [HtmlAttributeName(AppendVersionAttributeName)]
        public bool AppendVersion { get; set; }

        protected IHostingEnvironment HostingEnvironment { get; }

        protected IMemoryCache Cache { get; }

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

            output.CopyHtmlAttribute(SrcAttributeName, context);
            ProcessUrlAttribute(SrcAttributeName, output);

            if (AppendVersion)
            {
                EnsureFileVersionProvider();

                // Retrieve the TagHelperOutput variation of the "src" attribute in case other TagHelpers in the
                // pipeline have touched the value. If the value is already encoded this ImageTagHelper may
                // not function properly.
                Src = output.Attributes[SrcAttributeName].Value as string;

                output.Attributes.SetAttribute(SrcAttributeName, _fileVersionProvider.AddFileVersionToPath(Src));
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
    }
}