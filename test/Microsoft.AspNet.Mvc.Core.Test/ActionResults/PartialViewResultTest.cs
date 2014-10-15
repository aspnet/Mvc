﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Routing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core
{
    public class PartialViewResultTest
    {
        // The buffer size of the StreamWriter used in ViewResultBase.
        private const int StreamWriterBufferSize = 1024;

        [Fact]
        public async Task ExecuteResultAsync_WritesOutputWithoutBOM()
        {
            // Arrange
            var expected = new byte[] { 97, 98, 99, 100 };

            var view = new Mock<IView>();
            view.Setup(v => v.RenderAsync(It.IsAny<ViewContext>()))
                 .Callback((ViewContext v) =>
                 {
                     view.ToString();
                     v.Writer.Write("abcd");
                 })
                 .Returns(Task.FromResult(0));

            var routeDictionary = new Dictionary<string, object>();

            var viewEngine = new Mock<ICompositeViewEngine>();

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetService(typeof(ICompositeViewEngine)))
                           .Returns(viewEngine.Object);

            var memoryStream = new MemoryStream();
            var response = new Mock<HttpResponse>();
            response.SetupGet(r => r.Body)
                   .Returns(memoryStream);

            var context = new Mock<HttpContext>();
            context.SetupGet(c => c.Response)
                   .Returns(response.Object);
            context.SetupGet(c => c.RequestServices)
                .Returns(serviceProvider.Object);

            var actionContext = new ActionContext(context.Object,
                                                  new RouteData() { Values = routeDictionary },
                                                  new ActionDescriptor());

            viewEngine.Setup(v => v.FindPartialView(actionContext, It.IsAny<string>()))
                      .Returns(ViewEngineResult.Found("MyPartialView", view.Object));


            var viewResult = new PartialViewResult();

            // Act
            await viewResult.ExecuteResultAsync(actionContext);

            // Assert
            Assert.Equal(expected, memoryStream.ToArray());
        }

        [Fact]
        public async Task ExecuteResultAsync_UsesProvidedViewEngine()
        {
            // Arrange
            var expected = new byte[] { 97, 98, 99, 100 };

            var view = new Mock<IView>();
            view.Setup(v => v.RenderAsync(It.IsAny<ViewContext>()))
                 .Callback((ViewContext v) =>
                 {
                     view.ToString();
                     v.Writer.Write("abcd");
                 })
                 .Returns(Task.FromResult(0));

            var routeDictionary = new Dictionary<string, object>();

            var goodViewEngine = new Mock<IViewEngine>();

            var badViewEngine = new Mock<ICompositeViewEngine>(MockBehavior.Strict);

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetService(typeof(ICompositeViewEngine)))
                           .Returns(badViewEngine.Object);

            var memoryStream = new MemoryStream();
            var response = new Mock<HttpResponse>();
            response.SetupGet(r => r.Body)
                   .Returns(memoryStream);

            var context = new Mock<HttpContext>();
            context.SetupGet(c => c.Response)
                   .Returns(response.Object);
            context.SetupGet(c => c.RequestServices)
                .Returns(serviceProvider.Object);

            var actionContext = new ActionContext(context.Object,
                                                  new RouteData() { Values = routeDictionary },
                                                  new ActionDescriptor());

            goodViewEngine.Setup(v => v.FindPartialView(actionContext, It.IsAny<string>()))
                          .Returns(ViewEngineResult.Found("MyPartialView", view.Object));


            var viewResult = new PartialViewResult()
            {
                ViewEngine = goodViewEngine.Object,
            };

            // Act
            await viewResult.ExecuteResultAsync(actionContext);

            // Assert
            Assert.Equal(expected, memoryStream.ToArray());
        }

        public static IEnumerable<object[]> ExecuteResultAsync_DoesNotWriteToResponse_OnceExceptionIsThrownData
        {
            get
            {
                yield return new object[] { 30, 0 };

                if (PlatformHelper.IsMono)
                {
                    // The StreamWriter in Mono buffers 2x the buffer size before flushing.
                    yield return new object[] { StreamWriterBufferSize * 2 + 30, StreamWriterBufferSize };
                }
                else
                {
                    yield return new object[] { StreamWriterBufferSize + 30, StreamWriterBufferSize };
                }
            }
        }

        // The StreamWriter used by ViewResult an internal buffer and consequently anything written to this buffer
        // prior to it filling up will not be written to the underlying stream once an exception is thrown.
        [Theory]
        [MemberData(nameof(ExecuteResultAsync_DoesNotWriteToResponse_OnceExceptionIsThrownData))]
        public async Task ExecuteResultAsync_DoesNotWriteToResponse_OnceExceptionIsThrown(int writtenLength, int expectedLength)
        {
            // Arrange
            var longString = new string('a', writtenLength);

            var routeDictionary = new Dictionary<string, object>();

            var view = new Mock<IView>();
            view.Setup(v => v.RenderAsync(It.IsAny<ViewContext>()))
                 .Callback((ViewContext v) =>
                 {
                     view.ToString();
                     v.Writer.Write(longString);
                     throw new Exception();
                 });

            var viewEngine = new Mock<ICompositeViewEngine>();
            viewEngine.Setup(v => v.FindView(It.IsAny<ActionContext>(), It.IsAny<string>()))
                      .Returns(ViewEngineResult.Found("MyView", view.Object));

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetService(typeof(ICompositeViewEngine)))
                           .Returns(viewEngine.Object);

            var memoryStream = new MemoryStream();
            var response = new Mock<HttpResponse>();
            response.SetupGet(r => r.Body)
                   .Returns(memoryStream);
            var context = new Mock<HttpContext>();
            context.SetupGet(c => c.Response)
                   .Returns(response.Object);
            context.SetupGet(c => c.RequestServices).Returns(serviceProvider.Object);

            var actionContext = new ActionContext(context.Object,
                                                  new RouteData() { Values = routeDictionary },
                                                  new ActionDescriptor());

            var viewResult = new PartialViewResult();

            // Act
            await Record.ExceptionAsync(() => viewResult.ExecuteResultAsync(actionContext));

            // Assert
            Assert.Equal(expectedLength, memoryStream.Length);
        }
    }
}