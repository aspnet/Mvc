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
            if (_metadata.ConvertEmptyStringToNull && string.Empty.Equals(_metadata.Model))
            {
                _metadata.Model = null;
            }

            var formattedModelValue = _metadata.Model;
            if (_metadata.Model == null && _readOnly)
            {
                formattedModelValue = _metadata.NullDisplayText;
            }

            var formatString = _readOnly ? _metadata.DisplayFormatString : _metadata.EditFormatString;

            if (_metadata.Model != null && !string.IsNullOrEmpty(formatString))
            {
                formattedModelValue = string.Format(CultureInfo.CurrentCulture, formatString, _metadata.Model);
            }

            // Normally this shouldn't happen, unless someone writes their own custom Object templates which
            // don't check to make sure that the object hasn't already been displayed
            object visitedObjectsKey = _metadata.Model ?? _metadata.GetRealModelType();
            if (_viewData.TemplateInfo.Visited(visitedObjectsKey))
            {
                return string.Empty;
            }

            var viewData = new ViewDataDictionary(_viewData)
            {
                Model = _metadata.Model,
                ModelMetadata = _metadata,
                TemplateInfo = new TemplateInfo(_viewData.TemplateInfo)
                {
                    FormattedModelValue = formattedModelValue,
                    HtmlFieldPrefix = _viewData.TemplateInfo.GetFullHtmlFieldName(_htmlFieldName),
                }
            };

            if (_additionalViewData != null)
            {
                foreach (KeyValuePair<string, object> kvp in HtmlHelper.AnonymousObjectToDictionary(_additionalViewData))
                {
                    viewData[kvp.Key] = kvp.Value;
                }
            }

            viewData.TemplateInfo.AddVisited(visitedObjectsKey);

            var templateRenderer = new TemplateRenderer(_viewContext, viewData, _templateName, _readOnly);

            return templateRenderer.Render();
        }


    }
}
