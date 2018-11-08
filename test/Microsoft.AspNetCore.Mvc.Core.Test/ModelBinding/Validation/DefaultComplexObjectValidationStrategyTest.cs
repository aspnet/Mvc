// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Validation
{
    public class DefaultComplexObjectValidationStrategyTest
    {
        [Fact]
        public void GetChildren_ReturnsExpectedElements()
        {
            // Arrange
            var model = new Person()
            {
                Age = 23,
                Id = 1,
                Name = "Joey",
            };

            var metadata = TestModelMetadataProvider.CreateDefaultProvider().GetMetadataForType(typeof(Person));
            var strategy = DefaultComplexObjectValidationStrategy.Instance;

            // Act
            var enumerator = strategy.GetChildren(metadata, "prefix", model);

            // Assert
            Assert.Collection(
                BufferEntries(enumerator).OrderBy(e => e.Key),
                entry =>
                {
                    Assert.Equal("prefix.Age", entry.Key);
                    Assert.Equal(23, entry.Model);
                    Assert.Same(metadata.Properties["Age"], entry.Metadata);
                },
                entry =>
                {
                    Assert.Equal("prefix.Id", entry.Key);
                    Assert.Equal(1, entry.Model);
                    Assert.Same(metadata.Properties["Id"], entry.Metadata);
                },
                entry =>
                {
                    Assert.Equal("prefix.Name", entry.Key);
                    Assert.Equal("Joey", entry.Model);
                    Assert.Same(metadata.Properties["Name"], entry.Metadata);
                });
        }

        [Fact]
        public void GetChildren_SetsModelNull_IfContainerNull()
        {
            // Arrange
            Person model = null;
            var metadata = TestModelMetadataProvider.CreateDefaultProvider().GetMetadataForType(typeof(Person));
            var strategy = DefaultComplexObjectValidationStrategy.Instance;

            // Act
            var enumerator = strategy.GetChildren(metadata, "prefix", model);

            // Assert
            Assert.Collection(
                BufferEntries(enumerator).OrderBy(e => e.Key),
                entry =>
                {
                    Assert.Equal("prefix.Age", entry.Key);
                    Assert.Null(entry.Model);
                    Assert.Same(metadata.Properties["Age"], entry.Metadata);
                },
                entry =>
                {
                    Assert.Equal("prefix.Id", entry.Key);
                    Assert.Null(entry.Model);
                    Assert.Same(metadata.Properties["Id"], entry.Metadata);
                },
                entry =>
                {
                    Assert.Equal("prefix.Name", entry.Key);
                    Assert.Null(entry.Model);
                    Assert.Same(metadata.Properties["Name"], entry.Metadata);
                });
        }

        [Fact]
        public void GetChildren_LazyLoadsModel()
        {
            // Arrange
            var model = new LazyPerson(input: null);
            var metadata = TestModelMetadataProvider.CreateDefaultProvider().GetMetadataForType(typeof(LazyPerson));
            var strategy = DefaultComplexObjectValidationStrategy.Instance;

            // Act
            var enumerator = strategy.GetChildren(metadata, "prefix", model);

            // Assert
            // Note: NREs are not thrown until the Model property is accessed.
            Assert.Collection(
                BufferEntries(enumerator).OrderBy(e => e.Key),
                entry =>
                {
                    Assert.Equal("prefix.Age", entry.Key);
                    Assert.Equal(23, entry.Model);
                    Assert.Same(metadata.Properties["Age"], entry.Metadata);
                },
                entry =>
                {
                    Assert.Equal("prefix.Id", entry.Key);
                    Assert.Throws<NullReferenceException>(() => entry.Model);
                    Assert.Same(metadata.Properties["Id"], entry.Metadata);
                },
                entry =>
                {
                    Assert.Equal("prefix.Name", entry.Key);
                    Assert.Throws<NullReferenceException>(() => entry.Model);
                    Assert.Same(metadata.Properties["Name"], entry.Metadata);
                });
        }

        private List<ValidationEntry> BufferEntries(IEnumerator<ValidationEntry> enumerator)
        {
            var entries = new List<ValidationEntry>();
            while (enumerator.MoveNext())
            {
                entries.Add(enumerator.Current);
            }

            return entries;
        }

        private class Person
        {
            public int Id { get; set; }

            public int Age { get; set; }

            public string Name { get; set; }
        }

        private class LazyPerson
        {
            private string _string;

            public LazyPerson(string input)
            {
                _string = input;
            }

            public int Id => _string.Length;

            public int Age => 23;

            public string Name => _string.Substring(3, 5);
        }
    }
}
