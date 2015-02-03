// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc;
using System.Collections.Generic;

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
        /// </summary>
        /// <param name="matcher">The <see cref="Matcher"/>.</param>
        /// <param name="patterns">The set of globbing patterns.</param>
        public static void AddPatterns([NotNull]this Matcher matcher, [NotNull]IEnumerable<string> patterns)
        {
            foreach (var pattern in patterns)
            {
                if (pattern.StartsWith("!", StringComparison.OrdinalIgnoreCase))
                {
                    matcher.AddExclude(pattern);
                }
                else
                {
                    matcher.AddInclude(pattern);
                }
            }
        }
    }
}