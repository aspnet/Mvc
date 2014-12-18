// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.AspNet.Razor.TagHelpers;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
    /// <summary>
    /// <see cref="ITagHelper"/> implementation targeting &lt;span&gt; elements with an <c>asp-validation-for</c>
    /// attribute.
    /// </summary>
    [HtmlElementName("span")]
    public class ValidationMessageTagHelper : TagHelper
    {
        private const string ValidationForAttributeName = "asp-validation-for";

        // Protected to ensure subclasses are correctly activated. Internal for ease of use when testing.
        [Activate]
        protected internal ViewContext ViewContext { get; set; }

        // Protected to ensure subclasses are correctly activated. Internal for ease of use when testing.
        [Activate]
        protected internal IHtmlGenerator Generator { get; set; }

        /// <summary>
        /// Name to be validated on the current model.
        /// </summary>
        [HtmlAttributeName(ValidationForAttributeName)]
        public ModelExpression For { get; set; }

        /// <inheritdoc />
        /// <remarks>Does nothing if <see cref="For"/> is <c>null</c>.</remarks>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (For != null)
            {
                var tagBuilder = Generator.GenerateValidationMessage(ViewContext,
                                                                     For.Name,
                                                                     message: null,
                                                                     tag: null,
                                                                     htmlAttributes: null);

                if (tagBuilder != null)
                {
                    output.MergeAttributes(tagBuilder);

                    var childContent = await context.GetChildContentAsync();

                    // We check for whitespace to detect scenarios such as:
                    // <span validation-for="Name">
                    // </span>
                    if (string.IsNullOrWhiteSpace(childContent) && string.IsNullOrEmpty(output.Content))
                    {
                        output.Content = tagBuilder.InnerHtml;
                    }
                }
            }
        }
    }
}