using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNet.Mvc.Rendering.Test
{
    class PropertyHelperTest
    {
        [Fact]
        public void PropertyHelperReturnsNameCorrectly()
        {
            // Arrange
            var anonymous = new { foo = "bar" };
            PropertyInfo property = anonymous.GetType().GetProperties().First();

            // Act
            PropertyHelper helper = new PropertyHelper(property);

            // Assert
            Assert.Equal("foo", property.Name);
            Assert.Equal("foo", helper.Name);
        }

        [Fact]
        public void PropertyHelperReturnsValueCorrectly()
        {
            // Arrange
            var anonymous = new { bar = "baz" };
            PropertyInfo property = anonymous.GetType().GetProperties().First();

            // Act
            PropertyHelper helper = new PropertyHelper(property);

            // Assert
            Assert.Equal("bar", helper.Name);
            Assert.Equal("baz", helper.GetValue(anonymous));
        }

        [Fact]
        public void PropertyHelperReturnsValueCorrectlyForValueTypes()
        {
            // Arrange
            var anonymous = new { foo = 32 };
            PropertyInfo property = anonymous.GetType().GetProperties().First();

            // Act
            PropertyHelper helper = new PropertyHelper(property);

            // Assert
            Assert.Equal("foo", helper.Name);
            Assert.Equal(32, helper.GetValue(anonymous));
        }

        [Fact]
        public void PropertyHelperReturnsCachedPropertyHelper()
        {
            // Arrange
            var anonymous = new { foo = "bar" };

            // Act
            PropertyHelper[] helpers1 = PropertyHelper.GetProperties(anonymous);
            PropertyHelper[] helpers2 = PropertyHelper.GetProperties(anonymous);

            // Assert
            Assert.Equal(1, helpers1.Length);
            Assert.ReferenceEquals(helpers1, helpers2);
            Assert.ReferenceEquals(helpers1[0], helpers2[0]);
        }

        [Fact]
        public void PropertyHelperDoesNotChangeUnderscores()
        {
            // Arrange
            var anonymous = new { bar_baz2 = "foo" };

            // Act + Assert
            PropertyHelper helper = Assert.Single(PropertyHelper.GetProperties(anonymous));
            Assert.Equal("bar_baz2", helper.Name);
        }

        private class PrivateProperties
        {
            public int Prop1 { get; set; }
            protected int Prop2 { get; set; }
            private int Prop3 { get; set; }
        }

        [Fact]
        public void PropertyHelperDoesNotFindPrivateProperties()
        {
            // Arrange
            var anonymous = new PrivateProperties();

            // Act + Assert
            PropertyHelper helper = Assert.Single(PropertyHelper.GetProperties(anonymous));
            Assert.Equal("Prop1", helper.Name);
        }

        private class Static
        {
            public static int Prop2 { get; set; }
            public int Prop5 { get; set; }
        }

        [Fact]
        public void PropertyHelperDoesNotFindStaticProperties()
        {
            // Arrange
            var anonymous = new Static();

            // Act + Assert
            PropertyHelper helper = Assert.Single(PropertyHelper.GetProperties(anonymous));
            Assert.Equal("Prop5", helper.Name);
        }

        private class SetOnly
        {
            public int Prop2 { set { } }
            public int Prop6 { get; set; }
        }

        [Fact]
        public void PropertyHelperDoesNotFindSetOnlyProperties()
        {
            // Arrange
            var anonymous = new SetOnly();

            // Act + Assert
            PropertyHelper helper = Assert.Single(PropertyHelper.GetProperties(anonymous));
            Assert.Equal("Prop6", helper.Name);
        }

        private struct MyProperties
        {
            public int IntProp { get; set; }
            public string StringProp { get; set; }
        }

        [Fact]
        public void PropertyHelperWorksForStruct()
        {
            // Arrange
            var anonymous = new MyProperties();

            anonymous.IntProp = 3;
            anonymous.StringProp = "Five";

            // Act + Assert
            PropertyHelper helper1 = Assert.Single(PropertyHelper.GetProperties(anonymous).Where(prop => prop.Name == "IntProp"));
            PropertyHelper helper2 = Assert.Single(PropertyHelper.GetProperties(anonymous).Where(prop => prop.Name == "StringProp"));
            Assert.Equal(3, helper1.GetValue(anonymous));
            Assert.Equal("Five", helper2.GetValue(anonymous));
        }

        public class BaseClass
        {
            public string PropA { get; set; }

            protected string PropProtected { get; set; }
        }

        public class DerivedClass : BaseClass
        {
            public string PropB { get; set; }
        }

        public class BaseClassWithVirtual
        {
            public virtual string PropA { get; set; }
            public string PropB { get; set; }
        }

        public class DerivedClassWithNew : BaseClassWithVirtual
        {
            public new string PropB { get { return "Newed"; } }
        }

        public class DerivedClassWithOverride : BaseClassWithVirtual
        {
            public override string PropA { get { return "Overriden"; } }
        }

        [Fact]
        public void PropertyHelperForDerivedClass()
        {
            // Arrange
            object derived = new DerivedClass { PropA = "propAValue", PropB = "propBValue" };

            // Act
            PropertyHelper[] helpers = PropertyHelper.GetProperties(derived).ToArray();

            // Assert
            Assert.NotNull(helpers);
            Assert.Equal(2, helpers.Length);

            PropertyHelper propAHelper = Assert.Single(helpers.Where(h => h.Name == "PropA"));
            PropertyHelper propBHelper = Assert.Single(helpers.Where(h => h.Name == "PropB"));

            Assert.Equal("propAValue", propAHelper.GetValue(derived));
            Assert.Equal("propBValue", propBHelper.GetValue(derived));
        }

        [Fact]
        public void PropertyHelperForDerivedClassWithNew()
        {
            // Arrange
            object derived = new DerivedClassWithNew { PropA = "propAValue" };

            // Act
            PropertyHelper[] helpers = PropertyHelper.GetProperties(derived).ToArray();

            // Assert
            Assert.NotNull(helpers);
            Assert.Equal(2, helpers.Length);

            PropertyHelper propAHelper = Assert.Single(helpers.Where(h => h.Name == "PropA"));
            PropertyHelper propBHelper = Assert.Single(helpers.Where(h => h.Name == "PropB"));

            Assert.Equal("propAValue", propAHelper.GetValue(derived));
            Assert.Equal("Newed", propBHelper.GetValue(derived));
        }

        [Fact]
        public void PropertyHelperForDerivedWithVirtual()
        {
            // Arrange
            object derived = new DerivedClassWithOverride { PropA = "propAValue", PropB = "propBValue" };

            // Act
            PropertyHelper[] helpers = PropertyHelper.GetProperties(derived).ToArray();

            // Assert
            Assert.NotNull(helpers);
            Assert.Equal(2, helpers.Length);

            PropertyHelper propAHelper = Assert.Single(helpers.Where(h => h.Name == "PropA"));
            PropertyHelper propBHelper = Assert.Single(helpers.Where(h => h.Name == "PropB"));

            Assert.Equal("Overriden", propAHelper.GetValue(derived));
            Assert.Equal("propBValue", propBHelper.GetValue(derived));
        }
    }
}
