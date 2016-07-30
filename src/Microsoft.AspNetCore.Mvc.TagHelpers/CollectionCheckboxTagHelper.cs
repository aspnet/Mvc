// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.TagHelpers
{
    [HtmlTargetElement("input", Attributes = ForCollectionAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    public class CollectionCheckboxTagHelper : TagHelper
    {
        private const string ForCollectionAttributeName = "asp-for-collection";

        /// <summary>
        /// Creates a new <see cref="CollectionCheckboxTagHelper"/>.
        /// </summary>
        /// <param name="generator">The <see cref="IHtmlGenerator"/>.</param>
        public CollectionCheckboxTagHelper(IHtmlGenerator generator)
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

        protected IHtmlGenerator Generator { get; }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// An expression to be evaluated against the current model.
        /// </summary>
        [HtmlAttributeName(ForCollectionAttributeName)]
        public ModelExpression ForCollection { get; set; }

        /// <summary>
        /// The value of the &lt;input&gt; element.
        /// </summary>
        /// <remarks>
        /// Passed through to the generated HTML in all cases. Also used to determine the generated "checked" attribute
        /// </remarks>
        public Object Value { get; set; }

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

            var metadata = ForCollection.Metadata;
            var modelExplorer = ForCollection.ModelExplorer;
            if (metadata == null)
            {
                throw new InvalidOperationException(Resources.FormatTagHelpers_NoProvidedMetadata(
                    "<input>",
                    ForCollectionAttributeName,
                    nameof(IModelMetadataProvider),
                    ForCollection.Name));
            }

            // Prepare to move attributes from current element to <input type="checkbox"/> generated just below.
            var htmlAttributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            // Perf: Avoid allocating enumerator
            // Construct attributes correctly (first attribute wins).
            for (var i = 0; i < output.Attributes.Count; i++)
            {
                var attribute = output.Attributes[i];
                if (!htmlAttributes.ContainsKey(attribute.Name))
                {
                    htmlAttributes.Add(attribute.Name, attribute.Value);
                }
            }

            // Checking if the model array contains the passed in value
            bool? isChecked = null;
            if (ForCollection.Model != null && ForCollection.Model is Array)
            {
                var modelItems = ForCollection.Model as Array;
                foreach (var item in ForCollection.Model as Array)
                {
                    if (object.Equals(item, Value))
                    {
                        isChecked = true;
                        break;
                    }
                }
            }

            var checkBoxTag = Generator.GenerateCheckBox(
                ViewContext,
                modelExplorer,
                ForCollection.Name,
                isChecked: isChecked,
                htmlAttributes: htmlAttributes,
                idModifier: Value?.ToString());
            if (checkBoxTag != null)
            {
                output.Attributes.Clear();
                output.TagName = null;

                output.Content.AppendHtml(checkBoxTag);
            }
        }
    }
}
