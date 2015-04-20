// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Rendering.Internal;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public static class DefaultEditorTemplates
    {
        private const string HtmlAttributeKey = "htmlAttributes";

        public static HtmlString BooleanTemplate(IHtmlHelper htmlHelper)
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

        private static HtmlString BooleanTemplateCheckbox(IHtmlHelper htmlHelper, bool value)
        {
            return htmlHelper.CheckBox(
                expression: null,
                isChecked: value,
                htmlAttributes: CreateHtmlAttributes(htmlHelper, "check-box"));
        }

        private static HtmlString BooleanTemplateDropDownList(IHtmlHelper htmlHelper, bool? value)
        {
            return htmlHelper.DropDownList(
                expression: null,
                selectList: DefaultDisplayTemplates.TriStateValues(value),
                optionLabel: null,
                htmlAttributes: CreateHtmlAttributes(htmlHelper, "list-box tri-state"));
        }

        public static HtmlString CollectionTemplate(IHtmlHelper htmlHelper)
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

            var typeInCollection = typeof(string);
            var genericEnumerableType = collection.GetType().ExtractGenericInterface(typeof(IEnumerable<>));
            if (genericEnumerableType != null)
            {
                typeInCollection = genericEnumerableType.GetGenericArguments()[0];
            }

            var typeInCollectionIsNullableValueType = typeInCollection.IsNullableValueType();
            var oldPrefix = viewData.TemplateInfo.HtmlFieldPrefix;

            try
            {
                viewData.TemplateInfo.HtmlFieldPrefix = string.Empty;

                var fieldNameBase = oldPrefix;
                var result = new StringCollectionTextWriter(htmlHelper.ViewContext.Writer.Encoding);

                var serviceProvider = htmlHelper.ViewContext.HttpContext.RequestServices;
                var metadataProvider = serviceProvider.GetRequiredService<IModelMetadataProvider>();
                var viewEngine = serviceProvider.GetRequiredService<ICompositeViewEngine>();

                var index = 0;
                foreach (var item in collection)
                {
                    var itemType = typeInCollection;
                    if (item != null && !typeInCollectionIsNullableValueType)
                    {
                        itemType = item.GetType();
                    }

                    var modelExplorer = metadataProvider.GetModelExplorerForType(itemType, item);
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

                    templateBuilder.Build().WriteTo(result);
                }

                return new HtmlString(result);
            }
            finally
            {
                viewData.TemplateInfo.HtmlFieldPrefix = oldPrefix;
            }
        }

        public static HtmlString DecimalTemplate(IHtmlHelper htmlHelper)
        {
            if (htmlHelper.ViewData.TemplateInfo.FormattedModelValue == htmlHelper.ViewData.Model)
            {
                htmlHelper.ViewData.TemplateInfo.FormattedModelValue =
                    string.Format(CultureInfo.CurrentCulture, "{0:0.00}", htmlHelper.ViewData.Model);
            }

            return StringTemplate(htmlHelper);
        }

        public static HtmlString HiddenInputTemplate(IHtmlHelper htmlHelper)
        {
            var viewData = htmlHelper.ViewData;
            var model = viewData.Model;

            var result = new StringCollectionTextWriter(htmlHelper.ViewContext.Writer.Encoding);
            if (viewData.ModelMetadata.HideSurroundingHtml)
            {
                result.Write(string.Empty);
            }
            else
            {
                result.Write(DefaultDisplayTemplates.StringTemplate(htmlHelper));
            }

            // Special-case opaque values and arbitrary binary data.
            var modelAsByteArray = model as byte[];
            if (modelAsByteArray != null)
            {
                model = Convert.ToBase64String(modelAsByteArray);
            }

            var htmlAttributesObject = viewData[HtmlAttributeKey];
            var hiddenResult = htmlHelper.Hidden(expression: null, value: model, htmlAttributes: htmlAttributesObject);
            hiddenResult.WriteTo(result);

            return new HtmlString(result);
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

        public static HtmlString MultilineTemplate(IHtmlHelper htmlHelper)
        {
            return htmlHelper.TextArea(
                expression: string.Empty,
                value: htmlHelper.ViewContext.ViewData.TemplateInfo.FormattedModelValue.ToString(),
                rows: 0,
                columns: 0,
                htmlAttributes: CreateHtmlAttributes(htmlHelper, "text-box multi-line"));
        }

        public static HtmlString ObjectTemplate(IHtmlHelper htmlHelper)
        {
            var viewData = htmlHelper.ViewData;
            var templateInfo = viewData.TemplateInfo;
            var modelExplorer = viewData.ModelExplorer;
            var builder = new StringCollectionTextWriter(htmlHelper.ViewContext.Writer.Encoding);

            if (templateInfo.TemplateDepth > 1)
            {
                if (modelExplorer.Model == null)
                {
                    return new HtmlString(modelExplorer.Metadata.NullDisplayText);
                }

                var text = modelExplorer.GetSimpleDisplayText();
                if (modelExplorer.Metadata.HtmlEncode)
                {
                    text = htmlHelper.Encode(text);
                }

                return new HtmlString(text);
            }

            var serviceProvider = htmlHelper.ViewContext.HttpContext.RequestServices;
            var viewEngine = serviceProvider.GetRequiredService<ICompositeViewEngine>();

            foreach (var propertyExplorer in modelExplorer.Properties)
            {
                var propertyMetadata = propertyExplorer.Metadata;
                if (!ShouldShow(propertyExplorer, templateInfo))
                {
                    continue;
                }

                var divTag = new TagBuilder("div", htmlHelper.HtmlEncoder);

                if (!propertyMetadata.HideSurroundingHtml)
                {
                    var label = htmlHelper.Label(
                        propertyMetadata.PropertyName,
                        labelText: null,
                        htmlAttributes: null)
                            .ToString();
                    if (!string.IsNullOrEmpty(label))
                    {
                        divTag.AddCssClass("editor-label");
                        divTag.InnerHtml = new HtmlString(label); // already escaped
                        divTag.WriteTo(builder, TagRenderMode.Normal);
                        builder.WriteLine();

                        // Reset divTag for reuse.
                        divTag.Attributes.Clear();
                    }

                    divTag.AddCssClass("editor-field");
                    divTag.WriteTo(builder, TagRenderMode.StartTag);
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

                templateBuilder.Build().WriteTo(builder);

                if (!propertyMetadata.HideSurroundingHtml)
                {
                    builder.Write(" ");
                    builder.Write(htmlHelper.ValidationMessage(
                        propertyMetadata.PropertyName,
                        message: null,
                        htmlAttributes: null,
                        tag: null));

                    divTag.WriteTo(builder, TagRenderMode.EndTag);
                    builder.WriteLine();
                }
            }

            return new HtmlString(builder);
        }

        public static HtmlString PasswordTemplate(IHtmlHelper htmlHelper)
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

        public static HtmlString StringTemplate(IHtmlHelper htmlHelper)
        {
            return GenerateTextBox(htmlHelper);
        }

        public static HtmlString PhoneNumberInputTemplate(IHtmlHelper htmlHelper)
        {
            return GenerateTextBox(htmlHelper, inputType: "tel");
        }

        public static HtmlString UrlInputTemplate(IHtmlHelper htmlHelper)
        {
            return GenerateTextBox(htmlHelper, inputType: "url");
        }

        public static HtmlString EmailAddressInputTemplate(IHtmlHelper htmlHelper)
        {
            return GenerateTextBox(htmlHelper, inputType: "email");
        }

        public static HtmlString DateTimeInputTemplate(IHtmlHelper htmlHelper)
        {
            ApplyRfc3339DateFormattingIfNeeded(htmlHelper, "{0:yyyy-MM-ddTHH:mm:ss.fffK}");
            return GenerateTextBox(htmlHelper, inputType: "datetime");
        }

        public static HtmlString DateTimeLocalInputTemplate(IHtmlHelper htmlHelper)
        {
            ApplyRfc3339DateFormattingIfNeeded(htmlHelper, "{0:yyyy-MM-ddTHH:mm:ss.fff}");
            return GenerateTextBox(htmlHelper, inputType: "datetime-local");
        }

        public static HtmlString DateInputTemplate(IHtmlHelper htmlHelper)
        {
            ApplyRfc3339DateFormattingIfNeeded(htmlHelper, "{0:yyyy-MM-dd}");
            return GenerateTextBox(htmlHelper, inputType: "date");
        }

        public static HtmlString TimeInputTemplate(IHtmlHelper htmlHelper)
        {
            ApplyRfc3339DateFormattingIfNeeded(htmlHelper, "{0:HH:mm:ss.fff}");
            return GenerateTextBox(htmlHelper, inputType: "time");
        }

        public static HtmlString NumberInputTemplate(IHtmlHelper htmlHelper)
        {
            return GenerateTextBox(htmlHelper, inputType: "number");
        }

        public static HtmlString FileInputTemplate([NotNull] IHtmlHelper htmlHelper)
        {
            return GenerateTextBox(htmlHelper, inputType: "file");
        }

        public static HtmlString FileCollectionInputTemplate([NotNull] IHtmlHelper htmlHelper)
        {
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

        private static HtmlString GenerateTextBox(IHtmlHelper htmlHelper, string inputType = null)
        {
            return GenerateTextBox(htmlHelper, inputType, htmlHelper.ViewData.TemplateInfo.FormattedModelValue);
        }

        private static HtmlString GenerateTextBox(IHtmlHelper htmlHelper, string inputType, object value)
        {
            var htmlAttributes =
                CreateHtmlAttributes(htmlHelper, className: "text-box single-line", inputType: inputType);

            return GenerateTextBox(htmlHelper, value, htmlAttributes);
        }

        private static HtmlString GenerateTextBox(IHtmlHelper htmlHelper, object value, object htmlAttributes)
        {
            return htmlHelper.TextBox(
                current: null,
                value: value,
                format: null,
                htmlAttributes: htmlAttributes);
        }
    }
}
