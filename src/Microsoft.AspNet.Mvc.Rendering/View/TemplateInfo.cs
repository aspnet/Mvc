using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class TemplateInfo
    {
        private string _htmlFieldPrefix;
        private object _formattedModelValue;
        private HashSet<object> _visitedObjects;

        public object FormattedModelValue
        {
            get { return _formattedModelValue ?? string.Empty; }
            set { _formattedModelValue = value; }
        }

        public string HtmlFieldPrefix
        {
            get { return _htmlFieldPrefix ?? string.Empty; }
            set { _htmlFieldPrefix = value; }
        }

        public int TemplateDepth
        {
            get { return VisitedObjects.Count; }
        }

        // DDB #224750 - Keep a collection of visited objects to prevent infinite recursion
        internal HashSet<object> VisitedObjects
        {
            get
            {
                if (_visitedObjects == null)
                {
                    _visitedObjects = new HashSet<object>();
                }
                return _visitedObjects;
            }
            set { _visitedObjects = value; }
        }

        public string GetFullHtmlFieldName(string partialFieldName)
        {
            if (partialFieldName != null && partialFieldName.StartsWith("[", StringComparison.Ordinal))
            {
                // The partialFieldName might represent an indexer access, in which case combining
                // with a 'dot' would be invalid.
                return HtmlFieldPrefix + partialFieldName;
            }
            else
            {
                // This uses "combine and trim" because either or both of these values might be empty.
                return (HtmlFieldPrefix + "." + (partialFieldName ?? string.Empty)).Trim('.');
            }
        }

        public bool Visited(ModelMetadata metadata)
        {
            return VisitedObjects.Contains(metadata.Model ?? metadata.ModelType);
        }
    }
}
