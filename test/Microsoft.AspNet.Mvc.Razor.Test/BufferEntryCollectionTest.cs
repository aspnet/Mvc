// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.AspNet.Mvc.Razor
{
    public class BufferEntryCollectionTest
    {
        [Fact]
        public void Add_AddsBufferEntries()
        {
            // Arrange
            var collection = new BufferEntryCollection();
            var inner = new BufferEntryCollection();

            // Act
            collection.Add("Hello");
            collection.Add(new[] { 'a', 'b', 'c' }, 1, 2);
            collection.Add(inner);

            // Assert
            Assert.Equal("Hello", collection.BufferEntries[0].Value);
            Assert.Equal("bc", collection.BufferEntries[1].Value);
            Assert.Same(inner, collection.BufferEntries[2].Buffer);
        }

        public static IEnumerable<object[]> Enumerator_TraversesThroughBufferData
        {
            get
            {
                var collection1 = new BufferEntryCollection();
                collection1.Add("foo");
                collection1.Add("bar");

                var expected1 = new[]
                {
                    "foo",
                    "bar"
                };
                yield return new object[] { collection1, expected1 };

                // Nested collection
                var nestedCollection2 = new BufferEntryCollection();
                nestedCollection2.Add("level 1");
                var nestedCollection2SecondLevel = new BufferEntryCollection();
                nestedCollection2SecondLevel.Add("level 2");
                nestedCollection2.Add(nestedCollection2SecondLevel);
                var collection2 = new BufferEntryCollection();
                collection2.Add("foo");
                collection2.Add(nestedCollection2);
                collection2.Add("qux");

                var expected2 = new[]
                {
                    "foo",
                    "level 1",
                    "level 2",
                    "qux"
                };
                yield return new object[] { collection2, expected2 };

                // Nested collection
                var collection3 = new BufferEntryCollection();
                collection3.Add("Hello");
                var emptyNestedCollection = new BufferEntryCollection();
                emptyNestedCollection.Add(new BufferEntryCollection());
                collection3.Add(emptyNestedCollection);
                collection3.Add("world");

                var expected3 = new[]
                {
                    "Hello",
                    "world"
                };
                yield return new object[] { collection3, expected3 };
            }
        }


        [Theory]
        [MemberData("Enumerator_TraversesThroughBufferData")]
        public void Enumerator_TraversesThroughBuffer(BufferEntryCollection buffer, string[] expected)
        {
            // Act and Assert
            Assert.Equal(expected, buffer);
        }
    }
}