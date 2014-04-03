using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNet.Mvc.Rendering.Test
{
    class HtmlAttributePropertyHelperTest
    {
        [Fact]
        public void HtmlAttributePropertyHelperRenamesPropertyNames()
        {
            // Arrange
            var anonymous = new { bar_baz = "foo" };
            PropertyInfo property = anonymous.GetType().GetProperties().First();

            // Act
            HtmlAttributePropertyHelper helper = new HtmlAttributePropertyHelper(property);

            // Assert
            Assert.Equal("bar_baz", property.Name);
            Assert.Equal("bar-baz", helper.Name);
        }

        [Fact]
        public void HtmlAttributePropertyHelperReturnsNameCorrectly()
        {
            // Arrange
            var anonymous = new { foo = "bar" };
            PropertyInfo property = anonymous.GetType().GetProperties().First();

            // Act
            HtmlAttributePropertyHelper helper = new HtmlAttributePropertyHelper(property);

            // Assert
            Assert.Equal("foo", property.Name);
            Assert.Equal("foo", helper.Name);
        }

        [Fact]
        public void HtmlAttributePropertyHelperReturnsValueCorrectly()
        {
            // Arrange
            var anonymous = new { bar = "baz" };
            PropertyInfo property = anonymous.GetType().GetProperties().First();

            // Act
            HtmlAttributePropertyHelper helper = new HtmlAttributePropertyHelper(property);

            // Assert
            Assert.Equal("bar", helper.Name);
            Assert.Equal("baz", helper.GetValue(anonymous));
        }

        [Fact]
        public void HtmlAttributePropertyHelperReturnsValueCorrectlyForValueTypes()
        {
            // Arrange
            var anonymous = new { foo = 32 };
            PropertyInfo property = anonymous.GetType().GetProperties().First();

            // Act
            HtmlAttributePropertyHelper helper = new HtmlAttributePropertyHelper(property);

            // Assert
            Assert.Equal("foo", helper.Name);
            Assert.Equal(32, helper.GetValue(anonymous));
        }

        [Fact]
        public void HtmlAttributePropertyHelperReturnsCachedPropertyHelper()
        {
            // Arrange
            var anonymous = new { foo = "bar" };

            // Act
            PropertyHelper[] helpers1 = HtmlAttributePropertyHelper.GetProperties(anonymous);
            PropertyHelper[] helpers2 = HtmlAttributePropertyHelper.GetProperties(anonymous);

            // Assert
            Assert.Equal(1, helpers1.Length);
            Assert.ReferenceEquals(helpers1, helpers2);
            Assert.ReferenceEquals(helpers1[0], helpers2[0]);
        }

        [Fact]
        public void HtmlAttributeDoesNotShareCacheWithPropertyHelper()
        {
            // Arrange
            var anonymous = new { bar_baz1 = "foo" };

            // Act
            PropertyHelper[] helpers1 = HtmlAttributePropertyHelper.GetProperties(anonymous);
            PropertyHelper[] helpers2 = PropertyHelper.GetProperties(anonymous);

            // Assert
            Assert.Equal(1, helpers1.Length);
            Assert.Equal(1, helpers2.Length);

            Assert.NotEqual<PropertyHelper[]>(helpers1, helpers2);
            Assert.NotEqual<PropertyHelper>(helpers1[0], helpers2[0]);

            Assert.IsType<HtmlAttributePropertyHelper>(helpers1[0]);
            Assert.IsNotType<HtmlAttributePropertyHelper>(helpers2[0]);

            Assert.Equal("bar-baz1", helpers1[0].Name);
            Assert.Equal("bar_baz1", helpers2[0].Name);
        }
    }
}
