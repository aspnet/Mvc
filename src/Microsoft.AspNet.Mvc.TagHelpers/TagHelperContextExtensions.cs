// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.Logging;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.TagHelpers;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
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
            ILogger logger = null)
        {
            var attributes = GetPresentMissingAttributes(context, requiredAttributes);
            var present = attributes.Item1;
            var missing = attributes.Item2;

            if (missing.Any())
            {
                if (present.Any() && logger != null && logger.IsEnabled(LogLevel.Warning))
                {
                    // At least 1 attribute was present indicating the user intended to use the tag helper,
                    // but at least 1 was missing too, so log a warning with the details.
                    logger.WriteWarning(new MissingAttributeLoggerStructure(context.UniqueId, missing));
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
        /// <typeparam name="TSet">The type representing which attributes are required for which mode.</typeparam>
        /// <param name="context">The <see cref="TagHelperContext"/>.</param>
        /// <param name="modeRequiredAttributes">The modes and their required attributes.</param>
        /// <param name="logger">An <see cref="ILogger"/> to log messages to.</param>
        /// <returns>The <see cref="ModeMatchResult{TMode}"/>.</returns>
        public static ModeMatchResult<TMode> DetermineMode<TMode, TSet>(
            this TagHelperContext context,
            IEnumerable<Tuple<TMode, TSet>> modeRequiredAttributes,
            ILogger logger = null)
            where TSet : IEnumerable<string>
        {
            List<Tuple<TMode, IEnumerable<string>>> modeCandidates = null;

            foreach (var set in modeRequiredAttributes)
            {
                var attributes = GetPresentMissingAttributes(context, set.Item2);
                var present = attributes.Item1;
                var missing = attributes.Item2;

                if (!missing.Any())
                {
                    return ModeMatchResult.Matched(set.Item1);
                }

                if (missing.Any() && present.Any())
                {
                    // The set had some present attributes but others missing so capture details of those missing to
                    // log later on if no match is found
                    modeCandidates = modeCandidates ?? new List<Tuple<TMode, IEnumerable<string>>>();
                    modeCandidates.Add(Tuple.Create(set.Item1, missing));
                }
            }

            // If a partial was match found, log a warning
            if (modeCandidates != null && logger != null && logger.IsEnabled(LogLevel.Warning))
            {
                logger.WriteWarning(new PartialModeMatchLoggerStructure(context.UniqueId,
                    modeCandidates.Select(candidate =>
                        Tuple.Create(candidate.Item1.ToString(), candidate.Item2.ToArray()))));
            }

            return ModeMatchResult<TMode>.Unmatched;
        }

        private static Tuple<IEnumerable<string>, IEnumerable<string>> GetPresentMissingAttributes(
            [NotNull]this TagHelperContext context,
            [NotNull]IEnumerable<string> requiredAttributes)
        {
            // Check for all attribute values
            var presentAttrNames = new List<string>();
            var missingAttrNames = new List<string>();

            foreach (var attr in requiredAttributes)
            {
                if (!context.AllAttributes.ContainsKey(attr)
                    || context.AllAttributes[attr] == null
                    || string.IsNullOrWhiteSpace(context.AllAttributes[attr] as string))
                {
                    // Missing attribute!
                    missingAttrNames.Add(attr);
                }
                else
                {
                    presentAttrNames.Add(attr);
                }
            }

            return Tuple.Create((IEnumerable<string>)presentAttrNames, (IEnumerable<string>)missingAttrNames);
        }
    }
}