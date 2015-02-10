// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace Microsoft.AspNet.Mvc.TagHelpers.Internal
{
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
    /// A mapping of a <see cref="ITagHelper"/> mode to its missing and present attributes.
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