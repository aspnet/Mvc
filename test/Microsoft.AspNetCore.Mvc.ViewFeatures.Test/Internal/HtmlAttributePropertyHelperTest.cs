// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Internal;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    public class HtmlAttributePropertyHelperTest
    {
        [Fact]
        public void HtmlAttributePropertyHelper_RenamesPropertyNames()
        {
            // Arrange
            var anonymous = new { bar_baz = "foo" };
            var property = anonymous.GetType().GetTypeInfo().DeclaredProperties.First();

            // Act
            var helper = new HtmlAttributePropertyHelper(property);

            // Assert
            Assert.Equal("bar_baz", property.Name);
            Assert.Equal("bar-baz", helper.Name);
        }

        [Fact]
        public void HtmlAttributePropertyHelper_ReturnsNameCorrectly()
        {
            // Arrange
            var anonymous = new { foo = "bar" };
            var property = anonymous.GetType().GetTypeInfo().DeclaredProperties.First();

            // Act
            var helper = new HtmlAttributePropertyHelper(property);

            // Assert
            Assert.Equal("foo", property.Name);
            Assert.Equal("foo", helper.Name);
        }

        [Fact]
        public void HtmlAttributePropertyHelper_ReturnsValueCorrectly()
        {
            // Arrange
            var anonymous = new { bar = "baz" };
            var property = anonymous.GetType().GetTypeInfo().DeclaredProperties.First();

            // Act
            var helper = new HtmlAttributePropertyHelper(property);

            // Assert
            Assert.Equal("bar", helper.Name);
            Assert.Equal("baz", helper.GetValue(anonymous));
        }

        [Fact]
        public void HtmlAttributePropertyHelper_ReturnsValueCorrectly_ForValueTypes()
        {
            // Arrange
            var anonymous = new { foo = 32 };
            var property = anonymous.GetType().GetTypeInfo().DeclaredProperties.First();

            // Act
            var helper = new HtmlAttributePropertyHelper(property);

            // Assert
            Assert.Equal("foo", helper.Name);
            Assert.Equal(32, helper.GetValue(anonymous));
        }

        [Fact]
        public void HtmlAttributePropertyHelper_ReturnsCachedPropertyHelper()
        {
            // Arrange
            var anonymous = new { foo = "bar" };

            // Act
            var helpers1 = HtmlAttributePropertyHelper.GetProperties(anonymous.GetType());
            var helpers2 = HtmlAttributePropertyHelper.GetProperties(anonymous.GetType());

            // Assert
            Assert.Single(helpers1);
            Assert.Same(helpers1, helpers2);
            Assert.Same(helpers1[0], helpers2[0]);
        }

        [Fact]
        public void HtmlAttributePropertyHelper_DoesNotShareCacheWithPropertyHelper()
        {
            // Arrange
            var anonymous = new { bar_baz1 = "foo" };

            // Act
            var helpers1 = HtmlAttributePropertyHelper.GetProperties(anonymous.GetType());
            var helpers2 = PropertyHelper.GetProperties(anonymous.GetType());

            // Assert
            Assert.Single(helpers1);
            Assert.Single(helpers2);

            Assert.NotEqual<PropertyHelper[]>(helpers1, helpers2);
            Assert.NotEqual<PropertyHelper>(helpers1[0], helpers2[0]);

            Assert.IsType<HtmlAttributePropertyHelper>(helpers1[0]);
            Assert.IsNotType<HtmlAttributePropertyHelper>(helpers2[0]);

            Assert.Equal("bar-baz1", helpers1[0].Name);
            Assert.Equal("bar_baz1", helpers2[0].Name);
        }
    }
}
