using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc.Rendering
{
    internal class TemplateBuilder
    {
        private ViewContext _viewContext;
        private ViewDataDictionary _viewData;
        private ModelMetadata _metadata;
        private string _htmlFieldName;
        private string _templateName;
        private bool _readOnly;
        private object _additionalViewData;

        public TemplateBuilder(ViewContext viewContext, ViewDataDictionary viewData, ModelMetadata metadata, string htmlFieldName, string templateName, bool readOnly, object additionalViewData)
        {
            _viewContext = viewContext;
            _viewData = viewData;
            _metadata = metadata;
            _htmlFieldName = htmlFieldName;
            _templateName = templateName;
            _readOnly = readOnly;
            _additionalViewData = additionalViewData;
        }

        public string Build()
        {
            // TODO: Convert Editor into Display if model.IsReadOnly is true? Need to be careful about this because
            // the Model property on the ViewPage/ViewUserControl is get-only, so the type descriptor automatically
            // decorates it with a [ReadOnly] attribute...

            if (metadata.ConvertEmptyStringToNull && String.Empty.Equals(metadata.Model))
            {
                metadata.Model = null;
            }

            object formattedModelValue = metadata.Model;
            if (metadata.Model == null && mode == DataBoundControlMode.ReadOnly)
            {
                formattedModelValue = metadata.NullDisplayText;
            }

            string formatString = mode == DataBoundControlMode.ReadOnly ? metadata.DisplayFormatString : metadata.EditFormatString;
            if (metadata.Model != null && !String.IsNullOrEmpty(formatString))
            {
                formattedModelValue = String.Format(CultureInfo.CurrentCulture, formatString, metadata.Model);
            }

            // Normally this shouldn't happen, unless someone writes their own custom Object templates which
            // don't check to make sure that the object hasn't already been displayed
            object visitedObjectsKey = metadata.Model ?? metadata.RealModelType;
            if (html.ViewDataContainer.ViewData.TemplateInfo.VisitedObjects.Contains(visitedObjectsKey))
            {
                // DDB #224750
                return String.Empty;
            }

            ViewDataDictionary viewData = new ViewDataDictionary(html.ViewDataContainer.ViewData)
            {
                Model = metadata.Model,
                ModelMetadata = metadata,
                TemplateInfo = new TemplateInfo
                {
                    FormattedModelValue = formattedModelValue,
                    HtmlFieldPrefix = html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlFieldName),
                    VisitedObjects = new HashSet<object>(html.ViewContext.ViewData.TemplateInfo.VisitedObjects), // DDB #224750
                }
            };

            if (additionalViewData != null)
            {
                foreach (KeyValuePair<string, object> kvp in TypeHelper.ObjectToDictionary(additionalViewData))
                {
                    viewData[kvp.Key] = kvp.Value;
                }
            }

            viewData.TemplateInfo.VisitedObjects.Add(visitedObjectsKey); // DDB #224750

            return executeTemplate(html, viewData, templateName, mode, GetViewNames, GetDefaultActions);
        }


    }
}