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
        public IList<ModeMatchAttributes<TMode>> PartialMatches { get; } = new List<ModeMatchAttributes<TMode>>();

        /// <summary>
        /// Modes that had all attributes present.
        /// </summary>
        public IList<ModeMatchAttributes<TMode>> FullMatches { get; } = new List<ModeMatchAttributes<TMode>>();

        /// <summary>
        /// Modes that are only in <see cref="PartialMatches"/> and not in <see cref="FullMatches"/>.
        /// </summary>
        public IEnumerable<ModeMatchAttributes<TMode>> PartialOnlyMatches { get; set; } = new List<ModeMatchAttributes<TMode>>();

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
            if (logger.IsEnabled(LogLevel.Warning) && PartialOnlyMatches.Any())
            {
                logger.WriteWarning(new PartialModeMatchLoggerStructure<TMode>(uniqueId, PartialOnlyMatches));
            }

            if (logger.IsEnabled(LogLevel.Verbose) && !FullMatches.Any())
            {
                logger.WriteVerbose("Skipping processing for {0} {1}",
                    tagHelper.GetType().GetTypeInfo().Name, uniqueId);
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

        public IEnumerable<string> Attributes { get; set; }
    }

    /// <summary>
    /// Static creation methods for <see cref="ModeMatchAttributes{TMode}"/>.
    /// </summary>
    public static class ModeMatchAttributes
    {
        /// <summary>
        /// Creates an <see cref="ModeMatchAttributes{TMode}"/>/
        /// </summary>
        public static ModeMatchAttributes<TMode> Create<TMode>(
           TMode mode,
           IEnumerable<string> presentAttributes)
        {
            return Create(mode, presentAttributes, missingAttributes: null);
        }

        /// <summary>
        /// Creates an <see cref="ModeMatchAttributes{TMode}"/>/
        /// </summary>
        public static ModeMatchAttributes<TMode> Create<TMode>(
            TMode mode,
            IEnumerable<string> presentAttributes,
            IEnumerable<string> missingAttributes)
        {
            return new ModeMatchAttributes<TMode>
            {
                Mode = mode,
                PresentAttributes = presentAttributes,
                MissingAttributes = missingAttributes
            };
        }
    }

    /// <summary>
    /// A mapping of a <see cref="ITagHelper"/> mode to attributes.
    /// </summary>
    /// <typeparam name="TMode">The type representing the <see cref="ITagHelper"/>'s mode.</typeparam>
    public class ModeMatchAttributes<TMode>
    {
        /// <summary>
        /// The <see cref="ITagHelper"/>'s mode.
        /// </summary>
        public TMode Mode { get; set; }

        public IEnumerable<string> PresentAttributes { get; set; }

        public IEnumerable<string> MissingAttributes { get; set; }
    }
}