// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    public class TemplateInfo
    {
        // Remember previous GetFullHtmlFieldName() calculation to avoid repeated concatenations. Frequently see
        // clusters of elements (names and values for display and labels, inputs and validations for editing).
        // Maintained per-request because the clusters almost never span views and to keep TemplateInfo size down.
        private readonly LastFullHtmlFieldName _lastFullHtmlFieldName;

        // Keep a collection of visited objects to prevent infinite recursion.
        private readonly HashSet<object> _visitedObjects;

        private object _formattedModelValue;
        private string _htmlFieldPrefix;

        public TemplateInfo()
        {
            _lastFullHtmlFieldName = new LastFullHtmlFieldName();
            _visitedObjects = new HashSet<object>();
            _formattedModelValue = string.Empty;
            _htmlFieldPrefix = string.Empty;
        }

        public TemplateInfo(TemplateInfo original)
        {
            _lastFullHtmlFieldName = original._lastFullHtmlFieldName;
            _visitedObjects = new HashSet<object>(original._visitedObjects);
            FormattedModelValue = original.FormattedModelValue;
            HtmlFieldPrefix = original.HtmlFieldPrefix;
        }

        /// <summary>
        /// Gets or sets the formatted model value.
        /// </summary>
        /// <remarks>
        /// Will never return <c>null</c> to avoid problems when using HTML helpers within a template.  Otherwise the
        /// helpers could find elements in the `ViewDataDictionary`, not the intended Model properties.
        /// </remarks>
        /// <value>The formatted model value.</value>
        public object FormattedModelValue
        {
            get { return _formattedModelValue; }
            set { _formattedModelValue = value ?? string.Empty; }
        }

        /// <summary>
        /// Gets or sets the HTML field prefix.
        /// </summary>
        /// <remarks>
        /// Will never return <c>null</c> for consistency with <see cref="FormattedModelValue"/>.
        /// </remarks>
        /// <value>The HTML field prefix.</value>
        public string HtmlFieldPrefix
        {
            get { return _htmlFieldPrefix; }
            set { _htmlFieldPrefix = value ?? string.Empty; }
        }

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
            if (string.IsNullOrEmpty(partialFieldName))
            {
                return HtmlFieldPrefix;
            }

            if (string.IsNullOrEmpty(HtmlFieldPrefix))
            {
                return partialFieldName;
            }

            if (string.Equals(_lastFullHtmlFieldName.HtmlFieldPrefix, HtmlFieldPrefix, StringComparison.Ordinal) &&
                string.Equals(_lastFullHtmlFieldName.PartialFieldName, partialFieldName, StringComparison.Ordinal))
            {
                return _lastFullHtmlFieldName.FullHtmlFieldName;
            }

            _lastFullHtmlFieldName.HtmlFieldPrefix = HtmlFieldPrefix;
            _lastFullHtmlFieldName.PartialFieldName = partialFieldName;
            if (partialFieldName.StartsWith("[", StringComparison.Ordinal))
            {
                // The partialFieldName might represent an indexer access, in which case combining
                // with a 'dot' would be invalid.
                _lastFullHtmlFieldName.FullHtmlFieldName = HtmlFieldPrefix + partialFieldName;
            }
            else
            {
                _lastFullHtmlFieldName.FullHtmlFieldName = HtmlFieldPrefix + "." + partialFieldName;
            }

            return _lastFullHtmlFieldName.FullHtmlFieldName;
        }

        public bool Visited(ModelExplorer modelExplorer)
        {
            return _visitedObjects.Contains(modelExplorer.Model ?? modelExplorer.Metadata.ModelType);
        }

        private class LastFullHtmlFieldName
        {
            public string HtmlFieldPrefix { get; set; }

            public string PartialFieldName { get; set; }

            public string FullHtmlFieldName { get; set; }
        }
    }
}
