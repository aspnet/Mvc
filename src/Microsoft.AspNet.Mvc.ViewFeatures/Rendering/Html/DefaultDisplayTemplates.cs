// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNet.Html.Abstractions;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Rendering.Internal;
using Microsoft.AspNet.Mvc.ViewFeatures;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public static class DefaultDisplayTemplates
    {
        public static IHtmlContent BooleanTemplate(IHtmlHelper htmlHelper)
        {
            bool? value = null;
            if (htmlHelper.ViewData.Model != null)
            {
                value = Convert.ToBoolean(htmlHelper.ViewData.Model, CultureInfo.InvariantCulture);
            }

            return htmlHelper.ViewData.ModelMetadata.IsNullableValueType ?
                BooleanTemplateDropDownList(htmlHelper, value) :
                BooleanTemplateCheckbox(value ?? false, htmlHelper);
        }

        private static IHtmlContent BooleanTemplateCheckbox(bool value, IHtmlHelper htmlHelper)
        {
            var inputTag = new TagBuilder("input");
            inputTag.AddCssClass("check-box");
            inputTag.Attributes["disabled"] = "disabled";
            inputTag.Attributes["type"] = "checkbox";
            if (value)
            {
                inputTag.Attributes["checked"] = "checked";
            }

            inputTag.TagRenderMode = TagRenderMode.SelfClosing;
            return inputTag;
        }

        private static IHtmlContent BooleanTemplateDropDownList(IHtmlHelper htmlHelper, bool? value)
        {
            var selectTag = new TagBuilder("select");
            selectTag.AddCssClass("list-box");
            selectTag.AddCssClass("tri-state");
            selectTag.Attributes["disabled"] = "disabled";

            var content = new BufferedHtmlContent();
            foreach (var item in TriStateValues(value))
            {
                content.Append(DefaultHtmlGenerator.GenerateOption(item, item.Text));
            }

            selectTag.InnerHtml = content;
            return selectTag;
        }

        // Will soon need to be shared with the default editor templates implementations.
        internal static List<SelectListItem> TriStateValues(bool? value)
        {
            return new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = Resources.Common_TriState_NotSet,
                    Value = string.Empty,
                    Selected = !value.HasValue
                },
                new SelectListItem
                {
                    Text = Resources.Common_TriState_True,
                    Value = "true",
                    Selected = (value == true),
                },
                new SelectListItem
                {
                    Text = Resources.Common_TriState_False,
                    Value = "false",
                    Selected = (value == false),
                },
            };
        }

        public static IHtmlContent CollectionTemplate(IHtmlHelper htmlHelper)
        {
            var model = htmlHelper.ViewData.Model;
            if (model == null)
            {
                return HtmlString.Empty;
            }

            var collection = model as IEnumerable;
            if (collection == null)
            {
                // Only way we could reach here is if user passed templateName: "Collection" to a Display() overload.
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

            var oldPrefix = htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix;
            try
            {
                htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix = string.Empty;

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
                        readOnly: true,
                        additionalViewData: null);
                    result.Append(templateBuilder.Build());
                }

                return result;
            }
            finally
            {
                htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix = oldPrefix;
            }
        }

        public static IHtmlContent DecimalTemplate(IHtmlHelper htmlHelper)
        {
            if (htmlHelper.ViewData.TemplateInfo.FormattedModelValue == htmlHelper.ViewData.Model)
            {
                htmlHelper.ViewData.TemplateInfo.FormattedModelValue =
                    string.Format(CultureInfo.CurrentCulture, "{0:0.00}", htmlHelper.ViewData.Model);
            }

            return StringTemplate(htmlHelper);
        }

        public static IHtmlContent EmailAddressTemplate(IHtmlHelper htmlHelper)
        {
            var uriString = "mailto:" + ((htmlHelper.ViewData.Model == null) ?
                string.Empty :
                htmlHelper.ViewData.Model.ToString());
            var linkedText = (htmlHelper.ViewData.TemplateInfo.FormattedModelValue == null) ?
                string.Empty :
                htmlHelper.ViewData.TemplateInfo.FormattedModelValue.ToString();

            return HyperlinkTemplate(uriString, linkedText, htmlHelper);
        }

        public static IHtmlContent HiddenInputTemplate(IHtmlHelper htmlHelper)
        {
            if (htmlHelper.ViewData.ModelMetadata.HideSurroundingHtml)
            {
                return HtmlString.Empty;
            }

            return StringTemplate(htmlHelper);
        }

        public static IHtmlContent HtmlTemplate(IHtmlHelper htmlHelper)
        {
            return new HtmlString(htmlHelper.ViewData.TemplateInfo.FormattedModelValue.ToString());
        }

        public static IHtmlContent ObjectTemplate(IHtmlHelper htmlHelper)
        {
            var viewData = htmlHelper.ViewData;
            var templateInfo = viewData.TemplateInfo;
            var modelExplorer = viewData.ModelExplorer;

            if (modelExplorer.Model == null)
            {
                return new HtmlString(modelExplorer.Metadata.NullDisplayText);
            }

            if (templateInfo.TemplateDepth > 1)
            {
                var text = modelExplorer.GetSimpleDisplayText();
                if (modelExplorer.Metadata.HtmlEncode)
                {
                    text = htmlHelper.Encode(text);
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

                if (!propertyMetadata.HideSurroundingHtml)
                {
                    var label = propertyMetadata.GetDisplayName();
                    if (!string.IsNullOrEmpty(label))
                    {
                        var labelTag = new TagBuilder("div");
                        labelTag.SetInnerText(label);
                        labelTag.AddCssClass("display-label");
                        content.AppendLine(labelTag);
                    } 
                }

                var templateBuilder = new TemplateBuilder(
                    viewEngine,
                    htmlHelper.ViewContext,
                    htmlHelper.ViewData,
                    propertyExplorer,
                    htmlFieldName: propertyMetadata.PropertyName,
                    templateName: null,
                    readOnly: true,
                    additionalViewData: null);

                var templateBuilderResult = templateBuilder.Build();
                if (!propertyMetadata.HideSurroundingHtml)
                {
                    var valueDivTag = new TagBuilder("div");
                    valueDivTag.AddCssClass("display-field");
                    valueDivTag.InnerHtml = templateBuilderResult;
                    content.AppendLine(valueDivTag);
                }
                else
                {
                    content.Append(templateBuilderResult);
                }
            }

            return content;
        }

        private static bool ShouldShow(ModelExplorer modelExplorer, TemplateInfo templateInfo)
        {
            return
                modelExplorer.Metadata.ShowForDisplay &&
                !modelExplorer.Metadata.IsComplexType &&
                !templateInfo.Visited(modelExplorer);
        }

        public static IHtmlContent StringTemplate(IHtmlHelper htmlHelper)
        {
            var value = htmlHelper.ViewData.TemplateInfo.FormattedModelValue;
            if (value == null)
            {
                return HtmlString.Empty;
            }

            return new StringHtmlContent(value.ToString());
        }

        public static IHtmlContent UrlTemplate(IHtmlHelper htmlHelper)
        {
            var uriString = (htmlHelper.ViewData.Model == null) ? string.Empty : htmlHelper.ViewData.Model.ToString();
            var linkedText = (htmlHelper.ViewData.TemplateInfo.FormattedModelValue == null) ?
                string.Empty :
                htmlHelper.ViewData.TemplateInfo.FormattedModelValue.ToString();

            return HyperlinkTemplate(uriString, linkedText, htmlHelper);
        }

        // Neither uriString nor linkedText need be encoded prior to calling this method.
        private static IHtmlContent HyperlinkTemplate(string uriString, string linkedText, IHtmlHelper htmlHelper)
        {
            var hyperlinkTag = new TagBuilder("a");
            hyperlinkTag.MergeAttribute("href", uriString);
            hyperlinkTag.SetInnerText(linkedText);
            return hyperlinkTag;
        }
    }
}
