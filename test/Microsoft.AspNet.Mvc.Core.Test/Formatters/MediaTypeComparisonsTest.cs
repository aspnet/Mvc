using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Microsoft.AspNet.Mvc.Formatters
{
    public class MediaTypeComparisonsTest
    {
        [Theory]
        [InlineData("application/json", "application/json", true, true)]
        [InlineData("application/json", "application/json;charset=utf-8", true, true)]
        [InlineData("application/json;charset=utf-8", "application/json", true, false)]
        [InlineData("application/json;q=0.8", "application/json;q=0.9", true, true)]
        [InlineData("application/json;q=0.8", "application/json;q=0.9", false, false)]
        [InlineData("application/json;q=0.8", "application/json;q=0.8", false, true)]
        [InlineData("application/json;format=indent;charset=utf-8", "application/json", true, false)]
        [InlineData("application/json", "application/json;format=indent;charset=utf-8", true, true)]
        [InlineData("application/json;format=indent;charset=utf-8", "application/json;format=indent;charset=utf-8", true, true)]
        [InlineData("application/json;charset=utf-8;format=indent", "application/json;format=indent;charset=utf-8", true, true)]
        public void IsSubsetOf(string set, string subset, bool ignoreQuality, bool expectedResult)
        {
            // Arrange & Act
            var result = MediaTypeComparisons.IsSubsetOf(
                new StringSegment(set),
                new StringSegment(subset),
                ignoreQuality);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("application/json", "application/json", true, true)]
        [InlineData("application/json", "application/json;charset=utf-8", true, false)]
        [InlineData("application/json;charset=utf-8", "application/json", true, false)]
        [InlineData("application/json;charset=utf-8", "application/json;charset=utf-8", true, true)]
        [InlineData("application/json;q=0.8", "application/json;q=0.9", true, true)]
        [InlineData("application/json;q=0.8", "application/json;q=0.9", false, false)]
        [InlineData("application/json;q=0.8", "application/json;q=0.8", false, true)]
        [InlineData("application/json;q=0.8; charset=utf-8", "application/json;charset=utf-8;q=0.9", true, true)]
        [InlineData("application/json;q=0.8; charset=utf-8", "application/json;charset=utf-8;q=0.9", false, false)]
        [InlineData("application/json;q=0.8; charset=utf-8", "application/json;charset=utf-8;q=0.8", false, true)]
        public void AreEqual(string left, string right, bool ignoreQuality, bool expectedResult)
        {
            // Arrange & Act
            var result = MediaTypeComparisons.AreEqual(
                new StringSegment(left),
                new StringSegment(right),
                ignoreQuality);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("*/*", true)]
        [InlineData("text/*", false)]
        [InlineData("text/plain", false)]
        public void MatchesAllTypes(string value, bool expectedResult)
        {
            // Arrange
            var mediaType = new StringSegment(value);

            // Act
            var result = MediaTypeComparisons.MatchesAllTypes(mediaType);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("*/*", true)]
        [InlineData("text/*", true)]
        [InlineData("text/plain", false)]
        public void MatchesAllSubtypes(string value, bool expectedResult)
        {
            // Arrange
            var mediaType = new StringSegment(value);

            // Act
            var result = MediaTypeComparisons.MatchesAllSubtypes(mediaType);

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }
}
