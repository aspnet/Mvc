﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.Tracing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Http.Internal;
using Microsoft.AspNet.Mvc.Abstractions;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Mvc.ViewEngines;
using Microsoft.AspNet.Routing;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.ViewFeatures
{
    public class ViewExecutorTest
    {
        public static TheoryData<MediaTypeHeaderValue, string, string> ViewExecutorSetsContentTypeAndEncodingData
        {
            get
            {
                return new TheoryData<MediaTypeHeaderValue, string, string>
                {
                    {
                        null,
                        null,
                        "text/html; charset=utf-8"
                    },
                    {
                        new MediaTypeHeaderValue("text/foo"),
                        null,
                        "text/foo; charset=utf-8"
                    },
                    {
                        MediaTypeHeaderValue.Parse("text/foo; p1=p1-value"),
                        null,
                        "text/foo; p1=p1-value; charset=utf-8"
                    },
                    {
                        new MediaTypeHeaderValue("text/foo") { Charset = "us-ascii" },
                        null,
                        "text/foo; charset=us-ascii"
                    },
                    {
                        null,
                        "text/bar",
                        "text/bar"
                    },
                    {
                        null,
                        "application/xml; charset=us-ascii",
                        "application/xml; charset=us-ascii"
                    },
                    {
                        null,
                        "Invalid content type",
                        "Invalid content type"
                    },
                    {
                        new MediaTypeHeaderValue("text/foo") { Charset = "us-ascii" },
                        "text/bar",
                        "text/foo; charset=us-ascii"
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(ViewExecutorSetsContentTypeAndEncodingData))]
        public async Task ExecuteAsync_SetsContentTypeAndEncoding(
            MediaTypeHeaderValue contentType,
            string responseContentType,
            string expectedContentType)
        {
            // Arrange
            var view = CreateView(async (v) =>
            {
                await v.Writer.WriteAsync("abcd");
            });

            var context = new DefaultHttpContext();
            var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;
            context.Response.ContentType = responseContentType;

            var actionContext = new ActionContext(
                context,
                new RouteData(),
                new ActionDescriptor());
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider());

            var viewExecutor = CreateViewExecutor();

            // Act
            await viewExecutor.ExecuteAsync(
                actionContext,
                view,
                viewData,
                Mock.Of<ITempDataDictionary>(),
                contentType,
                statusCode: null);

            // Assert
            Assert.Equal(expectedContentType, context.Response.ContentType);
            Assert.Equal("abcd", Encoding.UTF8.GetString(memoryStream.ToArray()));
        }

        [Fact]
        public async Task ExecuteAsync_SetsStatusCode()
        {
            // Arrange
            var view = CreateView(async (v) =>
            {
                await v.Writer.WriteAsync("abcd");
            });

            var context = new DefaultHttpContext();
            var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            var actionContext = new ActionContext(
                context,
                new RouteData(),
                new ActionDescriptor());
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider());

            var viewExecutor = CreateViewExecutor();

            // Act
            await viewExecutor.ExecuteAsync(
                actionContext,
                view,
                viewData,
                Mock.Of<ITempDataDictionary>(),
                contentType: null,
                statusCode: 500);

            // Assert
            Assert.Equal(500, context.Response.StatusCode);
            Assert.Equal("abcd", Encoding.UTF8.GetString(memoryStream.ToArray()));
        }

        [Fact]
        public async Task ExecuteAsync_WritesTelemetry()
        {
            // Arrange
            var view = CreateView(async (v) =>
            {
                await v.Writer.WriteAsync("abcd");
            });

            var context = new DefaultHttpContext();
            var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            var actionContext = new ActionContext(
                context,
                new RouteData(),
                new ActionDescriptor());
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider());

            var adapter = new TestTelemetryListener();

            var telemetryListener = new TelemetryListener("Test");
            telemetryListener.SubscribeWithAdapter(adapter);

            var viewExecutor = CreateViewExecutor(telemetryListener);

            // Act
            await viewExecutor.ExecuteAsync(
                actionContext,
                view,
                viewData,
                Mock.Of<ITempDataDictionary>(),
                contentType: null,
                statusCode: null);

            // Assert
            Assert.Equal("abcd", Encoding.UTF8.GetString(memoryStream.ToArray()));

            Assert.NotNull(adapter.BeforeView?.View);
            Assert.NotNull(adapter.BeforeView?.ViewContext);
            Assert.NotNull(adapter.AfterView?.View);
            Assert.NotNull(adapter.AfterView?.ViewContext);
        }

        [Fact]
        public async Task ExecuteAsync_DoesNotWriteToResponse_OnceExceptionIsThrown()
        {
            // Arrange
            var expectedLength = 0;

            var view = new Mock<IView>();
            view.Setup(v => v.RenderAsync(It.IsAny<ViewContext>()))
                 .Callback((ViewContext v) =>
                 {
                     throw new Exception();
                 });

            var context = new DefaultHttpContext();
            var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            var actionContext = new ActionContext(
                context,
                new RouteData(),
                new ActionDescriptor());
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider());

            var viewExecutor = CreateViewExecutor();

            // Act
            await Record.ExceptionAsync(() => viewExecutor.ExecuteAsync(
                actionContext,
                view.Object,
                viewData,
                Mock.Of<ITempDataDictionary>(),
                contentType: null,
                statusCode: null));

            // Assert
            Assert.Equal(expectedLength, memoryStream.Length);
        }

        [Theory]
        [InlineData(HttpResponseStreamWriter.DefaultBufferSize - 1)]
        [InlineData(HttpResponseStreamWriter.DefaultBufferSize + 1)]
        [InlineData(2 * HttpResponseStreamWriter.DefaultBufferSize + 4)]
        public async Task ExecuteAsync_AsynchronouslyFlushesToTheResponseStream_PriorToDispose(int writeLength)
        {
            // Arrange
            var view = CreateView(async (v) =>
            {
                var text = new string('a', writeLength);
                await v.Writer.WriteAsync(text);
            });

            var context = new DefaultHttpContext();
            var stream = new Mock<Stream>();
            context.Response.Body = stream.Object;

            var actionContext = new ActionContext(
                context,
                new RouteData(),
                new ActionDescriptor());
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider());

            var viewExecutor = CreateViewExecutor();

            // Act
            await viewExecutor.ExecuteAsync(
                actionContext,
                view,
                viewData,
                Mock.Of<ITempDataDictionary>(),
                contentType: null,
                statusCode: null);

            // Assert
            stream.Verify(s => s.FlushAsync(It.IsAny<CancellationToken>()), Times.Once());
            stream.Verify(s => s.Write(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never());
        }

        private IView CreateView(Func<ViewContext, Task> action)
        {
            var view = new Mock<IView>(MockBehavior.Strict);
            view
                .Setup(v => v.RenderAsync(It.IsAny<ViewContext>()))
                .Returns(action);

            return view.Object;
        }

        private ViewExecutor CreateViewExecutor(TelemetryListener listener = null)
        {
            if (listener == null)
            {
                listener = new TelemetryListener("Test");
            }

            return new ViewExecutor(
                new TestOptionsManager<MvcViewOptions>(),
                new TestHttpResponseStreamWriterFactory(),
                new Mock<ICompositeViewEngine>(MockBehavior.Strict).Object,
                listener);
        }
    }
}