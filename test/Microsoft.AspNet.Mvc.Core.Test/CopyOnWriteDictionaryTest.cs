﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core
{
    public class CopyOnWriteDictionaryTest
    {
        [Fact]
        public void ReadOperation_DelegatesToSourceDictionary_IfNoMutationsArePerformed()
        {
            // Arrange
            var values = new List<object>();
            var enumerator = Mock.Of<IEnumerator<KeyValuePair<string, object>>>();
            var sourceDictionary = new Mock<IDictionary<string, object>>();
            sourceDictionary.SetupGet(d => d.Count)
                            .Returns(100)
                            .Verifiable();
            sourceDictionary.SetupGet(d => d.Values)
                            .Returns(values)
                            .Verifiable();
            sourceDictionary.Setup(d => d.ContainsKey("test-key"))
                            .Returns(value: true)
                            .Verifiable();
            sourceDictionary.Setup(d => d.GetEnumerator())
                            .Returns(enumerator)
                            .Verifiable();
            sourceDictionary.Setup(d => d["key2"])
                            .Returns("key2-value")
                            .Verifiable();
            object value;
            sourceDictionary.Setup(d => d.TryGetValue("different-key", out value))
                            .Returns(false)
                            .Verifiable();

            var copyOnWriteDictionary = new CopyOnWriteDictionary<object>(sourceDictionary.Object);

            // Act and Assert
            Assert.Equal("key2-value", copyOnWriteDictionary["key2"]);
            Assert.Equal(100, copyOnWriteDictionary.Count);
            Assert.Same(values, copyOnWriteDictionary.Values);
            Assert.True(copyOnWriteDictionary.ContainsKey("test-key"));
            Assert.Same(enumerator, copyOnWriteDictionary.GetEnumerator());
            Assert.False(copyOnWriteDictionary.TryGetValue("different-key", out value));
            sourceDictionary.Verify();
        }

        [Fact]
        public void ReadOperation_DoesNotDelegateToSourceDictionary_OnceAValueIsChanged()
        {
            // Arrange
            var values = new List<object>();
            var enumerator = new List<KeyValuePair<string, object>>().GetEnumerator();
            var sourceDictionary = new Dictionary<string, object>
            {
                { "key1", "value1" },
                { "key2", "value2" }
            };
            var copyOnWriteDictionary = new CopyOnWriteDictionary<object>(sourceDictionary);

            // Act
            copyOnWriteDictionary["key2"] = "value3";

            // Assert
            Assert.Equal("value2", sourceDictionary["key2"]);
            Assert.Equal(2, copyOnWriteDictionary.Count);
            Assert.Equal("value1", copyOnWriteDictionary["key1"]);
            Assert.Equal("value3", copyOnWriteDictionary["key2"]);
        }

        [Fact]
        public void ReadOperation_DoesNotDelegateToSourceDictionary_OnceDictionaryIsModified()
        {
            // Arrange
            var values = new List<object>();
            var enumerator = new List<KeyValuePair<string, object>>().GetEnumerator();
            var sourceDictionary = new Dictionary<string, object>
            {
                { "key1", "value1" },
                { "key2", "value2" }
            };
            var copyOnWriteDictionary = new CopyOnWriteDictionary<object>(sourceDictionary);

            // Act
            copyOnWriteDictionary.Add("key3", "value3");
            copyOnWriteDictionary.Remove("key1");


            // Assert
            Assert.Equal(2, sourceDictionary.Count);
            Assert.Equal("value1", sourceDictionary["key1"]);
            Assert.Equal(2, copyOnWriteDictionary.Count);
            Assert.Equal("value2", copyOnWriteDictionary["key2"]);
            Assert.Equal("value3", copyOnWriteDictionary["key3"]);
        }
    }
}