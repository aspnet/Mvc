// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    /// <summary>
    /// Static helper methods for <see cref="ModeMatchResult{TMode}"/>.
    /// </summary>
    public static class ModeMatchResult
    {
        /// <summary>
        /// Creates a new <see cref="ModeMatchResult{TMode}"/>.
        /// </summary>
        /// <typeparam name="TMode">The type representing the <see cref="ITagHelper"/>'s mode.</typeparam>
        /// <param name="mode">The <see cref="ITagHelper"/>'s mode.</param>
        /// <returns>The match result.</returns>
        public static ModeMatchResult<TMode> Matched<TMode>(TMode mode)
        {
            return new ModeMatchResult<TMode>(mode, true);
        }
    }

    /// <summary>
    /// Result of determining the mode an <see cref="ITagHelper"/> will run in.
    /// </summary>
    /// <typeparam name="TMode">The type representing the <see cref="ITagHelper"/>'s mode.</typeparam>
    public class ModeMatchResult<TMode>
    {
        private static readonly ModeMatchResult<TMode> _unmatched = new ModeMatchResult<TMode>(false);

        /// <summary>
        /// Creates a new <see cref="ModeMatchResult{TMode}"/>.
        /// </summary>
        /// <param name="mode">The <see cref="ITagHelper"/>'s mode.</param>
        /// <param name="matched">Whether this mode matched or not.</param>
        public ModeMatchResult(TMode mode, bool matched)
        {
            Mode = mode;
            Matched = matched;
        }

        private ModeMatchResult(bool matched)
        {
            Matched = matched;
        }

        /// <summary>
        /// The <see cref="ITagHelper"/>'s mode this match result is for.
        /// </summary>
        public TMode Mode { get; private set; }

        /// <summary>
        /// Whether this mode matched or not.
        /// </summary>
        public bool Matched { get; private set; }

        /// <summary>
        /// An unmatched result.
        /// </summary>
        public static ModeMatchResult<TMode> Unmatched
        {
            get { return _unmatched; }
        }
    }
}