// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc.Logging;
using Microsoft.AspNet.Mvc.TagHelpers.Logging;
using Microsoft.AspNet.Razor.TagHelpers;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Mvc.TagHelpers.Internal
{
    /// <summary>
    /// Result of determining the mode an <see cref="ITagHelper"/> will run in.
    /// </summary>
    /// <typeparam name="TMode">The type representing the <see cref="ITagHelper"/>'s mode.</typeparam>
    public class ModeMatchResult<TMode>
    {
        /// <summary>
        /// Modes that were missing attributes but had at least one attribute present.
        /// </summary>
        public IList<ModeMatchAttributes<TMode>> PartialMatches { get; } = new List<ModeMatchAttributes<TMode>>();

        /// <summary>
        /// Modes that had all attributes present.
        /// </summary>
        public IList<ModeMatchAttributes<TMode>> FullMatches { get; } = new List<ModeMatchAttributes<TMode>>();

        /// <summary>
        /// Attributes that are present in at least one mode in <see cref="PartialMatches"/>, but in no modes in
        /// <see cref="FullMatches"/>.
        /// </summary>
        public IList<string> PartiallyMatchedAttributes { get; } = new List<string>();

        /// <summary>
        /// Logs the details of the <see cref="ModeMatchResult{TMode}"/>.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="tagHelper">The <see cref="ITagHelper"/>.</param>
        /// <param name="uniqueId">The value of <see cref="TagHelperContext.UniqueId"/>.</param>
        /// <param name="viewPath">The path to the view the <see cref="ITagHelper"/> is on.</param>
        public void LogDetails<TTagHelper>(
            ILogger logger,
            TTagHelper tagHelper,
            string uniqueId,
            string viewPath)
            where TTagHelper : ITagHelper
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (tagHelper == null)
            {
                throw new ArgumentNullException(nameof(tagHelper));
            }

            if (logger.IsEnabled(LogLevel.Warning) && PartiallyMatchedAttributes.Any())
            {
                // Build the list of partial matches that contain attributes not appearing in at least one full match
                var partialOnlyMatches = PartialMatches.Where(
                    match => match.PresentAttributes.Any(
                        attribute => PartiallyMatchedAttributes.Contains(
                            attribute, StringComparer.OrdinalIgnoreCase)));
                logger.TagHelperPartialMatches<TMode>(uniqueId, viewPath, partialOnlyMatches);
            }

            if (logger.IsEnabled(LogLevel.Verbose) && !FullMatches.Any())
            {
                logger.TagHelperSkippingProcessing(
                    tagHelper,
                    uniqueId);
            }
        }
    }
}
