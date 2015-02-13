// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.FileSystemGlobbing;
using Xunit;

namespace Microsoft.AspNet.Mvc.TagHelpers.Internal
{
    public class MatcherExtensionsTest
    {
        [Fact]
        public void AddPatterns_CorrectlyAddsInclude()
        {
            // Arrange
            var matcher = new Matcher();
            var includeResult = new List<string>();
            var excludeResult = new List<string>();
            var includePatterns = new[] { "**/test.css", "blank.css" };
            string[] excludePatterns = null;

            // Act
            MatcherExtensions.AddPatterns(
                includePatterns,
                excludePatterns,
                pattern =>
                {
                    includeResult.Add(pattern);
                    return matcher;
                },
                pattern => matcher);

            // Assert
            Assert.Equal(includePatterns, includeResult);
            Assert.Equal(Enumerable.Empty<string>(), excludeResult);
        }

        [Fact]
        public void AddPatterns_CorrectlyAddsIncludeAndExclude()
        {
            // Arrange
            var matcher = new Matcher();
            var includeResult = new List<string>();
            var excludeResult = new List<string>();
            var includePatterns = new[] { "**/test.css", "blank.css" };
            var excludePatterns = new[] { "**/*.min.css" };

            // Act
            MatcherExtensions.AddPatterns(
                includePatterns,
                excludePatterns,
                pattern =>
                {
                    includeResult.Add(pattern);
                    return matcher;
                },
                pattern =>
                {
                    excludeResult.Add(pattern);
                    return matcher;
                });

            // Assert
            Assert.Equal(includePatterns, includeResult);
            Assert.Equal(excludePatterns, excludeResult);
        }

        [Theory]
        [InlineData("/")]
        [InlineData("\\")]
        public void AddPatterns_TrimsLeadingSlashFromPatterns(string leadingSlash)
        {
            // Arrange
            var matcher = new Matcher();
            var includeResult = new List<string>();
            var excludeResult = new List<string>();
            var includePatterns = new[] { $"{leadingSlash}**/test.css", $"{leadingSlash}blank.css" };
            var excludePatterns = new[] { $"{leadingSlash}**/*.min.css" };

            // Act
            MatcherExtensions.AddPatterns(
                includePatterns,
                excludePatterns,
                pattern =>
                {
                    includeResult.Add(pattern);
                    return matcher;
                },
                pattern =>
                {
                    excludeResult.Add(pattern);
                    return matcher;
                });

            // Assert
            Assert.Equal(new[] { "**/test.css", "blank.css" }, includeResult);
            Assert.Equal(new[] { "**/*.min.css" }, excludeResult);
        }

        [Theory]
        [InlineData("/")]
        [InlineData("\\")]
        public void AddPatterns_TrimsOnlySingleLeadingSlashFromPatterns(string leadingSlash)
        {
            // Arrange
            var leadingSlashes = $"{leadingSlash}{leadingSlash}";
            var matcher = new Matcher();
            var includeResult = new List<string>();
            var excludeResult = new List<string>();
            var includePatterns = new[] { $"{leadingSlashes}**/test.css", $"{leadingSlashes}blank.css" };
            var excludePatterns = new[] { $"{leadingSlashes}**/*.min.css" };

            // Act
            MatcherExtensions.AddPatterns(
                includePatterns,
                excludePatterns,
                pattern =>
                {
                    includeResult.Add(pattern);
                    return matcher;
                },
                pattern =>
                {
                    excludeResult.Add(pattern);
                    return matcher;
                });

            // Assert
            Assert.Equal(new[] { $"{leadingSlash}**/test.css", $"{leadingSlash}blank.css" }, includeResult);
            Assert.Equal(new[] { $"{leadingSlash}**/*.min.css" }, excludeResult);
        }
    }
}