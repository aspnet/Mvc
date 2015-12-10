// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Primitives;
using Xunit;

namespace Microsoft.AspNet.Mvc.Formatters
{
    public class MediaTypeComponentTest
    {
        [Fact]
        public void Ctor_InitializesMediaTypeComponent()
        {
            // Arrange & Act
            var component = new MediaTypeComponent(
                new StringSegment("type"),
                new StringSegment("application"));

            // Assert
            Assert.Equal(new StringSegment("type"), component.Name);
            Assert.Equal(new StringSegment("application"), component.Value);
        }

        [Theory]
        [InlineData("type")]
        [InlineData("subtype")]
        public void IsAcceptAll_ReturnsTrueForTypeAndSubtype_IfValueIsAsterisk(string name)
        {
            // Arrange
            var component = new MediaTypeComponent(
                new StringSegment(name),
                new StringSegment("*"));

            // Act
            var result = component.IsMatchesAll();

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("type")]
        [InlineData("subtype")]
        public void IsAcceptAll_ReturnsFalseForTypeAndSubtype_IfValueIsNotAsterisk(string name)
        {
            // Arrange
            var component = new MediaTypeComponent(
                new StringSegment(name),
                new StringSegment("other"));

            // Act
            var result = component.IsMatchesAll();

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("value")]
        [InlineData("*")]
        public void IsAcceptAll_ReturnsFalse_IfNameIsNotTypeOrSubtype(string value)
        {
            // Arrange
            var component = new MediaTypeComponent(
                new StringSegment("other"),
                new StringSegment(value));

            // Act
            var result = component.IsMatchesAll();

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("charset")]
        [InlineData("CHARSET")]
        public void HasName_ReturnsTrue_IfNameIsEqualToValue(string value)
        {
            // Arrange
            var component = new MediaTypeComponent(
                new StringSegment("charset"),
                new StringSegment("other"));

            // Act
            var result = component.HasName(value);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasName_ReturnsFalse_IfNameIsNotEqualToValue()
        {
            // Arrange
            var component = new MediaTypeComponent(
                new StringSegment("charset"),
                new StringSegment("other"));

            // Act
            var result = component.HasName("other");

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("utf-8")]
        [InlineData("UTF-8")]
        public void HasValue_ReturnsTrue_IfValueIsEqualToValue(string value)
        {
            // Arrange
            var component = new MediaTypeComponent(
                new StringSegment("charset"),
                new StringSegment("utf-8"));

            // Act
            var result = component.HasValue(value);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasValue_ReturnsFalse_IfValueIsNotEqualToValue()
        {
            // Arrange
            var component = new MediaTypeComponent(
                new StringSegment("charset"),
                new StringSegment("utf-8"));

            // Act
            var result = component.HasValue("utf-16");

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("charset", "utf-8", "charset", "utf-8")]
        [InlineData("charset", "utf-8", "charset", "UTF-8")]
        [InlineData("charset", "utf-8", "CHARSET", "utf-8")]
        [InlineData("charset", "utf-8", "CHARSET", "UTF-8")]
        public void Equals_ReturnsTrue_IfComponentsHaveTheSameNameAndValue(
            string c1Name,
            string c1Value,
            string c2Name,
            string c2Value)
        {
            var component1 = new MediaTypeComponent(
                new StringSegment(c1Name),
                new StringSegment(c1Value));

            var component2 = new MediaTypeComponent(
                new StringSegment(c2Name),
                new StringSegment(c2Value));

            // Act
            var result = component1.Equals(component2);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("charset", "utf-8", "charset", "other")]
        [InlineData("charset", "utf-8", "other", "utf-8")]
        public void Equals_ReturnsFalse_IfComponentsHaveDifferentNameOrValue(
            string c1Name,
            string c1Value,
            string c2Name,
            string c2Value)
        {
            var component1 = new MediaTypeComponent(
                new StringSegment(c1Name),
                new StringSegment(c1Value));

            var component2 = new MediaTypeComponent(
                new StringSegment(c2Name),
                new StringSegment(c2Value));

            // Act
            var result = component1.Equals(component2);

            // Assert
            Assert.False(result);
        }
    }
}
