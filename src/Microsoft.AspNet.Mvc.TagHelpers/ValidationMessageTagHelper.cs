﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
    /// <summary>
    /// <see cref="ITagHelper"/> implementation targeting any HTML element with an <c>asp-validation-for</c>
    /// attribute.
    /// </summary>
    [TargetElement("span", Attributes = ValidationForAttributeName)]
    public class ValidationMessageTagHelper : TagHelper
    {
        private const string ValidationForAttributeName = "asp-validation-for";

        [Activate, HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        [Activate, HtmlAttributeNotBound]
        public IHtmlGenerator Generator { get; set; }

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

                    // We check for whitespace to detect scenarios such as:
                    // <span validation-for="Name">
                    // </span>
                    if (!output.IsContentModified)
                    {
                        var childContent = await context.GetChildContentAsync();

                        if (childContent.IsWhiteSpace)
                        {
                            // Provide default label text since there was nothing useful in the Razor source.
                            output.Content.SetContent(tagBuilder.InnerHtml);
                        }
                        else
                        {
                            output.Content.SetContent(childContent);
                        }
                    }
                }
            }
        }
    }
}