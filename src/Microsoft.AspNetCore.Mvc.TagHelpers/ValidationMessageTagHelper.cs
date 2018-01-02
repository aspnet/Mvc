// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.TagHelpers
{
    /// <summary>
    /// <see cref="ITagHelper"/> implementation targeting any HTML element with an <c>asp-validation-for</c>
    /// attribute.
    /// </summary>
    [HtmlTargetElement("span", Attributes = ValidationForAttributeName)]
    public class ValidationMessageTagHelper : TagHelper
    {
        private const string DataValidationForAttributeName = "data-valmsg-for";
        private const string ValidationForAttributeName = "asp-validation-for";

        /// <summary>
        /// Creates a new <see cref="ValidationMessageTagHelper"/>.
        /// </summary>
        /// <param name="generator">The <see cref="IHtmlGenerator"/>.</param>
        public ValidationMessageTagHelper(IHtmlGenerator generator)
        {
            Generator = generator;
        }

        /// <inheritdoc />
        public override int Order => -1000;

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        protected IHtmlGenerator Generator { get; }

        /// <summary>
        /// The name of an &lt;input&gt; element in the current &lt;form&gt; that has the same <see cref="For"/>
        /// expression.
        /// </summary>
        /// <remarks>
        /// Passed through to the generated HTML in all cases. Also used to determine whether <see cref="For"/> is
        /// valid with an empty <see cref="ModelExpression.Name"/>.
        /// </remarks>
        [HtmlAttributeName(DataValidationForAttributeName)]
        public string DataValidationFor { get; set; }

        /// <summary>
        /// An expression on the current model for which the associated element should contain validation messages.
        /// </summary>
        [HtmlAttributeName(ValidationForAttributeName)]
        public ModelExpression For { get; set; }

        /// <inheritdoc />
        /// <remarks>Does nothing if <see cref="For"/> is <c>null</c>.</remarks>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            // Pass through attribute that is also an HTML attribute. Must be done prior to any copying from a
            // TagBuilder.
            if (DataValidationFor != null)
            {
                output.CopyHtmlAttribute(DataValidationForAttributeName, context);
            }

            if (For != null)
            {
                // Ensure Generator does not throw due to empty "fullName" if user provided data-valmsg-for attribute.
                IDictionary<string, object> htmlAttributes = null;
                if (string.IsNullOrEmpty(For.Name) &&
                    string.IsNullOrEmpty(ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix) &&
                    !string.IsNullOrEmpty(DataValidationFor))
                {
                    htmlAttributes = new Dictionary<string, object>
                    {
                        { DataValidationForAttributeName, DataValidationFor },
                    };
                }

                var tagBuilder = Generator.GenerateValidationMessage(
                    ViewContext,
                    For.ModelExplorer,
                    For.Name,
                    message: null,
                    tag: null,
                    htmlAttributes: htmlAttributes);

                if (tagBuilder != null)
                {
                    output.MergeAttributes(tagBuilder);

                    // Do not update the content if another tag helper targeting this element has already done so.
                    if (!output.IsContentModified)
                    {
                        // We check for whitespace to detect scenarios such as:
                        // <span validation-for="Name">
                        // </span>
                        var childContent = await output.GetChildContentAsync();
                        if (childContent.IsEmptyOrWhiteSpace)
                        {
                            // Provide default message text (if any) since there was nothing useful in the Razor source.
                            if (tagBuilder.HasInnerHtml)
                            {
                                output.Content.SetHtmlContent(tagBuilder.InnerHtml);
                            }
                        }
                        else
                        {
                            output.Content.SetHtmlContent(childContent);
                        }
                    }
                }
            }
        }
    }
}