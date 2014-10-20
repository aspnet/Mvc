﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.AspNet.Razor.TagHelpers;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
    /// <summary>
    /// <see cref="ITagHelper"/> implementation targeting &lt;div&gt; elements with a <c>validation-summary</c> 
    /// attribute.
    /// </summary>
    [TagName("div")]
    [ContentBehavior(ContentBehavior.Append)]
    public class ValidationSummaryTagHelper : TagHelper
    {
        // Protected to ensure subclasses are correctly activated. Internal for ease of use when testing.
        [Activate]
        protected internal ViewContext ViewContext { get; set; }

        // Protected to ensure subclasses are correctly activated. Internal for ease of use when testing.
        [Activate]
        protected internal IHtmlGenerator Generator { get; set; }

        // TODO: Change to ValidationSummary enum once https://github.com/aspnet/Razor/issues/196 has been completed.
        /// <summary>
        /// If <c>All</c> or <c>ModelOnly</c>, appends a validation summary. Acceptable values are defined by the 
        /// <see cref="ValidationSummary"/> enum.
        /// </summary>
        [HtmlAttributeName("validation-summary")]
        public string ValidationSummaryValue { get; set; }

        /// <inheritdoc />
        /// Does nothing if <see cref="ValidationSummaryValue"/> is <c>null</c>, empty or "None".
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!string.IsNullOrEmpty(ValidationSummaryValue))
            {
                ValidationSummary validationSummaryValue;
                if (!Enum.TryParse(ValidationSummaryValue, ignoreCase: true, result: out validationSummaryValue))
                {
                    throw new InvalidOperationException(
                        Resources.FormatTagHelpers_InvalidValue_ThreeAcceptableValues(
                            "<div>",
                            "validation-summary",
                            ValidationSummaryValue,
                            ValidationSummary.All,
                            ValidationSummary.ModelOnly,
                            ValidationSummary.None));
                }
                else if (validationSummaryValue == ValidationSummary.None)
                {
                    return;
                }

                var validationModelErrorsOnly = validationSummaryValue == ValidationSummary.ModelOnly;
                var tagBuilder = Generator.GenerateValidationSummary(
                    ViewContext,
                    excludePropertyErrors: validationModelErrorsOnly,
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
}