// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Microsoft.AspNet.Mvc.Formatters
{
    public class MediaTypeParserTests
    {
        [Theory]
        [InlineData("application/json")]
        [InlineData("application /json")]
        public void CanParse_ParameterlessMediaTypes(string mediaType)
        {
            // Arrange
            var expectedComponents = new[]
            {
                new MediaTypeComponent(
                    new StringSegment("type"),
                    new StringSegment("application")),
                new MediaTypeComponent(
                    new StringSegment("subtype"),
                    new StringSegment("json")),
            };

            var parser = new MediaTypeParser(mediaType, 0, mediaType.Length);

            // Act
            var result = parser.ToArray();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedComponents, result);
        }

        [Theory]
        [InlineData("application/json;format=pretty;charset=utf-8;q=0.8")]
        [InlineData("application/json;format=pretty;charset=utf-8; q=0.8 ")]
        [InlineData("application/json;format=pretty;charset=utf-8 ; q=0.8 ")]
        [InlineData("application/json;format=pretty; charset=utf-8 ; q=0.8 ")]
        [InlineData("application/json;format=pretty ; charset=utf-8 ; q=0.8 ")]
        [InlineData("application/json; format=pretty ; charset=utf-8 ; q=0.8 ")]
        [InlineData("application/json; format=pretty ; charset=utf-8 ; q=  0.8 ")]
        [InlineData("application/json; format=pretty ; charset=utf-8 ; q  =  0.8 ")]
        public void CanParse_MediaTypesWithParameters(string mediaType)
        {
            // Arrange
            var expectedComponents = new[]
            {
                new MediaTypeComponent(
                    new StringSegment("type"),
                    new StringSegment("application")),
                new MediaTypeComponent(
                    new StringSegment("subtype"),
                    new StringSegment("json")),
                new MediaTypeComponent(
                    new StringSegment("format"),
                    new StringSegment("pretty")),
                new MediaTypeComponent(
                    new StringSegment("charset"),
                    new StringSegment("utf-8")),
                new MediaTypeComponent(
                    new StringSegment("q"),
                    new StringSegment("0.8")),
            };

            var parser = new MediaTypeParser(mediaType, 0, mediaType.Length);

            // Act
            var result = parser.ToArray();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedComponents, result);
        }

        [Theory]
        [InlineData("application/json;charset=utf-8")]
        [InlineData("application/json;format=indent;q=0.8;charset=utf-8")]
        [InlineData("application/json;format=indent;charset=utf-8;q=0.8")]
        [InlineData("application/json;charset=utf-8;format=indent;q=0.8")]
        public void GetParameter_ReturnsParameter_IfParameterIsInMediaType(string mediaType)
        {
            // Arrange
            var expectedParameter = new MediaTypeComponent(
                new StringSegment("charset"),
                new StringSegment("utf-8"));

            var parser = new MediaTypeParser(mediaType, 0, mediaType.Length);

            // Act
            var result = parser.GetParameter("charset");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedParameter, result.Value);
        }

        [Fact]
        public void GetParameter_ReturnsNull_IfParameterIsNotInMediaType()
        {
            var expectedParameter = new MediaTypeComponent(
                new StringSegment("charset"),
                new StringSegment("utf-8"));

            var mediaType = "application/json;charset=utf-8;format=indent;q=0.8";

            var parser = new MediaTypeParser(mediaType, 0, mediaType.Length);

            // Act
            var result = parser.GetParameter("other");

            // Assert
            Assert.Null(result);
        }
    }
}
