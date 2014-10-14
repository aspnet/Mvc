﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
    /// <summary>
    /// <see cref="ITagHelper"/> implementation targeting &lt;textarea&gt; elements.
    /// </summary>
    public class TextAreaTagHelper : TagHelper
    {
        [Activate]
        private IHtmlGenerator Generator { get; set; }

        [Activate]
        private ViewContext ViewContext { get; set; }

        /// <summary>
        /// An expression to be evaluated against the current model.
        /// </summary>
        public ModelExpression For { get; set; }

        /// <inheritdoc />
        /// <remarks>Does nothing unless user binds "for" attribute in Razor source.</remarks>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (For != null)
            {
                var tagBuilder = Generator.GenerateTextArea(
                    ViewContext,
                    For.Metadata,
                    For.Name,
                    rows: 0,
                    columns: 0,
                    htmlAttributes: null);

                if (tagBuilder != null)
                {
                    foreach (var attribute in tagBuilder.Attributes)
                    {
                        // TODO: Should special-case the "class" attribute.
                        if (!output.Attributes.ContainsKey(attribute.Key))
                        {
                            output.Attributes.Add(attribute.Key, attribute.Value);
                        }
                    }

                    output.Content = tagBuilder.InnerHtml;
                    output.SelfClosing = false;
                    output.TagName = tagBuilder.TagName;
                }
            }
        }
    }
}