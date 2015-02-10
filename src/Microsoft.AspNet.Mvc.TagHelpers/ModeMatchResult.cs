// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
    /// <summary>
    /// Result of determining the mode an <see cref="ITagHelper"/> will run in.
    /// </summary>
    /// <typeparam name="TMode">The type representing the <see cref="ITagHelper"/>'s mode.</typeparam>
    public class ModeMatchResult<TMode>
    {
        //private static readonly ModeMatchResult<TMode> _unmatched = new ModeMatchResult<TMode>(matched: false);

        ///// <summary>
        ///// Creates a new <see cref="ModeMatchResult{TMode}"/>.
        ///// </summary>
        ///// <param name="mode">The <see cref="ITagHelper"/>'s mode.</param>
        ///// <param name="matched">Whether this mode matched or not.</param>
        //public ModeMatchResult(TMode mode, bool matched)
        //{
        //    Mode = mode;
        //    Matched = matched;
        //}

        //private ModeMatchResult(bool matched)
        //{
        //    Matched = matched;
        //}

        public IList<ModeInfo<TMode>> PartialMatches { get; } = new List<ModeInfo<TMode>>();

        public IList<ModeInfo<TMode>> FullMatches { get; } = new List<ModeInfo<TMode>>();

        ///// <summary>
        ///// The <see cref="ITagHelper"/>'s mode this match result is for.
        ///// </summary>
        //public TMode Mode { get; private set; }

        ///// <summary>
        ///// Whether this mode matched or not.
        ///// </summary>
        //public bool Matched { get; private set; }

        ///// <summary>
        ///// An unmatched result.
        ///// </summary>
        //public static ModeMatchResult<TMode> Unmatched
        //{
        //    get { return _unmatched; }
        //}
    }

    public static class ModeInfo
    {
        public static ModeInfo<TMode> Create<TMode>(TMode mode, IEnumerable<string> attributes)
        {
            return new ModeInfo<TMode>
            {
                Mode = mode,
                Attributes = attributes
            };
        }
    }

    public class ModeInfo<TMode>
    {
        public TMode Mode { get; set; }

        public IEnumerable<string> Attributes { get; set; }
    }
}