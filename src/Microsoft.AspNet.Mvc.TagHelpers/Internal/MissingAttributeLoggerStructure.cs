// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.TagHelpers.Internal
{
    /// <summary>
    /// An <see cref="ILoggerStructure"/> for log messages regarding <see cref="ITagHelper"/> instances that opt out of
    /// processing due to missing required attributes.
    /// </summary>
    public class MissingAttributeLoggerStructure : ILoggerStructure
    {
        private readonly string _uniqueId;
        private readonly string _viewPath;
        private readonly IEnumerable<KeyValuePair<string, object>> _values;
        
        // Internal for unit testing
        internal IEnumerable<string> MissingAttributes { get; }

        /// <summary>
        /// Creates a new <see cref="MissingAttributeLoggerStructure"/>.
        /// </summary>
        /// <param name="uniqueId">The unique ID of the HTML element this message applies to.</param>
        /// <param name="viewPath">The path to the view.</param>
        /// <param name="missingAttributes">The missing required attributes.</param>
        public MissingAttributeLoggerStructure(string uniqueId, string viewPath, IEnumerable<string> missingAttributes)
        {
            _uniqueId = uniqueId;
            _viewPath = viewPath;
            MissingAttributes = missingAttributes;
            _values = new Dictionary<string, object>
            {
                ["UniqueId"] = _uniqueId,
                ["ViewPath"] = _viewPath,
                ["MissingAttributes"] = MissingAttributes
            };
        }

        /// <summary>
        /// The log message.
        /// </summary>
        public string Message
        {
            get
            {
                return "Tag Helper has one or more missing required attributes.";
            }
        }

        /// <summary>
        /// Gets the values associated with this structured log message.
        /// </summary>
        /// <returns>The values.</returns>
        public IEnumerable<KeyValuePair<string, object>> GetValues()
        {
            return _values;
        }

        /// <summary>
        /// Generates a human readable string for this structured log message.
        /// </summary>
        /// <returns>The message.</returns>
        public string Format()
        {
            return string.Format("Tag Helper with ID {0} in view '{1}' is missing attributes: {2}",
                _uniqueId,
                _viewPath,
                string.Join(",", MissingAttributes));
        }
    }
}