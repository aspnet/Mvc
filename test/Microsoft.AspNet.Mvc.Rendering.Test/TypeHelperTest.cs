using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNet.Mvc.Rendering.Test
{
    public class TypeHelperTest
    {
        [Fact]
        public void ObjectToDictionaryWithNullObjectReturnsEmptyDictionary()
        {
            // Arrange
            object dict = null;

            IDictionary<string, object> dictValues = TypeHelper.ObjectToDictionary(dict);

            Assert.NotNull(dictValues);
            Assert.Equal(0, dictValues.Count);
        }

        [Fact]
        public void ObjectToDictionaryWithPlainObjectTypeReturnsEmptyDictionary()
        {
            // Arrange
            object dict = new object();

            // Act
            IDictionary<string, object> dictValues = TypeHelper.ObjectToDictionary(dict);

            // Assert
            Assert.NotNull(dictValues);
            Assert.Equal(0, dictValues.Count);
        }

        [Fact]
        public void ObjectToDictionaryWithPrimitiveTypeLooksUpPublicProperties()
        {
            // Arrange
            object dict = "test";

            // Act
            IDictionary<string, object> dictValues = TypeHelper.ObjectToDictionary(dict);

            // Assert
            Assert.NotNull(dictValues);
            Assert.Equal(1, dictValues.Count);
            Assert.Equal(4, dictValues["Length"]);
        }

        [Fact]
        public void ObjectToDictionaryWithAnonymousTypeLooksUpProperties()
        {
            // Arrange
            object dict = new { test = "value", other = 1 };

            // Act
            IDictionary<string, object> dictValues = TypeHelper.ObjectToDictionary(dict);

            // Assert
            Assert.NotNull(dictValues);
            Assert.Equal(2, dictValues.Count);
            Assert.Equal("value", dictValues["test"]);
            Assert.Equal(1, dictValues["other"]);
        }

        [Fact]
        public void ObjectToDictionaryReturnsCaseInsensitiveDictionary()
        {
            // Arrange
            object dict = new { TEST = "value", oThEr = 1 };

            // Act
            IDictionary<string, object> dictValues = TypeHelper.ObjectToDictionary(dict);

            // Assert
            Assert.NotNull(dictValues);
            Assert.Equal(2, dictValues.Count);
            Assert.Equal("value", dictValues["test"]);
            Assert.Equal(1, dictValues["other"]);
        }

        [Fact]
        public void AddAnonymousTypeObjectToDictionaryTest()
        {
            IDictionary<string, object> d = new Dictionary<string, object>();
            d.Add("X", "Xvalue");
            TypeHelper.AddAnonymousObjectToDictionary(d, new { A = "a", B = "b" });
            Assert.Equal("Xvalue", d["X"]);
            Assert.Equal("a", d["A"]);
            Assert.Equal("b", d["B"]);
        }

        [Fact]
        public void IsAnonymousTypeTest()
        {
            Assert.False(TypeHelper.IsAnonymousType(typeof(object)));
            Assert.False(TypeHelper.IsAnonymousType(typeof(string)));
            Assert.False(TypeHelper.IsAnonymousType(typeof(IDictionary<object, object>)));
            Assert.True(TypeHelper.IsAnonymousType((new { A = "a", B = "b" }.GetType())));
            var x = "x";
            var y = "y";
            Assert.True(TypeHelper.IsAnonymousType((new { x, y }.GetType())));
        }

        [Fact]
        public void IsAnonymousTypeNullTest()
        {
            Assert.ThrowsArgumentNull(() => TypeHelper.IsAnonymousType(null), "type");
        }
    }
}
