// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class TemplateInfo
    {
        private object _formattedModelValue = string.Empty;
        private string _htmlFieldPrefix = string.Empty;
        private HashSet<object> _visitedObjects = new HashSet<object>();

        public object FormattedModelValue
        {
            get { return _formattedModelValue; }
            set { _formattedModelValue = value; }
        }

        public string HtmlFieldPrefix
        {
            get { return _htmlFieldPrefix; }
            set { _htmlFieldPrefix = value; }
        }

        public int TemplateDepth
        {
            get { return VisitedObjects.Count; }
        }

        // Keep a collection of visited objects to prevent infinite recursion.
        internal HashSet<object> VisitedObjects
        {
            get { return _visitedObjects; }
            set { _visitedObjects = value; }
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
                return (HtmlFieldPrefix + "." + (partialFieldName ?? string.Empty)).Trim('.');
            }
        }
    }
}
