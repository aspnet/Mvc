// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Mvc
{
    public class TemplateInfo
    {
        private string _htmlFieldPrefix;
        private object _formattedModelValue;
        private HashSet<object> _visitedObjects;

        public object FormattedModelValue
        {
            get { return _formattedModelValue ?? String.Empty; }
            set { _formattedModelValue = value; }
        }

        public string HtmlFieldPrefix
        {
            get { return _htmlFieldPrefix ?? String.Empty; }
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

        public string GetFullHtmlFieldId(string partialFieldName)
        {
            return HtmlHelper.GenerateIdFromName(GetFullHtmlFieldName(partialFieldName));
        }

        public string GetFullHtmlFieldName(string partialFieldName)
        {
            if (partialFieldName != null && partialFieldName.StartsWith("[", StringComparison.Ordinal))
            {
                // See Codeplex #544 - the partialFieldName might represent an indexer access, in which case combining
                // with a 'dot' would be invalid.
                return HtmlFieldPrefix + partialFieldName;
            }
            else
            {
                // This uses "combine and trim" because either or both of these values might be empty
                return (HtmlFieldPrefix + "." + (partialFieldName ?? String.Empty)).Trim('.');
            }
        }

        public bool Visited(ModelMetadata metadata)
        {
            return VisitedObjects.Contains(metadata.Model ?? metadata.ModelType);
        }
    }
}
