// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if DNX451
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Features.Internal;
using Moq;
using Xunit;

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
        [InlineData("multipart/form-data; boundary=----WebKitFormBoundarymx2fSWqWSd0OxQqq")]
        [InlineData("multipart/form-data; boundary=----WebKitFormBoundarymx2fSWqWSd0OxQqq; charset=utf-8")]
        public void GetValueProvider_ReturnsValueProviderInstanceWithInvariantCulture(string contentType)
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
            var collection = Mock.Of<IFormCollection>();
            var request = new Mock<HttpRequest>();
            request.Setup(f => f.ReadFormAsync(CancellationToken.None)).Returns(Task.FromResult(collection));
            request.SetupGet(r => r.ContentType).Returns(contentType);
            request.SetupGet(r => r.HasFormContentType).Returns(new FormFeature(request.Object).HasFormContentType);

            var context = new Mock<HttpContext>();
            context.SetupGet(c => c.Request).Returns(request.Object);

            return new ValueProviderFactoryContext(
                context.Object,
                new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase));
        }
    }
}
#endif
