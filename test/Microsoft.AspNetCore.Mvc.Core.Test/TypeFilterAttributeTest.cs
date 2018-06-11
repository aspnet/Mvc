// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Core.Test
{
    public class TypeFilterAttributeTest
    {
        [Fact]
        public void ThrowsException_OnInstantiating_WithAFilterFactoryType()
        {
            // Arrange
            var implementationType = typeof(FilterFactoryImplementingType);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(
                () => new TypeFilterAttribute(implementationType));
            Assert.Equal(
                $"Cannot use a filter factory type '{implementationType}' from within another" +
                $" filter factory type '{typeof(TypeFilterAttribute)}'.",
                exception.Message);
        }

        [Theory]
        [InlineData(typeof(JustFilterMetadataType))]
        [InlineData(typeof(TestAuthorizationFilter))]
        public void DoesNotThrow_OnInstantiating_WithNonFilterFactoryType(Type implementationType)
        {
            // Act
            var typeFilter = new TypeFilterAttribute(implementationType);

            // Assert
            Assert.Same(implementationType, typeFilter.ImplementationType);
        }

        [Fact]
        public void ThrowsException_WhenProvidedType_IsNotAFilter()
        {
            // Arrange
            var implementationType = typeof(NotFilter);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(
                () => new TypeFilterAttribute(implementationType));
            Assert.Equal(
                $"The type '{implementationType}' provided to '{typeof(TypeFilterAttribute)}' must implement '{typeof(IFilterMetadata)}'.",
                exception.Message);
        }

        private class FilterFactoryImplementingType : IFilterFactory
        {
            public bool IsReusable => throw new NotImplementedException();

            public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
            {
                throw new NotImplementedException();
            }
        }

        private class JustFilterMetadataType : IFilterMetadata
        {
        }

        private class TestAuthorizationFilter : IAsyncAuthorizationFilter
        {
            public Task OnAuthorizationAsync(AuthorizationFilterContext context)
            {
                throw new NotImplementedException();
            }
        }

        private class NotFilter { }
    }
}
