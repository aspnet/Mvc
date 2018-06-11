// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Core.Test
{
    public class ServiceFilterAttributeTest
    {
        [Fact]
        public void ThrowsException_OnInstantiating_WithAFilterFactoryType()
        {
            // Arrange
            var serviceType = typeof(FilterFactoryImplementingType);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(
                () => new ServiceFilterAttribute(serviceType));
            Assert.Equal(
                $"Cannot use a filter factory type '{serviceType}' from within another" +
                $" filter factory type '{typeof(ServiceFilterAttribute)}'.",
                exception.Message);
        }

        [Theory]
        [InlineData(typeof(JustFilterMetadataType))]
        [InlineData(typeof(TestAuthorizationFilter))]
        public void DoesNotThrow_OnInstantiating_WithNonFilterFactoryType(Type serviceType)
        {
            // Act
            var serviceFilter = new ServiceFilterAttribute(serviceType);

            // Assert
            Assert.Same(serviceType, serviceFilter.ServiceType);
        }

        [Fact]
        public void ThrowsException_WhenProvidedType_IsNotAFilter()
        {
            // Arrange
            var serviceType = typeof(NotFilter);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(
                () => new ServiceFilterAttribute(serviceType));
            Assert.Equal(
                $"The type '{serviceType}' provided to '{typeof(ServiceFilterAttribute)}' must implement '{typeof(IFilterMetadata)}'.",
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
