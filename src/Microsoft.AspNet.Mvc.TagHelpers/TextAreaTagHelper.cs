// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
    /// <summary>
    /// <see cref="ITagHelper"/> implementation targeting &lt;textarea&gt; elements with an <c>asp-for</c> attribute.
    /// </summary>
    [TargetElement("textarea", Attributes = ForAttributeName)]
    public class TextAreaTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";

        /// <summary>
        /// Creates a new <see cref="TextAreaTagHelper"/>.
        /// </summary>
        /// <param name="generator">The <see cref="IHtmlGenerator"/>.</param>
        public TextAreaTagHelper(IHtmlGenerator generator)
        {
            Generator = generator;
        }

        protected IHtmlGenerator Generator { get; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// An expression to be evaluated against the current model.
        /// </summary>
        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        /// <inheritdoc />
        /// <remarks>Does nothing if <see cref="For"/> is <c>null</c>.</remarks>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var tagBuilder = Generator.GenerateTextArea(
                ViewContext,
                For.ModelExplorer,
                For.Name,
                rows: 0,
                columns: 0,
                htmlAttributes: null);

            if (tagBuilder != null)
            {
                // Overwrite current Content to ensure expression result round-trips correctly.
                output.Content.SetContent(tagBuilder.InnerHtml);

                output.MergeAttributes(tagBuilder);
            }
        }
    }
}