// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNet.Html.Abstractions;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Mvc.ViewEngines;
using Microsoft.AspNet.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNet.Mvc.ViewFeatures
{
    public class DefaultEditorTemplates : IDefaultEditorTemplates
    {
        private const string HtmlAttributeKey = "htmlAttributes";

        public IHtmlContent BooleanTemplate(IHtmlHelper htmlHelper)
        {
            bool? value = null;
            if (htmlHelper.ViewData.Model != null)
            {
                value = Convert.ToBoolean(htmlHelper.ViewData.Model, CultureInfo.InvariantCulture);
            }

            return htmlHelper.ViewData.ModelMetadata.IsNullableValueType ?
                BooleanTemplateDropDownList(htmlHelper, value) :
                BooleanTemplateCheckbox(htmlHelper, value ?? false);
        }

        private static IHtmlContent BooleanTemplateCheckbox(IHtmlHelper htmlHelper, bool value)
        {
            return htmlHelper.CheckBox(
                expression: null,
                isChecked: value,
                htmlAttributes: CreateHtmlAttributes(htmlHelper, "check-box"));
        }

        private static IHtmlContent BooleanTemplateDropDownList(IHtmlHelper htmlHelper, bool? value)
        {
            return htmlHelper.DropDownList(
                expression: null,
                selectList: DefaultDisplayTemplates.TriStateValues(value),
                optionLabel: null,
                htmlAttributes: CreateHtmlAttributes(htmlHelper, "list-box tri-state"));
        }

        public IHtmlContent CollectionTemplate(IHtmlHelper htmlHelper)
        {
            var viewData = htmlHelper.ViewData;
            var model = viewData.Model;
            if (model == null)
            {
                return HtmlString.Empty;
            }

            var collection = model as IEnumerable;
            if (collection == null)
            {
                // Only way we could reach here is if user passed templateName: "Collection" to an Editor() overload.
                throw new InvalidOperationException(Resources.FormatTemplates_TypeMustImplementIEnumerable(
                    "Collection", model.GetType().FullName, typeof(IEnumerable).FullName));
            }

            var elementMetadata = htmlHelper.ViewData.ModelMetadata.ElementMetadata;
            Debug.Assert(elementMetadata != null);
            var typeInCollectionIsNullableValueType = elementMetadata.IsNullableValueType;

            var serviceProvider = htmlHelper.ViewContext.HttpContext.RequestServices;
            var metadataProvider = serviceProvider.GetRequiredService<IModelMetadataProvider>();

            // Use typeof(string) instead of typeof(object) for IEnumerable collections. Neither type is Nullable<T>.
            if (elementMetadata.ModelType == typeof(object))
            {
                elementMetadata = metadataProvider.GetMetadataForType(typeof(string));
            }

            var oldPrefix = viewData.TemplateInfo.HtmlFieldPrefix;
            try
            {
                viewData.TemplateInfo.HtmlFieldPrefix = string.Empty;

                var fieldNameBase = oldPrefix;
                var result = new BufferedHtmlContent();
                var viewEngine = serviceProvider.GetRequiredService<ICompositeViewEngine>();

                var index = 0;
                foreach (var item in collection)
                {
                    var itemMetadata = elementMetadata;
                    if (item != null && !typeInCollectionIsNullableValueType)
                    {
                        itemMetadata = metadataProvider.GetMetadataForType(item.GetType());
                    }

                    var modelExplorer = new ModelExplorer(
                        metadataProvider,
                        container: htmlHelper.ViewData.ModelExplorer,
                        metadata: itemMetadata,
                        model: item);
                    var fieldName = string.Format(CultureInfo.InvariantCulture, "{0}[{1}]", fieldNameBase, index++);

                    var templateBuilder = new TemplateBuilder(
                        viewEngine,
                        htmlHelper.ViewContext,
                        htmlHelper.ViewData,
                        modelExplorer,
                        htmlFieldName: fieldName,
                        templateName: null,
                        readOnly: false,
                        additionalViewData: null);
                    result.Append(templateBuilder.Build());
                }

                return result;
            }
            finally
            {
                viewData.TemplateInfo.HtmlFieldPrefix = oldPrefix;
            }
        }

        public IHtmlContent DecimalTemplate(IHtmlHelper htmlHelper)
        {
            if (htmlHelper.ViewData.TemplateInfo.FormattedModelValue == htmlHelper.ViewData.Model)
            {
                htmlHelper.ViewData.TemplateInfo.FormattedModelValue =
                    string.Format(CultureInfo.CurrentCulture, "{0:0.00}", htmlHelper.ViewData.Model);
            }

            return StringTemplate(htmlHelper);
        }

        public IHtmlContent HiddenInputTemplate(IHtmlHelper htmlHelper)
        {
            var viewData = htmlHelper.ViewData;
            var model = viewData.Model;

            var result = new BufferedHtmlContent();
            if (!viewData.ModelMetadata.HideSurroundingHtml)
            {
                result.Append(StringTemplate(htmlHelper));
            }

            // Special-case opaque values and arbitrary binary data.
            var modelAsByteArray = model as byte[];
            if (modelAsByteArray != null)
            {
                model = Convert.ToBase64String(modelAsByteArray);
            }

            var htmlAttributesObject = viewData[HtmlAttributeKey];
            var hiddenResult = htmlHelper.Hidden(expression: null, value: model, htmlAttributes: htmlAttributesObject);
            result.Append(hiddenResult);

            return result;
        }

        private static IDictionary<string, object> CreateHtmlAttributes(
            IHtmlHelper htmlHelper,
            string className,
            string inputType = null)
        {
            var htmlAttributesObject = htmlHelper.ViewData[HtmlAttributeKey];
            if (htmlAttributesObject != null)
            {
                return MergeHtmlAttributes(htmlAttributesObject, className, inputType);
            }

            var htmlAttributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { "class", className }
            };

            if (inputType != null)
            {
                htmlAttributes.Add("type", inputType);
            }

            return htmlAttributes;
        }

        private static IDictionary<string, object> MergeHtmlAttributes(
            object htmlAttributesObject,
            string className,
            string inputType)
        {
            var htmlAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributesObject);

            object htmlClassObject;
            if (htmlAttributes.TryGetValue("class", out htmlClassObject))
            {
                var htmlClassName = htmlClassObject.ToString() + " " + className;
                htmlAttributes["class"] = htmlClassName;
            }
            else
            {
                htmlAttributes.Add("class", className);
            }

            // The input type from the provided htmlAttributes overrides the inputType parameter.
            if (inputType != null && !htmlAttributes.ContainsKey("type"))
            {
                htmlAttributes.Add("type", inputType);
            }

            return htmlAttributes;
        }

        public IHtmlContent MultilineTemplate(IHtmlHelper htmlHelper)
        {
            return htmlHelper.TextArea(
                expression: string.Empty,
                value: htmlHelper.ViewContext.ViewData.TemplateInfo.FormattedModelValue.ToString(),
                rows: 0,
                columns: 0,
                htmlAttributes: CreateHtmlAttributes(htmlHelper, "text-box multi-line"));
        }

        public IHtmlContent ObjectTemplate(IHtmlHelper htmlHelper)
        {
            var viewData = htmlHelper.ViewData;
            var templateInfo = viewData.TemplateInfo;
            var modelExplorer = viewData.ModelExplorer;

            if (templateInfo.TemplateDepth > 1)
            {
                if (modelExplorer.Model == null)
                {
                    return new HtmlString(modelExplorer.Metadata.NullDisplayText);
                }

                var text = modelExplorer.GetSimpleDisplayText();
                if (modelExplorer.Metadata.HtmlEncode)
                {
                    return new StringHtmlContent(text);
                }

                return new HtmlString(text);
            }

            var serviceProvider = htmlHelper.ViewContext.HttpContext.RequestServices;
            var viewEngine = serviceProvider.GetRequiredService<ICompositeViewEngine>();

            var content = new BufferedHtmlContent();
            foreach (var propertyExplorer in modelExplorer.Properties)
            {
                var propertyMetadata = propertyExplorer.Metadata;
                if (!ShouldShow(propertyExplorer, templateInfo))
                {
                    continue;
                }

                var templateBuilder = new TemplateBuilder(
                    viewEngine,
                    htmlHelper.ViewContext,
                    htmlHelper.ViewData,
                    propertyExplorer,
                    htmlFieldName: propertyMetadata.PropertyName,
                    templateName: null,
                    readOnly: false,
                    additionalViewData: null);

                var templateBuilderResult = templateBuilder.Build();
                if (!propertyMetadata.HideSurroundingHtml)
                {
                    var label = htmlHelper.Label(propertyMetadata.PropertyName, labelText: null, htmlAttributes: null);
                    if (!string.IsNullOrEmpty(label.ToString()))
                    {
                        var labelTag = new TagBuilder("div");
                        labelTag.AddCssClass("editor-label");
                        labelTag.InnerHtml.SetContent(label);
                        content.AppendLine(labelTag);
                    }

                    var valueDivTag = new TagBuilder("div");
                    valueDivTag.AddCssClass("editor-field");

                    valueDivTag.InnerHtml.Append(templateBuilderResult);
                    valueDivTag.InnerHtml.AppendHtml(" ");
                    valueDivTag.InnerHtml.Append(htmlHelper.ValidationMessage(
                        propertyMetadata.PropertyName,
                        message: null,
                        htmlAttributes: null,
                        tag: null));

                    content.AppendLine(valueDivTag);
                }
                else
                {
                    content.Append(templateBuilderResult);
                }
            }

            return content;
        }

        public IHtmlContent PasswordTemplate(IHtmlHelper htmlHelper)
        {
            return htmlHelper.Password(
                expression: null,
                value: htmlHelper.ViewData.TemplateInfo.FormattedModelValue,
                htmlAttributes: CreateHtmlAttributes(htmlHelper, "text-box single-line password"));
        }

        private static bool ShouldShow(ModelExplorer modelExplorer, TemplateInfo templateInfo)
        {
            return
                modelExplorer.Metadata.ShowForEdit &&
                !modelExplorer.Metadata.IsComplexType &&
                !templateInfo.Visited(modelExplorer);
        }

        public IHtmlContent StringTemplate(IHtmlHelper htmlHelper)
        {
            return GenerateTextBox(htmlHelper);
        }

        public IHtmlContent PhoneNumberInputTemplate(IHtmlHelper htmlHelper)
        {
            return GenerateTextBox(htmlHelper, inputType: "tel");
        }

        public IHtmlContent UrlInputTemplate(IHtmlHelper htmlHelper)
        {
            return GenerateTextBox(htmlHelper, inputType: "url");
        }

        public IHtmlContent EmailAddressInputTemplate(IHtmlHelper htmlHelper)
        {
            return GenerateTextBox(htmlHelper, inputType: "email");
        }

        public IHtmlContent DateTimeInputTemplate(IHtmlHelper htmlHelper)
        {
            ApplyRfc3339DateFormattingIfNeeded(htmlHelper, "{0:yyyy-MM-ddTHH:mm:ss.fffK}");
            return GenerateTextBox(htmlHelper, inputType: "datetime");
        }

        public IHtmlContent DateTimeLocalInputTemplate(IHtmlHelper htmlHelper)
        {
            ApplyRfc3339DateFormattingIfNeeded(htmlHelper, "{0:yyyy-MM-ddTHH:mm:ss.fff}");
            return GenerateTextBox(htmlHelper, inputType: "datetime-local");
        }

        public IHtmlContent DateInputTemplate(IHtmlHelper htmlHelper)
        {
            ApplyRfc3339DateFormattingIfNeeded(htmlHelper, "{0:yyyy-MM-dd}");
            return GenerateTextBox(htmlHelper, inputType: "date");
        }

        public IHtmlContent TimeInputTemplate(IHtmlHelper htmlHelper)
        {
            ApplyRfc3339DateFormattingIfNeeded(htmlHelper, "{0:HH:mm:ss.fff}");
            return GenerateTextBox(htmlHelper, inputType: "time");
        }

        public IHtmlContent NumberInputTemplate(IHtmlHelper htmlHelper)
        {
            return GenerateTextBox(htmlHelper, inputType: "number");
        }

        public IHtmlContent FileInputTemplate(IHtmlHelper htmlHelper)
        {
            if (htmlHelper == null)
            {
                throw new ArgumentNullException(nameof(htmlHelper));
            }

            return GenerateTextBox(htmlHelper, inputType: "file");
        }

        public IHtmlContent FileCollectionInputTemplate(IHtmlHelper htmlHelper)
        {
            if (htmlHelper == null)
            {
                throw new ArgumentNullException(nameof(htmlHelper));
            }

            var htmlAttributes =
                CreateHtmlAttributes(htmlHelper, className: "text-box single-line", inputType: "file");
            htmlAttributes["multiple"] = "multiple";

            return GenerateTextBox(htmlHelper, htmlHelper.ViewData.TemplateInfo.FormattedModelValue, htmlAttributes);
        }

        private static void ApplyRfc3339DateFormattingIfNeeded(IHtmlHelper htmlHelper, string format)
        {
            if (htmlHelper.Html5DateRenderingMode != Html5DateRenderingMode.Rfc3339)
            {
                return;
            }

            var metadata = htmlHelper.ViewData.ModelMetadata;
            var value = htmlHelper.ViewData.Model;
            if (htmlHelper.ViewData.TemplateInfo.FormattedModelValue != value && metadata.HasNonDefaultEditFormat)
            {
                return;
            }

            if (value is DateTime || value is DateTimeOffset)
            {
                htmlHelper.ViewData.TemplateInfo.FormattedModelValue =
                    string.Format(CultureInfo.InvariantCulture, format, value);
            }
        }

        private static IHtmlContent GenerateTextBox(IHtmlHelper htmlHelper, string inputType = null)
        {
            return GenerateTextBox(htmlHelper, inputType, htmlHelper.ViewData.TemplateInfo.FormattedModelValue);
        }

        private static IHtmlContent GenerateTextBox(IHtmlHelper htmlHelper, string inputType, object value)
        {
            var htmlAttributes =
                CreateHtmlAttributes(htmlHelper, className: "text-box single-line", inputType: inputType);

            return GenerateTextBox(htmlHelper, value, htmlAttributes);
        }

        private static IHtmlContent GenerateTextBox(IHtmlHelper htmlHelper, object value, object htmlAttributes)
        {
            return htmlHelper.TextBox(
                current: null,
                value: value,
                format: null,
                htmlAttributes: htmlAttributes);
        }
    }
}
