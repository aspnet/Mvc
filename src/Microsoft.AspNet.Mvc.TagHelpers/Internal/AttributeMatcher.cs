// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Razor.TagHelpers;

namespace Microsoft.AspNet.Mvc.TagHelpers.Internal
{
    /// <summary>
    /// Methods for determining how an <see cref="ITagHelper"/> should run based on the attributes that were specified.
    /// </summary>
    public static class AttributeMatcher
    {
        /// <summary>
        /// Determines the most effective mode a <see cref="ITagHelper" /> can run in based on which modes have
        /// all their required attributes present.
        /// </summary>
        /// <typeparam name="TMode">The type representing the <see cref="ITagHelper" />'s modes.</typeparam>
        /// <param name="context">The <see cref="TagHelperContext"/>.</param>
        /// <param name="modeInfos">The modes and their required attributes.</param>
        /// <param name="compare">A comparer delegate.</param>
        /// <param name="result">The resulting most effective mode.</param>
        /// <returns><c>true</c> if a result was found, otherwise <c>false</c>.</returns>
        public static bool  TryDetermineMode<TMode>(
            TagHelperContext context,
            IReadOnlyList<ModeAttributes<TMode>> modeInfos,
            Func<TMode, TMode, int> compare,
            out TMode result)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (modeInfos == null)
            {
                throw new ArgumentNullException(nameof(modeInfos));
            }

            var foundResult = false;
            result = default(TMode);

            // Perf: Avoid allocating enumerator
            for (var i = 0; i < modeInfos.Count; i++)
            {
                var modeInfo = modeInfos[i];
                if (!HasMissingAttributes(context, modeInfo.Attributes) &&
                    compare(result, modeInfo.Mode) <= 0)
                {
                    foundResult = true;
                    result = modeInfo.Mode;
                }
            }

            return foundResult;
        }

        private static bool HasMissingAttributes(TagHelperContext context, string[] requiredAttributes)
        {
            if (context.AllAttributes.Count < requiredAttributes.Length)
            {
                // If there are fewer attributes present than required, one or more of them must be missing.
                return true;
            }

            // Check for all attribute values
            // Perf: Avoid allocating enumerator
            for (var i = 0; i < requiredAttributes.Length; i++)
            {
                IReadOnlyTagHelperAttribute attribute;
                if (!context.AllAttributes.TryGetAttribute(requiredAttributes[i], out attribute))
                {
                    // Missing attribute.
                    return true;
                }

                var valueAsString = attribute.Value as string;
                if (valueAsString != null && string.IsNullOrEmpty(valueAsString))
                {
                    // Treat attributes with empty values as missing.
                    return true;
                }
            }

            return false;
        }
    }
}