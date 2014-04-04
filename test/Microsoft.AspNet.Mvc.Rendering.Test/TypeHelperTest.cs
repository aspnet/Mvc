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

            // Act
            var dictValues = TypeHelper.ObjectToDictionary(dict);

            // Assert
            Assert.NotNull(dictValues);
            Assert.Equal(0, dictValues.Count);
        }

        [Fact]
        public void ObjectToDictionaryWithPlainObjectTypeReturnsEmptyDictionary()
        {
            // Arrange
            var dict = new object();

            // Act
            var dictValues = TypeHelper.ObjectToDictionary(dict);

            // Assert
            Assert.NotNull(dictValues);
            Assert.Equal(0, dictValues.Count);
        }

        [Fact]
        public void ObjectToDictionaryWithPrimitiveTypeLooksUpPublicProperties()
        {
            // Arrange
            var dict = "test";

            // Act
            var dictValues = TypeHelper.ObjectToDictionary(dict);

            // Assert
            Assert.NotNull(dictValues);
            Assert.Equal(1, dictValues.Count);
            Assert.Equal(4, dictValues["Length"]);
        }

        [Fact]
        public void ObjectToDictionaryWithAnonymousTypeLooksUpProperties()
        {
            // Arrange
            var dict = new { test = "value", other = 1 };

            // Act
            var dictValues = TypeHelper.ObjectToDictionary(dict);

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
            var dict = new { TEST = "value", oThEr = 1 };

            // Act
            var dictValues = TypeHelper.ObjectToDictionary(dict);

            // Assert
            Assert.NotNull(dictValues);
            Assert.Equal(2, dictValues.Count);
            Assert.Equal("value", dictValues["test"]);
            Assert.Equal(1, dictValues["other"]);
        }

        [Fact]
        public void ObjectToDictionaryReturnsInheritedProperties()
        {
            // Arrange
            var value = new ThreeDPoint() {X = 5, Y = 10, Z = 17};

            // Act
            var dictValues = TypeHelper.ObjectToDictionary(value);

            // Assert
            Assert.NotNull(dictValues);
            Assert.Equal(3, dictValues.Count);
            Assert.Equal(5, dictValues["X"]);
            Assert.Equal(10, dictValues["Y"]);
            Assert.Equal(17, dictValues["Z"]);
        }

        private class Point
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        private class ThreeDPoint : Point
        {
            public int Z { get; set; }
        }
    }
}
