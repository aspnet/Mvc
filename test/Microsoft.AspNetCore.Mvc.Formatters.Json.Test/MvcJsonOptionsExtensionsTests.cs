// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNetCore.Mvc
{
    public class MvcJsonOptionsExtensionsTests
    {
        [Fact]
        public void UseCamelCasing_WillChange_PropertyNames()
        {
            // Arrange
            var options = new MvcJsonOptions().UseCamelCasing();
            var foo = new { TestName = "TestFoo", TestValue = 10 };
            var expected = "{\"testName\":\"TestFoo\",\"testValue\":10}";

            // Act
            var actual = SerializeToJson(options, value: foo);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void UseCamelCasing_WillChangeFirstPartBeforeSeparator_InPropertyName()
        {
            // Arrange
            var options = new MvcJsonOptions().UseCamelCasing();
            var foo = new { TestFoo_TestValue = "Test" };
            var expected = "{\"testFoo_TestValue\":\"Test\"}";

            // Act
            var actual = SerializeToJson(options, value: foo);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void UseCamelCasing_WillChange_DictionaryKeys()
        {
            // Arrange
            var options = new MvcJsonOptions().UseCamelCasing();
            var dictionary = new Dictionary<string, int>
            {
                ["HelloWorld"] = 1,
                ["HELLOWORLD"] = 2
            };
            var expected = "{\"helloWorld\":1,\"helloworld\":2}";

            // Act
            var actual = SerializeToJson(options, value: dictionary);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void UseCamelCasing_WillChangeFirstPartBeforeSeparator_InDictionaryKey()
        {
            // Arrange
            var options = new MvcJsonOptions().UseCamelCasing();
            var dictionary = new Dictionary<string, int>()
            {
                ["HelloWorld_HelloWorld"] = 1
            };

            var expected = "{\"helloWorld_HelloWorld\":1}";

            // Act
            var actual = SerializeToJson(options, value: dictionary);

            // Assert
            Assert.Equal(expected, actual);
        }


        [Fact]
        public void UseMemberCasing_WillNotChange_PropertyNames()
        {
            // Arrange
            var options = new MvcJsonOptions().UseMemberCasing();
            var foo = new { fooName = "Test", FooValue = "Value"};
            var expected = "{\"fooName\":\"Test\",\"FooValue\":\"Value\"}";

            // Act
            var actual = SerializeToJson(options, value: foo);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void UseMemberCasing_WillNotChange_DictionaryKeys()
        {
            // Arrange
            var options = new MvcJsonOptions().UseMemberCasing();
            var dictionary = new Dictionary<string, int>()
            {
                ["HelloWorld"] = 1,
                ["helloWorld"] = 2,
                ["HELLO-WORLD"] = 3
            };
            var expected = "{\"HelloWorld\":1,\"helloWorld\":2,\"HELLO-WORLD\":3}";

            // Act
            var actual = SerializeToJson(options, value: dictionary);

            // Assert
            Assert.Equal(expected, actual);
        }

        private static string SerializeToJson(MvcJsonOptions options, object value)
        {
            return JsonConvert.SerializeObject(
                value: value,
                formatting: Formatting.None,
                settings: options.SerializerSettings);
        }
    }
}
