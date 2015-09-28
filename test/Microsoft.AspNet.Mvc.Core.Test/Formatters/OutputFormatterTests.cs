// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Internal;
using Microsoft.Framework.Internal;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Formatters
{
    public class OutputFormatterTests
    {
        public static IEnumerable<object[]> SelectResponseCharacterEncodingData
        {
            get
            {
                // string acceptEncodings, string requestEncoding, string[] supportedEncodings, string expectedEncoding
                yield return new object[] { "", null, new string[] { "utf-8", "utf-16" }, "utf-8" };
                yield return new object[] { "", "utf-16", new string[] { "utf-8", "utf-16" }, "utf-16" };

                yield return new object[] { "utf-8", null, new string[] { "utf-8", "utf-16" }, "utf-8" };
                yield return new object[] { "utf-16", "utf-8", new string[] { "utf-8", "utf-16" }, "utf-16" };
                yield return new object[] { "utf-16; q=0.5", "utf-8", new string[] { "utf-8", "utf-16" }, "utf-16" };

                yield return new object[] { "utf-8; q=0.0", null, new string[] { "utf-8", "utf-16" }, "utf-8" };
                yield return new object[] { "utf-8; q=0.0", "utf-16", new string[] { "utf-8", "utf-16" }, "utf-16" };
                yield return new object[]
                    { "utf-8; q=0.0, utf-16; q=0.0", "utf-16", new string[] { "utf-8", "utf-16" }, "utf-16" };
                yield return new object[]
                    { "utf-8; q=0.0, utf-16; q=0.0", null, new string[] { "utf-8", "utf-16" }, "utf-8" };

                yield return new object[] { "*; q=0.0", null, new string[] { "utf-8", "utf-16" }, "utf-8" };
                yield return new object[] { "*; q=0.0", "utf-16", new string[] { "utf-8", "utf-16" }, "utf-16" };
            }
        }

        [Theory]
        [MemberData(nameof(SelectResponseCharacterEncodingData))]
        public void SelectResponseCharacterEncoding_SelectsEncoding(
            string acceptCharsetHeaders,
            string requestEncoding,
            string[] supportedEncodings,
            string expectedEncoding)
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            var httpRequest = new DefaultHttpContext().Request;
            httpRequest.Headers["Accept-Charset"] = acceptCharsetHeaders;
            httpRequest.ContentType = "application/acceptCharset;charset=" + requestEncoding;
            mockHttpContext.SetupGet(o => o.Request).Returns(httpRequest);

            var formatter = new TestOutputFormatter();
            foreach (string supportedEncoding in supportedEncodings)
            {
                formatter.SupportedEncodings.Add(Encoding.GetEncoding(supportedEncoding));
            }

            var formatterContext = new OutputFormatterContext()
            {
                Object = "someValue",
                HttpContext = mockHttpContext.Object,
                DeclaredType = typeof(string)
            };

            // Act
            var actualEncoding = formatter.SelectCharacterEncoding(formatterContext);

            // Assert
            Assert.Equal(Encoding.GetEncoding(expectedEncoding), actualEncoding);
        }

        [Fact]
        public void WriteResponseContentHeaders_NoSupportedEncodings_NoEncodingIsSet()
        {
            // Arrange
            var formatter = new TestOutputFormatter();

            var testContentType = MediaTypeHeaderValue.Parse("text/json");

            formatter.SupportedEncodings.Clear();
            formatter.SupportedMediaTypes.Clear();
            formatter.SupportedMediaTypes.Add(testContentType);

            var formatterContext = new OutputFormatterContext()
            {
                HttpContext = new DefaultHttpContext(),
            };

            // Act
            formatter.WriteResponseHeaders(formatterContext);

            // Assert
            Assert.Null(formatterContext.SelectedEncoding);
            Assert.Equal(testContentType, formatterContext.SelectedContentType);

            // If we had set an encoding, it would be part of the content type header
            Assert.Equal(testContentType, formatterContext.HttpContext.Response.GetTypedHeaders().ContentType);
        }

        [Fact]
        public void WriteResponseContentHeaders_NoSelectedContentType_SetsOutputFormatterContext()
        {
            // Arrange
            var testFormatter = new DoesNotSetContext();
            var testContentType = MediaTypeHeaderValue.Parse("application/doesNotSetContext");
            var formatterContext = new OutputFormatterContext();
            var mockHttpContext = new Mock<HttpContext>();
            var httpRequest = new DefaultHttpContext().Request;
            mockHttpContext.SetupGet(o => o.Request).Returns(httpRequest);
            mockHttpContext.SetupProperty(o => o.Response.ContentType);
            formatterContext.HttpContext = mockHttpContext.Object;

            // Act
            testFormatter.WriteResponseHeaders(formatterContext);

            // Assert
            Assert.Equal(Encoding.Unicode.WebName, formatterContext.SelectedEncoding.WebName);
            Assert.Equal(Encoding.Unicode, formatterContext.SelectedEncoding);
            Assert.Equal("application/doesNotSetContext; charset=" + Encoding.Unicode.WebName,
                         formatterContext.SelectedContentType.ToString());
        }

        [Fact]
        public async Task WriteResponseHeaders_ClonesMediaType()
        {
            // Arrange
            var formatter = new PngImageFormatter();
            formatter.SupportedMediaTypes.Clear();
            var mediaType = new MediaTypeHeaderValue("image/png");
            formatter.SupportedMediaTypes.Add(mediaType);
            var formatterContext = new OutputFormatterContext();
            formatterContext.HttpContext = new DefaultHttpContext();

            // Act
            await formatter.WriteAsync(formatterContext);

            // Assert
            Assert.NotSame(mediaType, formatterContext.SelectedContentType);
            Assert.Null(mediaType.Charset);
            Assert.Equal("image/png; charset=utf-8", formatterContext.SelectedContentType.ToString());
        }

        [Fact]
        public void CanWriteResult_ForNullContentType_UsesFirstEntryInSupportedContentTypes()
        {
            // Arrange
            var context = new OutputFormatterContext();
            var formatter = new TestOutputFormatter();

            // Act
            var result = formatter.CanWriteResult(context, null);

            // Assert
            Assert.True(result);
            Assert.Equal(formatter.SupportedMediaTypes[0].ToString(), context.SelectedContentType.ToString());
        }

        [Fact]
        public void GetSupportedContentTypes_ReturnsNull_ForUnsupportedType()
        {
            // Arrange
            var formatter = new TypeSpecificFormatter();

            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));

            formatter.SupportedTypes.Add(typeof(int));

            // Act
            var contentTypes = formatter.GetSupportedContentTypes(
                declaredType: typeof(string),
                runtimeType: typeof(string),
                contentType: null);

            // Assert
            Assert.Null(contentTypes);
        }

        [Fact]
        public void CanWrite_ReturnsFalse_ForUnsupportedType()
        {
            // Arrange
            var context = new OutputFormatterContext();
            context.DeclaredType = typeof(string);
            context.Object = "Hello, world!";

            var formatter = new TypeSpecificFormatter();

            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));

            formatter.SupportedTypes.Add(typeof(int));

            // Act
            var result = formatter.CanWriteResult(context, formatter.SupportedMediaTypes[0]);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetSupportedContentTypes_ReturnsAllContentTypes_WithContentTypeNull()
        {
            // Arrange
            var formatter = new TestOutputFormatter();

            formatter.SupportedMediaTypes.Clear();
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/xml"));

            // Act
            var contentTypes = formatter.GetSupportedContentTypes(typeof(int), typeof(int), contentType: null);

            // Assert
            Assert.Equal(2, contentTypes.Count);
            Assert.Single(contentTypes, ct => ct.ToString() == "application/json");
            Assert.Single(contentTypes, ct => ct.ToString() == "application/xml");
        }

        [Fact]
        public void GetSupportedContentTypes_ReturnsMatchingContentTypes_WithContentType()
        {
            // Arrange
            var formatter = new TestOutputFormatter();

            formatter.SupportedMediaTypes.Clear();
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/xml"));

            // Act
            var contentTypes = formatter.GetSupportedContentTypes(
                typeof(int),
                typeof(int),
                contentType: MediaTypeHeaderValue.Parse("application/*"));

            // Assert
            var contentType = Assert.Single(contentTypes);
            Assert.Equal("application/json", contentType.ToString());
        }

        [Fact]
        public void GetSupportedContentTypes_ReturnsMatchingContentTypes_NoMatches()
        {
            // Arrange
            var formatter = new TestOutputFormatter();

            formatter.SupportedMediaTypes.Clear();
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/xml"));

            // Act
            var contentTypes = formatter.GetSupportedContentTypes(
                typeof(int),
                typeof(int),
                contentType: MediaTypeHeaderValue.Parse("application/xml"));

            // Assert
            Assert.Null(contentTypes);
        }

        [Fact]
        public void GetSupportedContentTypes_ReturnsAllContentTypes_ReturnsNullWithNoSupportedContentTypes()
        {
            // Arrange
            var formatter = new TestOutputFormatter();

            // Intentionally empty
            formatter.SupportedMediaTypes.Clear();

            // Act
            var contentTypes = formatter.GetSupportedContentTypes(
                typeof(int),
                typeof(int),
                contentType: null);

            // Assert
            Assert.Null(contentTypes);
        }

        private class TypeSpecificFormatter : OutputFormatter
        {
            public List<Type> SupportedTypes { get; } = new List<Type>();

            protected override bool CanWriteType(Type declaredType, Type runtimeType)
            {
                return SupportedTypes.Contains(declaredType ?? runtimeType);
            }

            public override Task WriteResponseBodyAsync(OutputFormatterContext context)
            {
                throw new NotImplementedException();
            }
        }

        private class TestOutputFormatter : OutputFormatter
        {
            public TestOutputFormatter()
            {
                SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/acceptCharset"));
            }

            public override Task WriteResponseBodyAsync(OutputFormatterContext context)
            {
                return Task.FromResult(true);
            }
        }

        private class DoesNotSetContext : OutputFormatter
        {
            public DoesNotSetContext()
            {
                SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/doesNotSetContext"));
                SupportedEncodings.Add(Encoding.Unicode);
            }

            public override bool CanWriteResult(OutputFormatterContext context, MediaTypeHeaderValue contentType)
            {
                // Do not set the selected media Type.
                // The WriteResponseHeaders should do it for you.
                return true;
            }

            public override Task WriteResponseBodyAsync(OutputFormatterContext context)
            {
                return Task.FromResult(true);
            }
        }

        private class PngImageFormatter : OutputFormatter
        {
            public PngImageFormatter()
            {
                SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("image/png"));
                SupportedEncodings.Add(Encoding.UTF8);
            }

            public override Task WriteResponseBodyAsync([NotNull] OutputFormatterContext context)
            {
                return Task.FromResult(true);
            }
        }
    }
}
