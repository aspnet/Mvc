// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc;

namespace Microsoft.Framework.FileSystemGlobbing
{
    /// <summary>
    /// Extension methods for <see cref="Matcher"/>.
    /// </summary>
    public static class MatcherExtensions
    {
        /// <summary>
        /// Adds include and exclude patterns.
        /// Patterns starting with a "!" will be added as excludes. All other patterns will be added as includes.
        /// Leading forward slashes (/) and tildes (~) are trimmed from the pattern before being added.
        /// </summary>
        /// <param name="matcher">The <see cref="Matcher"/>.</param>
        /// <param name="patterns">The set of globbing patterns.</param>
        public static void AddPatterns([NotNull]this Matcher matcher, [NotNull]IEnumerable<string> patterns)
        {
            foreach (var pattern in patterns)
            {
                if (pattern.StartsWith("!", StringComparison.OrdinalIgnoreCase))
                {
                    matcher.AddExclude(TrimLeadingTildeSlash(pattern.Substring(1)));
                }
                else
                {
                    matcher.AddInclude(TrimLeadingTildeSlash(pattern));
                }
            }
        }

        
        /// <summary>
        /// Determines whether a path contains characters suggesting it should be processed as a globbing pattern.
        /// </summary>
        /// <param name="matcher">The <see cref="Matcher"/>.</param>
        /// <param name="pattern">The value to test.</param>
        /// <returns>A <see cref="bool"/> indicating whether the path contains globbing characters.</returns>
        public static bool IsGlobbingPattern([NotNull]this Matcher matcher, [NotNull]string pattern)
        {
            return pattern.IndexOf("*", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string TrimLeadingTildeSlash(string pattern)
        {
            var trimmedPattern = pattern;
            trimmedPattern = trimmedPattern.StartsWith("~") ? trimmedPattern.Substring(1) : trimmedPattern;
            trimmedPattern = trimmedPattern.StartsWith("/") ? trimmedPattern.Substring(1) : trimmedPattern;
            return trimmedPattern;
        }
    }
}