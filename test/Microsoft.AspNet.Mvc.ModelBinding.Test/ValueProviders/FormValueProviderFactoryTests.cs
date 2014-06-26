// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET45
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Moq;
using Xunit;
using System;
using System.Threading;

namespace Microsoft.AspNet.Mvc.ModelBinding.Test
{
    public class FormValueProviderFactoryTests
    {
        [Fact]
        public void GetValueProvider_ReturnsNull_WhenContentTypeIsNotFormUrlEncoded()
        {
            // Arrange
            var context = CreateContext("some-content-type");
            var factory = new FormValueProviderFactory();
            
            // Act
            var result = factory.GetValueProvider(context);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("application/x-www-form-urlencoded")]
        [InlineData("application/x-www-form-urlencoded;charset=utf-8")]
        public void GetValueProvider_ReturnsValueProviderInstaceWithInvariantCulture(string contentType)
        {
            // Arrange
            var context = CreateContext(contentType);
            var factory = new FormValueProviderFactory();

            // Act
            var result = factory.GetValueProvider(context);

            // Assert
            var valueProvider = Assert.IsType<ReadableStringCollectionValueProvider>(result);
            Assert.Equal(CultureInfo.CurrentCulture, valueProvider.Culture);
        }

        private static ValueProviderFactoryContext CreateContext(string contentType)
        {
            var collection = Mock.Of<IReadableStringCollection>();
            var request = new Mock<HttpRequest>();
            request.Setup(f => f.GetFormAsync(CancellationToken.None)).Returns(Task.FromResult(collection));
            
            var mockHeader = new Mock<IHeaderDictionary>();
            mockHeader.Setup(h => h["Content-Type"]).Returns(contentType);
            request.SetupGet(r => r.Headers).Returns(mockHeader.Object);

            var context = new Mock<HttpContext>();
            context.SetupGet(c => c.Request).Returns(request.Object);

            return new ValueProviderFactoryContext(
                context.Object, 
                new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase));
        }
    }
}
#endif
