// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Framework.FileSystemGlobbing;

namespace Microsoft.AspNet.Mvc.TagHelpers.Internal
{
    public static class MatcherExtensions
    {
        /// <summary>
        /// Adds include and exclude patterns.
        /// </summary>
        /// <param name="matcher">The <see cref="Matcher"/>.</param>
        /// <param name="includePatterns">The set of include globbing patterns.</param>
        /// <param name="excludePatterns">The set of exclude globbing patterns.</param>
        public static void AddPatterns(
            [NotNull] this Matcher matcher,
            [NotNull] IEnumerable<string> includePatterns,
            IEnumerable<string> excludePatterns)
        {
            foreach (var pattern in includePatterns)
            {
                var includePattern = TrimLeadingSlash(pattern);
                matcher.AddInclude(includePattern);
            }
            if (excludePatterns != null)
            {
                foreach (var pattern in excludePatterns)
                {
                    var excludePattern = TrimLeadingSlash(pattern);
                    matcher.AddExclude(excludePattern);
                }
            }
        }

        private static string TrimLeadingSlash(string value)
        {
            var result = value;

            if (result.StartsWith("/", StringComparison.Ordinal) ||
                result.StartsWith("\\", StringComparison.Ordinal))
            {
                // Trim the leading slash as the matcher runs from the provided root only anyway
                result = result.Substring(1);
            }

            return result;
        }
    }
}