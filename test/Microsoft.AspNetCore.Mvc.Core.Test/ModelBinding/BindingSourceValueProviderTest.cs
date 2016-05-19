// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Core;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    public class BindingSourceValueProviderTest
    {
        [Fact]
        public void BindingSourceValueProvider_ThrowsOnNonGreedySource()
        {
            // Arrange
            var expected = Resources.FormatBindingSource_CannotBeGreedy("Test Source", "BindingSourceValueProvider");

            var bindingSource = new BindingSource(
                "Test",
                displayName: "Test Source",
                isGreedy: true,
                isFromRequest: true);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(
                () => new TestableBindingSourceValueProvider(bindingSource));
            Assert.StartsWith(expected, exception.Message);
        }

        [Fact]
        public void BindingSourceValueProvider_ThrowsOnCompositeSource()
        {
            // Arrange
            var expected = Resources.FormatBindingSource_CannotBeComposite("Test Source", "BindingSourceValueProvider");

            var bindingSource = CompositeBindingSource.Create(
                bindingSources: new BindingSource[] { BindingSource.Query, BindingSource.Form },
                displayName: "Test Source");

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(
                () => new TestableBindingSourceValueProvider(bindingSource));
            Assert.StartsWith(expected, exception.Message);
        }

        [Fact]
        public void BindingSourceValueProvider_ReturnsNull_WithNonMatchingSource()
        {
            // Arrange
            var valueProvider = new TestableBindingSourceValueProvider(BindingSource.Query);

            // Act 
            var result = valueProvider.Filter(BindingSource.Body);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void BindingSourceValueProvider_ReturnsSelf_WithMatchingSource()
        {
            // Arrange
            var valueProvider = new TestableBindingSourceValueProvider(BindingSource.Query);

            // Act 
            var result = valueProvider.Filter(BindingSource.Query);

            // Assert
            Assert.Same(valueProvider, result);
        }

        private class TestableBindingSourceValueProvider : BindingSourceValueProvider
        {
            public TestableBindingSourceValueProvider(BindingSource source)
                : base(source)
            {
            }

            public override bool ContainsPrefix(string prefix)
            {
                throw new NotImplementedException();
            }

            public override ValueProviderResult GetValue(string key)
            {
                throw new NotImplementedException();
            }
        }
    }
}