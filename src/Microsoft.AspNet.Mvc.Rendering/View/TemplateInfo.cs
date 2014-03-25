using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class TemplateInfo
    {
        // Keep a collection of visited objects to prevent infinite recursion.
        private HashSet<object> _visitedObjects = new HashSet<object>();

        public TemplateInfo()
        {
            FormattedModelValue = string.Empty;
            HtmlFieldPrefix = string.Empty;
            _visitedObjects = new HashSet<object>();
        }

        public TemplateInfo(TemplateInfo original)
        {
            FormattedModelValue = original.FormattedModelValue;
            HtmlFieldPrefix = original.HtmlFieldPrefix;
            _visitedObjects = new HashSet<object>(original._visitedObjects);
        }

        public object FormattedModelValue { get; set; }

        public string HtmlFieldPrefix { get; set; }

        public int TemplateDepth
        {
            get { return _visitedObjects.Count; }
        }

        public bool AddVisited(object value)
        {
            return _visitedObjects.Add(value);
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
            return _visitedObjects.Contains(metadata.Model ?? metadata.ModelType);
        }
    }
}
