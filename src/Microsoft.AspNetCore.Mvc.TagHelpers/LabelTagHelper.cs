// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.TagHelpers
{
    /// <summary>
    /// <see cref="ITagHelper"/> implementation targeting &lt;label&gt; elements with an <c>asp-for</c> attribute.
    /// </summary>
    [HtmlTargetElement("label", Attributes = ForAttributeName)]
    public class LabelTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";
        private const string ForValueAttributeName = "asp-for-value";

        /// <summary>
        /// Creates a new <see cref="LabelTagHelper"/>.
        /// </summary>
        /// <param name="generator">The <see cref="IHtmlGenerator"/>.</param>
        public LabelTagHelper(IHtmlGenerator generator)
        {
            Generator = generator;
        }

        /// <inheritdoc />
        public override int Order
        {
            get
            {
                return -1000;
            }
        }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        protected IHtmlGenerator Generator { get; }

        /// <summary>
        /// An expression to be evaluated against the current model.
        /// </summary>
        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        /// <summary>
        /// The value of the input checkbox or radio that this label is used for.
        /// Used to generate a unique html element id
        /// </summary>
        [HtmlAttributeName(ForValueAttributeName)]
        public Object ForValue { get; set; }

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

            var tagBuilder = Generator.GenerateLabel(
                ViewContext,
                For.ModelExplorer,
                For.Name,
                labelText: null,
                htmlAttributes: null,
                idModifier: ForValue?.ToString());

            if (tagBuilder != null)
            {
                output.MergeAttributes(tagBuilder);

                // We check for whitespace to detect scenarios such as:
                // <label for="Name">
                // </label>
                if (!output.IsContentModified)
                {
                    var childContent = await output.GetChildContentAsync();

                    if (childContent.IsEmptyOrWhiteSpace)
                    {
                        // Provide default label text since there was nothing useful in the Razor source.
                        output.Content.SetHtmlContent(tagBuilder.InnerHtml);
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