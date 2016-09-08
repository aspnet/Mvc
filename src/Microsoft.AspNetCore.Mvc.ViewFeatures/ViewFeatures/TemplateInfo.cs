// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    public class TemplateInfo
    {
        private static readonly char[] _prefixDividers = new[] { '.', '[', ']' };

        // Keep a collection of visited objects to prevent infinite recursion.
        private readonly HashSet<object> _visitedObjects;

        private string _htmlFieldPrefix;
        private StringValuesTutu _htmlFieldPrefixValues;
        private object _formattedModelValue;

        public TemplateInfo()
        {
            _formattedModelValue = string.Empty;
            _htmlFieldPrefix = string.Empty;
            _htmlFieldPrefixValues = StringValuesTutu.Empty;
            _visitedObjects = new HashSet<object>();
        }

        public TemplateInfo(TemplateInfo original)
        {
            FormattedModelValue = original.FormattedModelValue;
            _htmlFieldPrefix = original._htmlFieldPrefix;
            _htmlFieldPrefixValues = original._htmlFieldPrefixValues;
            _visitedObjects = new HashSet<object>(original._visitedObjects);
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
            get
            {
                if (_htmlFieldPrefix == null)
                {
                    _htmlFieldPrefix = _htmlFieldPrefixValues.ToString();
                }

                return _htmlFieldPrefix;
            }
            set
            {
                _htmlFieldPrefix = value ?? string.Empty;
                _htmlFieldPrefixValues = _htmlFieldPrefix;
            }
        }

        public StringValuesTutu HtmlFieldPrefixValues
        {
            get
            {
                return _htmlFieldPrefixValues;
            }
            set
            {
                _htmlFieldPrefix = null;
                _htmlFieldPrefixValues = value;
            }
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
            return GetFullHtmlFieldName(new StringValuesTutu(partialFieldName)).ToString();
        }

        public StringValuesTutu GetFullHtmlFieldName(StringValuesTutu partialFieldName)
        {
            if (StringValuesTutu.IsNullOrEmpty(partialFieldName))
            {
                return HtmlFieldPrefixValues;
            }

            if (StringValuesTutu.IsNullOrEmpty(HtmlFieldPrefixValues))
            {
                return partialFieldName;
            }

            if ('[' != partialFieldName[0][0])
            {
                return StringValuesTutu.Concat(HtmlFieldPrefixValues, ".", partialFieldName);
            }

            return StringValuesTutu.Concat(HtmlFieldPrefixValues, partialFieldName);
        }

        public bool Visited(ModelExplorer modelExplorer)
        {
            return _visitedObjects.Contains(modelExplorer.Model ?? modelExplorer.Metadata.ModelType);
        }
    }
}
