﻿using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNet.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.ModelBinding.Test
{
    public class FormValueProviderFactoryTests
    {
        [Fact]
        public async Task GetValueProvider_ReturnsNull_WhenContentTypeIsNotFormUrlEncoded()
        {
            // Arrange
            var requestContext = CreateRequestContext("some-content-type");
            var factory = new FormValueProviderFactory();
            
            // Act
            var result = await factory.GetValueProvider(requestContext);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetValueProvider_ReturnsValueProviderInstaceWithInvariantCulture()
        {
            // Arrange
            var requestContext = CreateRequestContext("application/form-url-encoded");
            var factory = new FormValueProviderFactory();

            // Act
            var result = await factory.GetValueProvider(requestContext);

            // Assert
            var valueProvider = Assert.IsType<ReadableStringCollectionValueProvider>(result);
            Assert.Equal(CultureInfo.CurrentCulture, valueProvider.Culture);
        }

        private static RequestContext CreateRequestContext(string contentType)
        {
            var collection = Mock.Of<IReadableStringCollection>();
            var request = new Mock<HttpRequest>();
            request.Setup(f => f.GetFormAsync()).Returns(Task.FromResult(collection));
            
            var mockHeader = new Mock<IHeaderDictionary>();
            mockHeader.Setup(h => h["Content-Type"]).Returns(contentType);
            request.SetupGet(r => r.Headers).Returns(mockHeader.Object);

            var context = new Mock<HttpContext>();
            context.SetupGet(c => c.Request).Returns(request.Object);
            
            var requestContext = new RequestContext(context.Object, new Dictionary<string, object>());
            return requestContext;
        }
    }
}
