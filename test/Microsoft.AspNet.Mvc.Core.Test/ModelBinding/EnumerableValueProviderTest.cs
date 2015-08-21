// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public abstract class EnumerableValueProviderTest
    {
        private static readonly IDictionary<string, string[]> _backingStore = new Dictionary<string, string[]>
        {
            { "some", new[] { "someValue1", "someValue2" } },
            { "null_value", null },
            { "prefix.name", new[] { "someOtherValue" } },
            { "prefix.null_value", null },
            { "prefix.property1.property", null },
            { "prefix.property2[index]", null },
            { "prefix[index1]", null },
            { "prefix[index1].property1", null },
            { "prefix[index1].property2", null },
            { "prefix[index2].property", null },
            { "[index]", null },
            { "[index].property", null },
            { "[index][anotherIndex]", null },
        };

        [Fact]
        public void ContainsPrefix_WithEmptyCollection_ReturnsFalseForEmptyPrefix()
        {
            // Arrange
            var backingStore = new Dictionary<string, string[]>();
            var valueProvider = GetEnumerableValueProvider(BindingSource.Query, backingStore, culture: null);

            // Act
            var result = valueProvider.ContainsPrefix(string.Empty);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ContainsPrefix_WithNonEmptyCollection_ReturnsTrueForEmptyPrefix()
        {
            // Arrange
            var valueProvider = GetEnumerableValueProvider(BindingSource.Query, _backingStore, culture: null);

            // Act
            var result = valueProvider.ContainsPrefix(string.Empty);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("some")]
        [InlineData("prefix")]
        [InlineData("prefix.name")]
        [InlineData("[index]")]
        [InlineData("prefix[index1]")]
        public void ContainsPrefix_WithNonEmptyCollection_ReturnsTrueForKnownPrefixes(string prefix)
        {
            // Arrange
            var valueProvider = GetEnumerableValueProvider(BindingSource.Query, _backingStore, culture: null);

            // Act & Assert
            Assert.True(valueProvider.ContainsPrefix(prefix));
        }

        [Fact]
        public void ContainsPrefix_WithNonEmptyCollection_ReturnsFalseForUnknownPrefix()
        {
            // Arrange
            var valueProvider = GetEnumerableValueProvider(BindingSource.Query, _backingStore, culture: null);

            // Act
            var result = valueProvider.ContainsPrefix("biff");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetKeysFromPrefix_EmptyPrefix_ReturnsAllPrefixes()
        {
            // Arrange
            var expected = new Dictionary<string, string>
            {
                { "index", "[index]" },
                { "null_value", "null_value" },
                { "prefix", "prefix" },
                { "some", "some" },
            };
            var valueProvider = GetEnumerableValueProvider(BindingSource.Query, _backingStore, culture: null);

            // Act
            var result = valueProvider.GetKeysFromPrefix(string.Empty);

            // Assert
            Assert.Equal(expected, result.OrderBy(kvp => kvp.Key));
        }

        [Fact]
        public void GetKeysFromPrefix_UnknownPrefix_ReturnsEmptyDictionary()
        {
            // Arrange
            var valueProvider = GetEnumerableValueProvider(BindingSource.Query, _backingStore, culture: null);

            // Act
            var result = valueProvider.GetKeysFromPrefix("abc");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetKeysFromPrefix_KnownPrefix_ReturnsMatchingItems()
        {
            // Arrange
            var expected = new Dictionary<string, string>
            {
                { "name", "prefix.name" },
                { "null_value", "prefix.null_value" },
                { "property1", "prefix.property1" },
                { "property2", "prefix.property2" },
                { "index1", "prefix[index1]" },
                { "index2", "prefix[index2]" },
            };
            var valueProvider = GetEnumerableValueProvider(BindingSource.Query, _backingStore, culture: null);

            // Act
            var result = valueProvider.GetKeysFromPrefix("prefix");

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetKeysFromPrefix_IndexPrefix_ReturnsMatchingItems()
        {
            // Arrange
            var expected = new Dictionary<string, string>
            {
                { "property", "[index].property" },
                { "anotherIndex", "[index][anotherIndex]" }
            };
            var valueProvider = GetEnumerableValueProvider(BindingSource.Query, _backingStore, culture: null);

            // Act
            var result = valueProvider.GetKeysFromPrefix("[index]");

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetValue_SingleValue()
        {
            // Arrange
            var culture = new CultureInfo("fr-FR");
            var valueProvider = GetEnumerableValueProvider(BindingSource.Query, _backingStore, culture);

            // Act
            var result = valueProvider.GetValue("prefix.name");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("someOtherValue", (string)result);
            Assert.Equal(culture, result.Culture);
        }

        [Fact]
        public void GetValue_MultiValue()
        {
            // Arrange
            var culture = new CultureInfo("fr-FR");
            var valueProvider = GetEnumerableValueProvider(BindingSource.Query, _backingStore, culture);

            // Act
            var result = valueProvider.GetValue("some");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(new[] { "someValue1", "someValue2" }, result.Values);
            Assert.Equal("someValue1,someValue2", (string)result);
            Assert.Equal(culture, result.Culture);
        }

        [Theory]
        [InlineData("null_value")]
        [InlineData("prefix.null_value")]
        public void GetValue_NullValue(string key)
        {
            // Arrange
            var culture = new CultureInfo("fr-FR");
            var valueProvider = GetEnumerableValueProvider(BindingSource.Query, _backingStore, culture);

            // Act
            var result = valueProvider.GetValue(key);

            // Assert
            Assert.Equal(ValueProviderResult.None, result);
        }

        [Fact]
        public void GetValue_NullMultipleValue()
        {
            // Arrange
            var backingStore = new Dictionary<string, string[]>
            {
                { "key", new string[] { null, null, "value" } },
            };
            var culture = new CultureInfo("fr-FR");
            var valueProvider = GetEnumerableValueProvider(BindingSource.Query, backingStore, culture);

            // Act
            var result = valueProvider.GetValue("key");

            // Assert
            Assert.Equal(new[] { null, null, "value" }, result.Values);
            Assert.Equal(",,value", (string)result);
        }

        [Fact]
        public void GetValue_ReturnsNullIfKeyNotFound()
        {
            // Arrange
            var valueProvider = GetEnumerableValueProvider(BindingSource.Query, _backingStore, culture: null);

            // Act
            var result = valueProvider.GetValue("prefix");

            // Assert
            Assert.Equal(ValueProviderResult.None, result);
        }

        [Fact]
        public void FilterInclude()
        {
            // Arrange
            var provider = GetBindingSourceValueProvider(BindingSource.Query, _backingStore, culture: null);

            var bindingSource = new BindingSource(
                BindingSource.Query.Id,
                displayName: null,
                isGreedy: true,
                isFromRequest: true);

            // Act
            var result = provider.Filter(bindingSource);

            // Assert
            Assert.NotNull(result);
            Assert.Same(result, provider);
        }

        [Fact]
        public void FilterExclude()
        {
            // Arrange
            var provider = GetBindingSourceValueProvider(BindingSource.Query, _backingStore, culture: null);

            var bindingSource = new BindingSource(
                "Test",
                displayName: null,
                isGreedy: true,
                isFromRequest: true);

            // Act
            var result = provider.Filter(bindingSource);

            // Assert
            Assert.Null(result);
        }

        private IBindingSourceValueProvider GetBindingSourceValueProvider(
            BindingSource bindingSource,
            IDictionary<string, string[]> values,
            CultureInfo culture)
        {
            var provider = GetEnumerableValueProvider(bindingSource, values, culture) as IBindingSourceValueProvider;

            // All IEnumerableValueProvider implementations also implement IBindingSourceValueProvider.
            Assert.NotNull(provider);

            return provider;
        }

        protected abstract IEnumerableValueProvider GetEnumerableValueProvider(
            BindingSource bindingSource,
            IDictionary<string, string[]> values,
            CultureInfo culture);
    }
}
