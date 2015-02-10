// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace Microsoft.AspNet.Mvc.TagHelpers.Internal
{
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
    /// A mapping of a <see cref="ITagHelper"/> mode to its required attributes.
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
}