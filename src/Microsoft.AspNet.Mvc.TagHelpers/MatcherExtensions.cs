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
        /// </summary>
        /// <param name="matcher">The <see cref="Matcher"/>.</param>
        /// <param name="includePatterns">The set of include globbing patterns.</param>
        /// <param name="excludePatterns">The set of exclude globbing patterns.</param>
        public static void AddPatterns([NotNull]this Matcher matcher,
            [NotNull]IEnumerable<string> includePatterns,
            IEnumerable<string> excludePatterns = null)
        {
            foreach (var pattern in includePatterns)
            {
                matcher.AddInclude(pattern);
            }
            if (excludePatterns != null)
            {
                foreach (var pattern in excludePatterns)
                {
                    matcher.AddExclude(pattern);
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
    }
}