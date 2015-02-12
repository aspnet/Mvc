using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.FileSystemGlobbing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.TagHelpers.Internal
{
    public class MatcherExtensionsTest
    {
        [Fact]
        public void AddPatternsImpl_CorrectlyAddsInclude()
        {
            // Arrange
            var matcher = new Matcher();
            var includeResult = new List<string>();
            var excludeResult = new List<string>();
            var includePatterns = new[] { "**/test.css", "blank.css" };
            string[] excludePatterns = null;

            // Act
            MatcherExtensions.AddPatternsImpl(
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
        public void AddPatternsImpl_CorrectlyAddsIncludeAndExclude()
        {
            // Arrange
            var matcher = new Matcher();
            var includeResult = new List<string>();
            var excludeResult = new List<string>();
            var includePatterns = new[] { "**/test.css", "blank.css" };
            var excludePatterns = new[] { "**/*.min.css" };

            // Act
            MatcherExtensions.AddPatternsImpl(
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
        public void AddPatternsImpl_TrimsLeadingSlashFromPatterns(string leadingSlash)
        {
            // Arrange
            var matcher = new Matcher();
            var includeResult = new List<string>();
            var excludeResult = new List<string>();
            var includePatterns = new[] { $"{leadingSlash}**/test.css", $"{leadingSlash}blank.css" };
            var excludePatterns = new[] { $"{leadingSlash}**/*.min.css" };

            // Act
            MatcherExtensions.AddPatternsImpl(
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
    }
}