// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc.TagHelpers.Internal;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
    /// <summary>
    /// Utility related extensions for <see cref="TagHelperContext"/>.
    /// </summary>
    public static class TagHelperContextExtensions
    {
        /// <summary>
        /// Determines whether a <see cref="ITagHelper" />'s required attributes are present, non null, non empty, and
        /// non whitepsace.
        /// </summary>
        /// <param name="context">The <see cref="TagHelperContext"/>.</param>
        /// <param name="requiredAttributes">The attributes the <see cref="ITagHelper" /> requires in order to run.</param>
        /// <param name="logger">An optional <see cref="ILogger"/> to log warning details to.</param>
        /// <returns>A <see cref="bool"/> indicating whether the <see cref="ITagHelper" /> should run.</returns> 
        public static bool AllRequiredAttributesArePresent(
            [NotNull]this TagHelperContext context,
            [NotNull]IEnumerable<string> requiredAttributes,
            ILogger logger)
        {
            var attributes = GetPresentMissingAttributes(context, requiredAttributes);

            if (attributes.Missing.Any())
            {
                if (attributes.Present.Any() && logger != null && logger.IsEnabled(LogLevel.Warning))
                {
                    // At least 1 attribute was present indicating the user intended to use the tag helper,
                    // but at least 1 was missing too, so log a warning with the details.
                    logger.WriteWarning(new MissingAttributeLoggerStructure(context.UniqueId, attributes.Missing));
                }

                return false;
            }

            // All required attributes present
            return true;
        }

        /// <summary>
        /// Determines the mode a <see cref="ITagHelper" /> should run in based on which mode has all its required
        /// attributes present, non null, non empty, and non whitepsace.
        /// </summary>
        /// <typeparam name="TMode">The type representing the <see cref="ITagHelper" />'s modes.</typeparam>
        /// <param name="context">The <see cref="TagHelperContext"/>.</param>
        /// <param name="modeInfos">The modes and their required attributes.</param>
        /// <returns>The <see cref="ModeMatchResult{TMode}"/>.</returns>
        public static ModeMatchResult<TMode> DetermineMode<TMode>(
            [NotNull] this TagHelperContext context,
            [NotNull] IEnumerable<ModeAttributes<TMode>> modeInfos)
        {
            // true == full match, false == partial match
            var matchedAttributes = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            var result = new ModeMatchResult<TMode>();

            foreach (var modeInfo in modeInfos)
            {
                var modeAttributes = GetPresentMissingAttributes(context, modeInfo.Attributes);

                if (modeAttributes.Present.Any())
                {
                    if (!modeAttributes.Missing.Any())
                    {
                        // A complete match, mark the attribute as fully matched
                        foreach (var attribute in modeAttributes.Present)
                        {
                            matchedAttributes[attribute] = true;
                        }

                        result.FullMatches.Add(modeInfo);
                    }
                    else
                    {
                        // A partial match, mark the attribute as partially matched if not already fully matched
                        foreach (var attribute in modeAttributes.Present)
                        {
                            bool attributeMatch;
                            if (!matchedAttributes.TryGetValue(attribute, out attributeMatch))
                            {
                                matchedAttributes[attribute] = false;
                            }
                        }

                        result.PartialMatches.Add(ModeAttributes.Create(modeInfo.Mode, modeAttributes.Missing));
                    }
                }
            }

            foreach (var attribute in matchedAttributes.Keys)
            {
                if (!matchedAttributes[attribute])
                {
                    result.PartiallyMatchedAttributes.Add(attribute);
                }
            }

            return result;
        }

        private static PresentMissingAttributes GetPresentMissingAttributes(
            TagHelperContext context,
            IEnumerable<string> requiredAttributes)
        {
            // Check for all attribute values
            var presentAttributes = new List<string>();
            var missingAttributes = new List<string>();

            foreach (var attr in requiredAttributes)
            {
                if (!context.AllAttributes.ContainsKey(attr) ||
                    context.AllAttributes[attr] == null ||
                    string.IsNullOrWhiteSpace(context.AllAttributes[attr] as string))
                {
                    // Missing attribute!
                    missingAttributes.Add(attr);
                }
                else
                {
                    presentAttributes.Add(attr);
                }
            }

            return new PresentMissingAttributes { Present = presentAttributes, Missing = missingAttributes };
        }

        private class PresentMissingAttributes
        {
            public IEnumerable<string> Present { get; set; }

            public IEnumerable<string> Missing { get; set; }
        }
    }
}