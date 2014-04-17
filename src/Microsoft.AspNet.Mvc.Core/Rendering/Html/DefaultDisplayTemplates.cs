using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.Data.Entity;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public static class DefaultDisplayTemplates
    {
        public static string BooleanTemplate(IHtmlHelper<object> html)
        {
            bool? value = null;
            if (html.ViewData.Model != null)
            {
                value = Convert.ToBoolean(html.ViewData.Model, CultureInfo.InvariantCulture);
            }

            return html.ViewData.ModelMetadata.IsNullableValueType ?
                BooleanTemplateDropDownList(html, value) :
                BooleanTemplateCheckbox(value ?? false);
        }

        private static string BooleanTemplateCheckbox(bool value)
        {
            var inputTag = new TagBuilder("input");
            inputTag.AddCssClass("check-box");
            inputTag.Attributes["disabled"] = "disabled";
            inputTag.Attributes["type"] = "checkbox";
            if (value)
            {
                inputTag.Attributes["checked"] = "checked";
            }

            return inputTag.ToString(TagRenderMode.SelfClosing);
        }

        private static string BooleanTemplateDropDownList(IHtmlHelper<object> html, bool? value)
        {
            var selectTag = new TagBuilder("select");
            selectTag.AddCssClass("list-box");
            selectTag.AddCssClass("tri-state");
            selectTag.Attributes["disabled"] = "disabled";

            var builder = new StringBuilder();
            builder.Append(selectTag.ToString(TagRenderMode.StartTag));

            foreach (var item in TriStateValues(value))
            {
                var encodedText = html.Encode(item.Text);
                var option = HtmlHelper.GenerateOption(item, encodedText);
                builder.Append(option);
            }

            builder.Append(selectTag.ToString(TagRenderMode.EndTag));
            return builder.ToString();
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

        public static string CollectionTemplate(IHtmlHelper<object> html)
        {
            var model = html.ViewData.ModelMetadata.Model;
            if (model == null)
            {
                return string.Empty;
            }

            var collection = model as IEnumerable;
            if (collection == null)
            {
                throw new InvalidOperationException(
                    Resources.FormatTemplates_TypeMustImplementIEnumerable(model.GetType().FullName));
            }

            var typeInCollection = typeof(string);
            var genericEnumerableType = collection.GetType().ExtractGenericInterface(typeof(IEnumerable<>));
            if (genericEnumerableType != null)
            {
                typeInCollection = genericEnumerableType.GetGenericArguments()[0];
            }

            var typeInCollectionIsNullableValueType = typeInCollection.IsNullableValueType();

            var oldPrefix = html.ViewData.TemplateInfo.HtmlFieldPrefix;

            try
            {
                html.ViewData.TemplateInfo.HtmlFieldPrefix = string.Empty;

                var fieldNameBase = oldPrefix;
                var result = new StringBuilder();

                var serviceProvider = html.ViewContext.HttpContext.RequestServices;
                var metadataProvider = serviceProvider.GetService<IModelMetadataProvider>();
                var viewEngine = serviceProvider.GetService<IViewEngine>();

                var index = 0;
                foreach (var item in collection)
                {
                    var itemType = typeInCollection;
                    if (item != null && !typeInCollectionIsNullableValueType)
                    {
                        itemType = item.GetType();
                    }

                    var metadata = metadataProvider.GetMetadataForType(() => item, itemType);
                    var fieldName = string.Format(CultureInfo.InvariantCulture, "{0}[{1}]", fieldNameBase, index++);

                    var templateBuilder = new TemplateBuilder(
                        viewEngine,
                        html.ViewContext,
                        html.ViewData,
                        metadata,
                        htmlFieldName: fieldName,
                        templateName: null,
                        readOnly: true,
                        additionalViewData: null);

                    var output = templateBuilder.Build();
                    result.Append(output);
                }

                return result.ToString();
            }
            finally
            {
                html.ViewData.TemplateInfo.HtmlFieldPrefix = oldPrefix;
            }
        }

        public static string DecimalTemplate(IHtmlHelper<object> html)
        {
            if (html.ViewData.TemplateInfo.FormattedModelValue == html.ViewData.ModelMetadata.Model)
            {
                html.ViewData.TemplateInfo.FormattedModelValue =
                    string.Format(CultureInfo.CurrentCulture, "{0:0.00}", html.ViewData.ModelMetadata.Model);
            }

            return StringTemplate(html);
        }

        public static string EmailAddressTemplate(IHtmlHelper<object> html)
        {
            return string.Format(CultureInfo.InvariantCulture,
                                 "<a href=\"mailto:{0}\">{1}</a>",
                                 html.Encode(html.ViewData.Model),
                                 html.Encode(html.ViewData.TemplateInfo.FormattedModelValue));
        }

        public static string HiddenInputTemplate(IHtmlHelper<object> html)
        {
            // TODO: add ModelMetadata.HideSurroundingHtml and use here (return string.Empty)
            return StringTemplate(html);
        }

        public static string HtmlTemplate(IHtmlHelper<object> html)
        {
            return html.ViewData.TemplateInfo.FormattedModelValue.ToString();
        }

        public static string ObjectTemplate(IHtmlHelper<object> html)
        {
            var viewData = html.ViewData;
            var templateInfo = viewData.TemplateInfo;
            var modelMetadata = viewData.ModelMetadata;
            var builder = new StringBuilder();

            if (modelMetadata.Model == null)
            {
                return modelMetadata.NullDisplayText;
            }

            if (templateInfo.TemplateDepth > 1)
            {
                // TODO: add ModelMetadata.SimpleDisplayText and use here (return SimpleDisplayText)
                return modelMetadata.Model.ToString();
            }

            var serviceProvider = html.ViewContext.HttpContext.RequestServices;
            var viewEngine = serviceProvider.GetService<IViewEngine>();

            foreach (var propertyMetadata in modelMetadata.Properties.Where(pm => ShouldShow(pm, templateInfo)))
            {
                // TODO: add ModelMetadata.HideSurroundingHtml and use here (skip this block)
                {
                    var label = propertyMetadata.GetDisplayName();
                    if (!string.IsNullOrEmpty(label))
                    {
                        builder.AppendFormat(CultureInfo.InvariantCulture, "<div class=\"display-label\">{0}</div>",
                            label);
                        builder.AppendLine();
                    }

                    builder.Append("<div class=\"display-field\">");
                }

                var templateBuilder = new TemplateBuilder(
                    viewEngine,
                    html.ViewContext,
                    html.ViewData,
                    propertyMetadata,
                    htmlFieldName: propertyMetadata.PropertyName,
                    templateName: null,
                    readOnly: true,
                    additionalViewData: null);

                builder.Append(templateBuilder.Build());

                // TODO: add ModelMetadata.HideSurroundingHtml and use here (skip this block)
                {
                    builder.AppendLine("</div>");
                }
            }

            return builder.ToString();
        }

        private static bool ShouldShow(ModelMetadata metadata, TemplateInfo templateInfo)
        {
            // TODO: add ModelMetadata.ShowForDisplay and include in this calculation (first)
            return
                metadata.ModelType != typeof(EntityState) &&
                !metadata.IsComplexType &&
                !templateInfo.Visited(metadata);
        }

        public static string StringTemplate(IHtmlHelper<object> html)
        {
            return html.Encode(html.ViewData.TemplateInfo.FormattedModelValue);
        }

        public static string UrlTemplate(IHtmlHelper<object> html)
        {
            return string.Format(CultureInfo.InvariantCulture,
                                 "<a href=\"{0}\">{1}</a>",
                                 html.Encode(html.ViewData.Model),
                                 html.Encode(html.ViewData.TemplateInfo.FormattedModelValue));
        }
    }
}
