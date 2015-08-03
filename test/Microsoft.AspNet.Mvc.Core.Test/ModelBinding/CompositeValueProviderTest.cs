// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if DNX451
using System;
#endif
using System.Collections.Generic;
using System.Globalization;
#if DNX451
using System.Threading.Tasks;
using Moq;
using Xunit;
#endif

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class CompositeValueProviderTest : EnumerableValueProviderTest
    {
        protected override IEnumerableValueProvider GetEnumerableValueProvider(
            BindingSource bindingSource,
            IDictionary<string, string[]> values,
            CultureInfo culture)
        {
            var emptyValueProvider =
                new JQueryFormValueProvider(bindingSource, new Dictionary<string, string[]>(), culture);
            var valueProvider = new JQueryFormValueProvider(bindingSource, values, culture);

            return new CompositeValueProvider(new[] { emptyValueProvider, valueProvider });
        }

        protected override void CheckFilterExcludeResult(IValueProvider result)
        {
            // CompositeValueProvider returns an empty instance rather than null. CompositeModelBinder and
            // MutableObjectModelBinder depend on this empty instance.
            var compositeProvider = Assert.IsType<CompositeValueProvider>(result);
            Assert.Empty(compositeProvider);
        }

#if DNX451
        [Fact]
        public async Task GetKeysFromPrefixAsync_ReturnsResultFromFirstValueProviderThatReturnsValues()
        {
            // Arrange
            var provider1 = Mock.Of<IValueProvider>();
            var dictionary = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "prefix-test", "some-value" },
            };
            var provider2 = new Mock<IEnumerableValueProvider>();
            provider2.Setup(p => p.GetKeysFromPrefixAsync("prefix"))
                     .Returns(Task.FromResult<IDictionary<string, string>>(dictionary))
                     .Verifiable();
            var provider = new CompositeValueProvider(new[] { provider1, provider2.Object });

            // Act
            var values = await provider.GetKeysFromPrefixAsync("prefix");

            // Assert
            var result = Assert.Single(values);
            Assert.Equal("prefix-test", result.Key);
            Assert.Equal("some-value", result.Value);
            provider2.Verify();
        }

        [Fact]
        public async Task GetKeysFromPrefixAsync_ReturnsEmptyDictionaryIfNoValueProviderReturnsValues()
        {
            // Arrange
            var provider1 = Mock.Of<IValueProvider>();
            var provider2 = Mock.Of<IValueProvider>();
            var provider = new CompositeValueProvider(new[] { provider1, provider2 });

            // Act
            var values = await provider.GetKeysFromPrefixAsync("prefix");

            // Assert
            Assert.Empty(values);
        }

        public static IEnumerable<object[]> BinderMetadata
        {
            get
            {
                yield return new object[] { new TestValueProviderMetadata() };
                yield return new object[] { new DerivedValueProviderMetadata() };
            }
        }

        [Theory]
        [MemberData(nameof(BinderMetadata))]
        public void FilterReturnsItself_ForAnyClassRegisteredAsGenericParam(IBindingSourceMetadata metadata)
        {
            // Arrange
            var values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            var valueProvider1 = GetMockValueProvider("Test");
            var valueProvider2 = GetMockValueProvider("Unrelated");

            var provider = new CompositeValueProvider(new List<IValueProvider>() { valueProvider1.Object, valueProvider2.Object });

            // Act
            var result = provider.Filter(metadata.BindingSource);

            // Assert
            var valueProvider = Assert.IsType<CompositeValueProvider>(result);
            var filteredProvider = Assert.Single(valueProvider);

            // should not be unrelated metadata.
            Assert.Same(valueProvider1.Object, filteredProvider);
        }

        private Mock<IBindingSourceValueProvider> GetMockValueProvider(string bindingSourceId)
        {
            var valueProvider = new Mock<IBindingSourceValueProvider>(MockBehavior.Strict);

            valueProvider
                .Setup(o => o.Filter(It.Is<BindingSource>(s => s.Id == bindingSourceId)))
                .Returns(valueProvider.Object);

            valueProvider
                .Setup(o => o.Filter(It.Is<BindingSource>(s => s.Id != bindingSourceId)))
                .Returns((IBindingSourceValueProvider)null);

            return valueProvider;
        }

        private class TestValueProviderMetadata : IBindingSourceMetadata
        {
            public BindingSource BindingSource
            {
                get
                {
                    return new BindingSource("Test", displayName: null, isGreedy: true, isFromRequest: true);
                }
            }
        }

        private class DerivedValueProviderMetadata : TestValueProviderMetadata
        {
        }

        private class UnrelatedValueBinderMetadata : IBindingSourceMetadata
        {
            public BindingSource BindingSource
            {
                get
                {
                    return new BindingSource("Unrelated", displayName: null, isGreedy: true, isFromRequest: true);
                }
            }
        }
#endif
    }
}
