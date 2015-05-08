// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Internal;
using Microsoft.AspNet.Mvc.Xml;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class JsonResultTest
    {
        private static readonly byte[] _abcdUTF8Bytes
            = new byte[] { 123, 34, 102, 111, 111, 34, 58, 34, 97, 98, 99, 100, 34, 125 };

        [Fact]
        public async Task ExecuteResultAsync_OptionsFormatter_WithoutBOM()
        {
            // Arrange
            var expected = _abcdUTF8Bytes;

            var optionsFormatters = new List<IOutputFormatter>()
            {
                new XmlDataContractSerializerOutputFormatter(), // This won't be used
                new JsonOutputFormatter(),
            };

            var context = GetHttpContext(optionsFormatters);
            var actionContext = new ActionContext(context, new RouteData(), new ActionDescriptor());

            var result = new JsonResult(new { foo = "abcd" });

            // Act
            await result.ExecuteResultAsync(actionContext);
            var written = GetWrittenBytes(context);

            // Assert
            Assert.Equal(expected, written);
            Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);
        }

        [Fact]
        public async Task ExecuteResultAsync_Null()
        {
            // Arrange
            var expected = _abcdUTF8Bytes;

            var optionsFormatters = new List<IOutputFormatter>()
            {
                new XmlDataContractSerializerOutputFormatter(), // This won't be used
                new JsonOutputFormatter(),
            };

            var context = GetHttpContext(optionsFormatters);
            var actionContext = new ActionContext(context, new RouteData(), new ActionDescriptor());

            var result = new JsonResult(new { foo = "abcd" });

            // Act
            await result.ExecuteResultAsync(actionContext);
            var written = GetWrittenBytes(context);

            // Assert
            Assert.Equal(expected, written);
            Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);
        }

        [Fact]
        public async Task ExecuteResultAsync_OptionsFormatter_Conneg()
        {
            // Arrange
            var expected = _abcdUTF8Bytes;

            var optionsFormatters = new List<IOutputFormatter>()
            {
                new XmlDataContractSerializerOutputFormatter(), // This won't be used
                new JsonOutputFormatter(),
            };

            var context = GetHttpContext(optionsFormatters);
            context.Request.Headers["Accept"] = "text/json";

            var actionContext = new ActionContext(context, new RouteData(), new ActionDescriptor());

            var result = new JsonResult(new { foo = "abcd" });

            // Act
            await result.ExecuteResultAsync(actionContext);
            var written = GetWrittenBytes(context);

            // Assert
            Assert.Equal(expected, written);
            Assert.Equal("text/json; charset=utf-8", context.Response.ContentType);
        }

        [Fact]
        public async Task ExecuteResultAsync_UsesPassedInFormatter()
        {
            // Arrange
            var expected = _abcdUTF8Bytes;

            var context = GetHttpContext();
            var actionContext = new ActionContext(context, new RouteData(), new ActionDescriptor());

            var formatter = new JsonOutputFormatter();
            formatter.SupportedEncodings.Clear();

            // This is UTF-8 WITH BOM
            formatter.SupportedEncodings.Add(Encoding.UTF8);

            var result = new JsonResult(new { foo = "abcd" }, formatter);

            // Act
            await result.ExecuteResultAsync(actionContext);
            var written = GetWrittenBytes(context);

            // Assert
            Assert.Equal(expected, written);
            Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);
        }

        [Fact]
        public async Task ExecuteResultAsync_UsesPassedInFormatter_ContentTypeSpecified()
        {
            // Arrange
            var expected = _abcdUTF8Bytes;

            var context = GetHttpContext();
            var actionContext = new ActionContext(context, new RouteData(), new ActionDescriptor());

            var formatter = new JsonOutputFormatter();
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/hal+json"));

            var result = new JsonResult(new { foo = "abcd" }, formatter);
            result.ContentTypes.Add(MediaTypeHeaderValue.Parse("application/hal+json"));

            // Act
            await result.ExecuteResultAsync(actionContext);
            var written = GetWrittenBytes(context);

            // Assert
            Assert.Equal(expected, written);
            Assert.Equal("application/hal+json; charset=utf-8", context.Response.ContentType);
        }

        // If no formatter in options can match the given content-types, then use the one registered
        // in services
        [Fact]
        public async Task ExecuteResultAsync_UsesGlobalFormatter_IfNoFormatterIsConfigured()
        {
            // Arrange
            var expected = _abcdUTF8Bytes;

            var context = GetHttpContext(enableFallback: true);
            var actionContext = new ActionContext(context, new RouteData(), new ActionDescriptor());

            var result = new JsonResult(new { foo = "abcd" });

            // Act
            await result.ExecuteResultAsync(actionContext);
            var written = GetWrittenBytes(context);

            // Assert
            Assert.Equal(expected, written);
            Assert.Equal("application/json; charset=utf-8", context.Response.ContentType);
        }

        private HttpContext GetHttpContext(
            IReadOnlyList<IOutputFormatter> outputFormatters = null,
            bool enableFallback = false)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            var services = new Mock<IServiceProvider>(MockBehavior.Strict);
            httpContext.RequestServices = services.Object;

            var options = new MvcOptions();
            if (outputFormatters != null)
            {
                foreach (var formatter in outputFormatters)
                {
                    options.OutputFormatters.Add(formatter);
                }
            }

            var optionsAccessor = new Mock<IOptions<MvcOptions>>();
            optionsAccessor.SetupGet(o => o.Options)
                .Returns(options);

            services.Setup(s => s.GetService(typeof(IOptions<MvcOptions>)))
                .Returns(optionsAccessor.Object);
            services.Setup(s => s.GetService(typeof(ILogger<ObjectResult>)))
                .Returns(new Mock<ILogger<ObjectResult>>().Object);

            // This is the ultimate fallback, it will be used if none of the formatters from options
            // work.
            if (enableFallback)
            {
                services
                    .Setup(s => s.GetService(typeof(JsonOutputFormatter)))
                    .Returns(new JsonOutputFormatter());
            }

            return httpContext;
        }

        private byte[] GetWrittenBytes(HttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            return Assert.IsType<MemoryStream>(context.Response.Body).ToArray();
        }
    }
}