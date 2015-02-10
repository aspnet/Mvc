// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Framework.Logging;

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
        public IList<ModeAttributes<TMode>> PartialMatches { get; } = new List<ModeAttributes<TMode>>();

        /// <summary>
        /// Modes that had all attributes present.
        /// </summary>
        public IList<ModeAttributes<TMode>> FullMatches { get; } = new List<ModeAttributes<TMode>>();

        /// <summary>
        /// Attributes that are present in at least one mode in <see cref="PartialMatches"/>, but in no modes in
        /// <see cref="FullMatches"/>.
        /// </summary>
        public IList<string> PartiallyMatchedAttributes { get; } = new List<string>();

        /// <summary>
        /// Logs the details of the <see cref="ModeMatchResult{TMode}"/>.
        /// </summary>
        /// <typeparam name="TTagHelper"></typeparam>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="tagHelper">The <see cref="ITagHelper"/>.</param>
        /// <param name="uniqueId">The value of <see cref="TagHelperContext.UniqueId"/>.</param>
        public void LogDetails<TTagHelper>([NotNull] ILogger logger, [NotNull] TTagHelper tagHelper, string uniqueId)
        {
            if (logger.IsEnabled(LogLevel.Warning) && PartiallyMatchedAttributes.Any())
            {
                var unmatchedPartials = PartialMatches.Where(partial =>
                    partial.Attributes.Any(attribute =>
                        PartiallyMatchedAttributes.Contains(attribute, StringComparer.OrdinalIgnoreCase)));
                logger.WriteWarning(new PartialModeMatchLoggerStructure<TMode>(uniqueId, PartialMatches));
            }

            if (!FullMatches.Any() && logger.IsEnabled(LogLevel.Verbose))
            {
                logger.WriteVerbose("Skipping processing for {0} {1}", tagHelper.GetType().GetTypeInfo().Name, uniqueId);
            }
        }
    }

    /// <summary>
    /// Static creation methods for <see cref="ModeAttributes{TMode}"/>.
    /// </summary>
    public static class ModeAttributes
    {
        /// <summary>
        /// Creates an <see cref="ModeAttributes{TMode}"/>/
        /// </summary>
        /// <typeparam name="TMode">The type representing the <see cref="ITagHelper"/>'s mode.</typeparam>
        /// <param name="mode">The <see cref="ITagHelper"/>'s mode.</param>
        /// <param name="attributes">The attributes.</param>
        /// <returns></returns>
        public static ModeAttributes<TMode> Create<TMode>(TMode mode, IEnumerable<string> attributes)
        {
            return new ModeAttributes<TMode>
            {
                Mode = mode,
                Attributes = attributes
            };
        }
    }

    /// <summary>
    /// A mapping of a <see cref="ITagHelper"/> mode to attributes.
    /// </summary>
    /// <typeparam name="TMode">The type representing the <see cref="ITagHelper"/>'s mode.</typeparam>
    public class ModeAttributes<TMode>
    {
        /// <summary>
        /// The <see cref="ITagHelper"/>'s mode.
        /// </summary>
        public TMode Mode { get; set; }

        /// <summary>
        /// The attributes. The meaning of the attributes is not defined and is intead dependent on the context of this
        /// type being used, e.g. the mode's required attributes, attributes missing for this mode, etc.
        /// </summary>
        public IEnumerable<string> Attributes { get; set; }
    }
}