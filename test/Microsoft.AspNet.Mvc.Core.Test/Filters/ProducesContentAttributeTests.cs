// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET45
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;
using Microsoft.AspNet.Routing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Test
{
    public class ProducesContentAttributeTests
    {
        [Fact]
        public async Task ProducesContentAttribute_SetsContentType()
        {
            // Arrange
            var mediaType1 = MediaTypeHeaderValue.Parse("application/json");
            var mediaType2 = MediaTypeHeaderValue.Parse("text/json;charset=utf-8");
            var producesContentAttribute = new ProducesContentAttribute("application/json", "text/json;charset=utf-8");
            var resultExecutingContext = CreateResultExecutingContext(producesContentAttribute);
            var next = new ResultExecutionDelegate(
                            () => Task.FromResult(CreateResultExecutedContext(resultExecutingContext)));

            // Act
            await producesContentAttribute.OnResultExecutionAsync(resultExecutingContext, next);

            // Assert
            var objectResult = resultExecutingContext.Result as ObjectResult;
            Assert.Equal(2, objectResult.ContentTypes.Count);
            ValidateMediaType(mediaType1, objectResult.ContentTypes[0]);
            ValidateMediaType(mediaType2, objectResult.ContentTypes[1]);
        }

        private static void ValidateMediaType(MediaTypeHeaderValue expectedMediaType, MediaTypeHeaderValue actualMediaType)
        {
            Assert.Equal(expectedMediaType.MediaType, actualMediaType.MediaType);
            Assert.Equal(expectedMediaType.MediaSubType, actualMediaType.MediaSubType);
            Assert.Equal(expectedMediaType.Charset, actualMediaType.Charset);
            Assert.Equal(expectedMediaType.MediaTypeRange, actualMediaType.MediaTypeRange);
            Assert.Equal(expectedMediaType.Parameters.Count, actualMediaType.Parameters.Count);
            foreach (var item in expectedMediaType.Parameters)
            {
                Assert.Equal(item.Value, actualMediaType.Parameters[item.Key]);
            }
        }

        private static ResultExecutedContext CreateResultExecutedContext(ResultExecutingContext context)
        {
            return new ResultExecutedContext(context, context.Filters, context.Result);
        }

        private static ResultExecutingContext CreateResultExecutingContext(IFilter filter)
        {
            return new ResultExecutingContext(
                CreateActionContext(),
                new IFilter[] { filter, },
                new ObjectResult("Some Value"));
        }

        private static ActionContext CreateActionContext()
        {
            return new ActionContext(Mock.Of<HttpContext>(), new RouteData(), new ActionDescriptor());
        }
    }
}
#endif