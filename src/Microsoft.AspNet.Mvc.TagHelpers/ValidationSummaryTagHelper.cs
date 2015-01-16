﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.AspNet.Razor.TagHelpers;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
    /// <summary>
    /// <see cref="ITagHelper"/> implementation targeting &lt;div&gt; elements with an <c>asp-validation-summary</c>
    /// attribute.
    /// </summary>
    [HtmlElementName("div")]
    [ContentBehavior(ContentBehavior.Append)]
    public class ValidationSummaryTagHelper : TagHelper
    {
        private const string ValidationSummaryAttributeName = "asp-validation-summary";
        private ValidationSummary _validationSummary;

        // Protected to ensure subclasses are correctly activated. Internal for ease of use when testing.
        [Activate]
        protected internal ViewContext ViewContext { get; set; }

        // Protected to ensure subclasses are correctly activated. Internal for ease of use when testing.
        [Activate]
        protected internal IHtmlGenerator Generator { get; set; }

        /// <summary>
        /// If <see cref="ValidationSummary.All"/> or <see cref="ValidationSummary.ModelOnly"/>, appends a validation
        /// summary. Otherwise (<see cref="ValidationSummary.None"/>, the default), this tag helper does nothing.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Thrown if setter is called with an undefined <see cref="ValidationSummary"/> value e.g.
        /// <c>(ValidationSummary)23</c>.
        /// </exception>
        [HtmlAttributeName(ValidationSummaryAttributeName)]
        public ValidationSummary ValidationSummary
        {
            get
            {
                return _validationSummary;
            }
            set
            {
                switch (value)
                {
                    case ValidationSummary.All:
                    case ValidationSummary.ModelOnly:
                    case ValidationSummary.None:
                        _validationSummary = value;
                        break;

                    default:
                        throw new ArgumentException(
                            message: Resources.FormatInvalidEnumArgument(
                                nameof(value),
                                value,
                                typeof(ValidationSummary).FullName),
                            paramName: nameof(value));
                }
            }
        }

        /// <inheritdoc />
        /// <remarks>Does nothing if <see cref="ValidationSummary"/> is <see cref="ValidationSummary.None"/>.</remarks>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (ValidationSummary == ValidationSummary.None)
            {
                return;
            }

            var tagBuilder = Generator.GenerateValidationSummary(
                ViewContext,
                excludePropertyErrors: ValidationSummary == ValidationSummary.ModelOnly,
                message: null,
                headerTag: null,
                htmlAttributes: null);
            if (tagBuilder != null)
            {
                output.MergeAttributes(tagBuilder);
                output.Content += tagBuilder.InnerHtml;
            }
        }
    }
}