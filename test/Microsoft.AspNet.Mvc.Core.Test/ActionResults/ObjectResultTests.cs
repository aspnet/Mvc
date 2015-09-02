// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Features;
using Microsoft.AspNet.Http.Internal;
using Microsoft.AspNet.Mvc.Actions;
using Microsoft.AspNet.Mvc.Formatters;
using Microsoft.AspNet.Mvc.Formatters.Xml;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Testing.xunit;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.ActionResults
{
    public class ObjectResultTests
    {
        public static IEnumerable<object[]> ContentTypes
        {
            get
            {
                var contentTypes = new string[]
                        {
                            "text/plain",
                            "text/xml",
                            "application/json",
                        };

                // Empty accept header, should select based on contentTypes.
                yield return new object[] { contentTypes, "", "application/json; charset=utf-8" };

                // null accept header, should select based on contentTypes.
                yield return new object[] { contentTypes, null, "application/json; charset=utf-8" };

                // No accept Header match with given contentype collection.
                // Should select based on if any formatter supported any content type.
                yield return new object[] { contentTypes, "text/custom", "application/json; charset=utf-8" };

                // Accept Header matches but no formatter supports the accept header.
                // Should select based on if any formatter supported any user provided content type.
                yield return new object[] { contentTypes, "text/xml", "application/json; charset=utf-8" };

                // Filtets out Accept headers with 0 quality and selects the one with highest quality.
                yield return new object[]
                        {
                            contentTypes,
                            "text/plain;q=0.3, text/json;q=0, text/cusotm;q=0.0, application/json;q=0.4",
                            "application/json; charset=utf-8"
                        };
            }
        }

        [Theory]
        [MemberData(nameof(ContentTypes))]
        public async Task ObjectResult_WithMultipleContentTypesAndAcceptHeaders_PerformsContentNegotiation(
            IEnumerable<string> contentTypes, string acceptHeader, string expectedHeader)
        {
            // Arrange
            var expectedContentType = expectedHeader;
            var input = "testInput";
            var stream = new MemoryStream();

            var httpResponse = new Mock<HttpResponse>();
            var tempContentType = string.Empty;
            httpResponse.SetupProperty<string>(o => o.ContentType);
            httpResponse.SetupGet(r => r.Body).Returns(stream);

            var actionContext = CreateMockActionContext(httpResponse.Object, acceptHeader);

            var result = new ObjectResult(input);

            // Set the content type property explicitly.
            result.ContentTypes = contentTypes.Select(contentType => MediaTypeHeaderValue.Parse(contentType)).ToList();
            result.Formatters = new List<IOutputFormatter>
                                            {
                                                new CannotWriteFormatter(),
                                                new JsonOutputFormatter(),
                                            };

            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert
            // should always select the Json Output formatter even though it is second in the list.
            httpResponse.VerifySet(r => r.ContentType = expectedContentType);
        }

        [Fact]
        public void ObjectResult_Create_CallsContentResult_InitializesValue()
        {
            // Arrange
            var input = "testInput";
            var actionContext = CreateMockActionContext();

            // Act
            var result = new ObjectResult(input);

            // Assert
            Assert.Equal(input, result.Value);
        }

        [Fact]
        public async Task NoAcceptAndContentTypeHeaders_406Formatter_DoesNotTakeEffect()
        {
            // Arrange
            var expectedContentType = "application/json; charset=utf-8";

            var input = 123;
            var httpResponse = new DefaultHttpContext().Response;
            httpResponse.Body = new MemoryStream();
            var actionContext = CreateMockActionContext(
                outputFormatters: new IOutputFormatter[]
                {
                    new HttpNotAcceptableOutputFormatter(),
                    new JsonOutputFormatter()
                },
                response: httpResponse,
                requestAcceptHeader: null,
                requestContentType: null,
                requestAcceptCharsetHeader: null);

            var result = new ObjectResult(input);
            result.ContentTypes = new List<MediaTypeHeaderValue>();
            result.ContentTypes.Add(MediaTypeHeaderValue.Parse(expectedContentType));

            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert
            Assert.Equal(expectedContentType, httpResponse.ContentType);
        }

        [Fact]
        public async Task ObjectResult_WithSingleContentType_TheGivenContentTypeIsSelected()
        {
            // Arrange
            var expectedContentType = "application/json; charset=utf-8";

            // non string value.
            var input = 123;
            var httpResponse = new DefaultHttpContext().Response;
            httpResponse.Body = new MemoryStream();
            var actionContext = CreateMockActionContext(httpResponse);

            // Set the content type property explicitly to a single value.
            var result = new ObjectResult(input);
            result.ContentTypes = new List<MediaTypeHeaderValue>();
            result.ContentTypes.Add(MediaTypeHeaderValue.Parse(expectedContentType));

            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert
            Assert.Equal(expectedContentType, httpResponse.ContentType);
        }

        [Fact]
        public async Task ObjectResult_FallsBackOn_FormattersInOptions()
        {
            // Arrange
            var formatter = GetMockFormatter();
            var actionContext = CreateMockActionContext(
                new[] { formatter.Object },
                setupActionBindingContext: false);

            // Set the content type property explicitly to a single value.
            var result = new ObjectResult("someValue");

            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert
            formatter.Verify(o => o.WriteAsync(It.IsAny<OutputFormatterContext>()));
        }

        [Fact]
        public async Task ObjectResult_WithSingleContentType_TheContentTypeIsIgnoredIfTheTypeIsString()
        {
            // Arrange
            var contentType = "application/json;charset=utf-8";
            var expectedContentType = "text/plain; charset=utf-8";

            // string value.
            var input = "1234";
            var httpResponse = GetMockHttpResponse();
            var actionContext = CreateMockActionContext(httpResponse.Object);

            // Set the content type property explicitly to a single value.
            var result = new ObjectResult(input);
            result.ContentTypes = new List<MediaTypeHeaderValue>();
            result.ContentTypes.Add(MediaTypeHeaderValue.Parse(contentType));

            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert
            httpResponse.VerifySet(r => r.ContentType = expectedContentType);
        }

        [Fact]
        public async Task ObjectResult_MultipleContentTypes_PicksFirstFormatterWhichSupportsAnyOfTheContentTypes()
        {
            // Arrange
            var expectedContentType = "application/json; charset=utf-8";
            var input = "testInput";
            var httpResponse = GetMockHttpResponse();
            var actionContext = CreateMockActionContext(httpResponse.Object, requestAcceptHeader: null);
            var result = new ObjectResult(input);

            // It should not select TestOutputFormatter,
            // This is because it should accept the first formatter which supports any of the two contentTypes.
            var contentTypes = new[] { "application/custom", "application/json" };

            // Set the content type and the formatters property explicitly.
            result.ContentTypes = contentTypes.Select(contentType => MediaTypeHeaderValue.Parse(contentType))
                                              .ToList();
            result.Formatters = new List<IOutputFormatter>
                                    {
                                        new CannotWriteFormatter(),
                                        new JsonOutputFormatter(),
                                    };
            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert
            // Asserts that content type is not text/custom.
            httpResponse.VerifySet(r => r.ContentType = expectedContentType);
        }

        [Fact]
        public async Task ObjectResult_MultipleFormattersSupportingTheSameContentType_SelectsTheFirstFormatterInList()
        {
            // Arrange
            var input = "testInput";
            var stream = new MemoryStream();

            var httpResponse = GetMockHttpResponse();
            var actionContext = CreateMockActionContext(httpResponse.Object, requestAcceptHeader: null);
            var result = new ObjectResult(input);

            // It should select the mock formatter as that is the first one in the list.
            var contentTypes = new[] { "application/json", "text/custom" };
            var mediaTypeHeaderValue = MediaTypeHeaderValue.Parse("text/custom");

            // Get a  mock formatter which supports everything.
            var mockFormatter = GetMockFormatter();

            result.ContentTypes = contentTypes.Select(contentType => MediaTypeHeaderValue.Parse(contentType)).ToList();
            result.Formatters = new List<IOutputFormatter>
                                        {
                                            mockFormatter.Object,
                                            new JsonOutputFormatter(),
                                            new CannotWriteFormatter()
                                        };
            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert
            // Verify that mock formatter was chosen.
            mockFormatter.Verify(o => o.WriteAsync(It.IsAny<OutputFormatterContext>()));
        }

        [Fact]
        public async Task ObjectResult_NoContentTypeSetWithAcceptHeaders_PicksFormatterOnAcceptHeaders()
        {
            // Arrange
            var expectedContentType = "application/json; charset=utf-8";
            var input = "testInput";
            var stream = new MemoryStream();

            var httpResponse = GetMockHttpResponse();
            var actionContext =
                CreateMockActionContext(httpResponse.Object,
                                        requestAcceptHeader: "text/custom;q=0.1,application/json;q=0.9",
                                        requestContentType: "application/custom");
            var result = new ObjectResult(input);

            // Set more than one formatters. The test output formatter throws on write.
            result.Formatters = new List<IOutputFormatter>
                                    {
                                        new CannotWriteFormatter(),
                                        new JsonOutputFormatter(),
                                    };

            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert
            // Asserts that content type is not text/custom. i.e the formatter is not TestOutputFormatter.
            httpResponse.VerifySet(r => r.ContentType = expectedContentType);
        }

        [Fact]
        public async Task ObjectResult_NoContentTypeSetWithNoAcceptHeaders_PicksFormatterOnRequestContentType()
        {
            // Arrange
            var stream = new MemoryStream();
            var expectedContentType = "application/json; charset=utf-8";
            var httpResponse = new Mock<HttpResponse>();
            httpResponse.SetupProperty<string>(o => o.ContentType);
            httpResponse.SetupGet(r => r.Body).Returns(stream);

            var actionContext = CreateMockActionContext(httpResponse.Object,
                                                        requestAcceptHeader: null,
                                                        requestContentType: "application/json");
            var input = "testInput";
            var result = new ObjectResult(input);

            // Set more than one formatters. The test output formatter throws on write.
            result.Formatters = new List<IOutputFormatter>
                                    {
                                        new CannotWriteFormatter(),
                                        new JsonOutputFormatter(),
                                    };
            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert
            // Asserts that content type is not text/custom.
            httpResponse.VerifySet(r => r.ContentType = expectedContentType);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("application/xml")]
        [InlineData("application/custom")]
        [InlineData("application/xml;q=1, application/custom;q=0.8")]
        public void SelectFormatter_WithNoMatchingAcceptHeadersAndRequestContentType_PicksFormatterBasedOnObjectType
            (string acceptHeader)
        {
            // For no accept headers,
            // can write is called twice once for the request Content-Type and once for the type match pass.
            // For each additional accept header, it is called once.
            // Arrange
            var acceptHeaderCollection = string.IsNullOrEmpty(acceptHeader) ?
                null : MediaTypeHeaderValue.ParseList(new[] { acceptHeader }).ToArray();
            var stream = new MemoryStream();
            var httpResponse = new Mock<HttpResponse>();
            httpResponse.SetupProperty<string>(o => o.ContentType);
            httpResponse.SetupGet(r => r.Body).Returns(stream);

            var actionContext = CreateMockActionContext(httpResponse.Object,
                                                        requestAcceptHeader: acceptHeader,
                                                        requestContentType: "application/xml");
            var input = "testInput";
            var result = new ObjectResult(input);
            var mockCountingFormatter = new Mock<IOutputFormatter>();

            var context = new OutputFormatterContext()
            {
                HttpContext = actionContext.HttpContext,
                Object = input,
                DeclaredType = typeof(string)
            };
            var mockCountingSupportedContentType = MediaTypeHeaderValue.Parse("application/text");
            mockCountingFormatter.Setup(o => o.CanWriteResult(context,
                                           It.Is<MediaTypeHeaderValue>(mth => mth == null)))
                                .Returns(true);
            mockCountingFormatter.Setup(o => o.CanWriteResult(context, mockCountingSupportedContentType))
                                 .Returns(true);

            // Set more than one formatters. The test output formatter throws on write.
            result.Formatters = new List<IOutputFormatter>
                                    {
                                        new CannotWriteFormatter(),
                                        mockCountingFormatter.Object,
                                    };

            // Act
            var formatter = result.SelectFormatter(context, result.Formatters);

            // Assert
            Assert.Equal(mockCountingFormatter.Object, formatter);
            mockCountingFormatter.Verify(v => v.CanWriteResult(context, null), Times.Once());

            // CanWriteResult is invoked for the following cases:
            // 1. For each accept header present
            // 2. Request Content-Type
            // 3. Type based match
            var callCount = (acceptHeaderCollection == null ? 0 : acceptHeaderCollection.Count()) + 2;
            mockCountingFormatter.Verify(v => v.CanWriteResult(context,
                                              It.IsNotIn<MediaTypeHeaderValue>(mockCountingSupportedContentType)),
                                              Times.Exactly(callCount));
        }

        [Fact]
        public async Task
            ObjectResult_NoContentTypeSetWithNoAcceptHeadersAndNoRequestContentType_PicksFirstFormatterWhichCanWrite()
        {
            // Arrange
            var stream = new MemoryStream();
            var expectedContentType = "application/json; charset=utf-8";
            var httpResponse = new Mock<HttpResponse>();
            httpResponse.SetupProperty<string>(o => o.ContentType);
            httpResponse.SetupGet(r => r.Body).Returns(stream);

            var actionContext = CreateMockActionContext(httpResponse.Object,
                                                        requestAcceptHeader: null,
                                                        requestContentType: null);
            var input = "testInput";
            var result = new ObjectResult(input);

            // Set more than one formatters. The test output formatter throws on write.
            result.Formatters = new List<IOutputFormatter>
                                    {
                                        new CannotWriteFormatter(),
                                        new JsonOutputFormatter(),
                                    };
            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert
            // Asserts that content type is not text/custom.
            httpResponse.VerifySet(r => r.ContentType = expectedContentType);
        }

        [Fact]
        public async Task ObjectResult_NoFormatterFound_Returns406()
        {
            // Arrange
            var stream = new MemoryStream();
            var httpResponse = new Mock<HttpResponse>();
            httpResponse.SetupProperty<string>(o => o.ContentType);
            httpResponse.SetupGet(r => r.Body).Returns(stream);

            var actionContext = CreateMockActionContext(httpResponse.Object,
                                                        requestAcceptHeader: null,
                                                        requestContentType: null);
            var input = "testInput";
            var result = new ObjectResult(input);

            // Set more than one formatters. The test output formatter throws on write.
            result.Formatters = new List<IOutputFormatter>
                                    {
                                        new CannotWriteFormatter(),
                                    };
            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert
            // Asserts that content type is not text/custom.
            httpResponse.VerifySet(r => r.StatusCode = StatusCodes.Status406NotAcceptable);
        }

        [Fact]
        public async Task ObjectResult_Execute_CallsContentResult_SetsContent()
        {
            // Arrange
            var expectedContentType = "text/plain; charset=utf-8";
            var input = "testInput";
            var stream = new MemoryStream();

            var httpResponse = new Mock<HttpResponse>();
            httpResponse.SetupProperty<string>(o => o.ContentType);
            httpResponse.SetupGet(r => r.Body).Returns(stream);

            var actionContext = CreateMockActionContext(httpResponse.Object,
                                                        requestAcceptHeader: null,
                                                        requestContentType: null);

            // Act
            var result = new ObjectResult(input);
            await result.ExecuteResultAsync(actionContext);

            // Assert
            httpResponse.VerifySet(r => r.ContentType = expectedContentType);

            // The following verifies the correct Content was written to Body
            Assert.Equal(input.Length, httpResponse.Object.Body.Length);
        }

        [Fact]
        public async Task ObjectResult_Execute_NullContent_SetsStatusCode()
        {
            // Arrange
            var stream = new MemoryStream();
            var expectedStatusCode = StatusCodes.Status201Created;
            var httpResponse = new Mock<HttpResponse>();
            httpResponse.SetupGet(r => r.Body).Returns(stream);

            var formatters = new IOutputFormatter[]
            {
                new HttpNoContentOutputFormatter(),
                new StringOutputFormatter(),
                new JsonOutputFormatter()
            };
            var actionContext = CreateMockActionContext(formatters,
                                                        httpResponse.Object,
                                                        requestAcceptHeader: null,
                                                        requestContentType: null);
            var result = new ObjectResult(null);
            result.StatusCode = expectedStatusCode;

            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert
            httpResponse.VerifySet(r => r.StatusCode = expectedStatusCode);
            Assert.Equal(0, httpResponse.Object.Body.Length);
        }

        [Fact]
        public async Task ObjectResult_Execute_CallsJsonResult_SetsContent()
        {
            // Arrange
            var expectedContentType = "application/json; charset=utf-8";
            var nonStringValue = new { x1 = 10, y1 = "Hello" };
            var httpResponse = Mock.Of<HttpResponse>();
            httpResponse.Body = new MemoryStream();
            var actionContext = CreateMockActionContext(httpResponse);
            var tempStream = new MemoryStream();
            var tempHttpContext = new Mock<HttpContext>();
            var tempHttpResponse = new Mock<HttpResponse>();

            tempHttpResponse.SetupGet(o => o.Body).Returns(tempStream);
            tempHttpResponse.SetupProperty<string>(o => o.ContentType);
            tempHttpContext.SetupGet(o => o.Request).Returns(new DefaultHttpContext().Request);
            tempHttpContext.SetupGet(o => o.Response).Returns(tempHttpResponse.Object);
            var tempActionContext = new ActionContext(tempHttpContext.Object,
                                                      new RouteData(),
                                                      new ActionDescriptor());
            var formatterContext = new OutputFormatterContext()
            {
                HttpContext = tempActionContext.HttpContext,
                Object = nonStringValue,
                DeclaredType = nonStringValue.GetType()
            };
            var formatter = new JsonOutputFormatter();
            formatter.WriteResponseHeaders(formatterContext);
            await formatter.WriteAsync(formatterContext);

            // Act
            var result = new ObjectResult(nonStringValue);
            await result.ExecuteResultAsync(actionContext);

            // Assert
            Assert.Equal(expectedContentType, httpResponse.ContentType);
            Assert.Equal(tempStream.ToArray(), ((MemoryStream)actionContext.HttpContext.Response.Body).ToArray());
        }

        [Theory]
        [InlineData("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
            "application/json; charset=utf-8")] // Chrome & Opera
        [InlineData("text/html, application/xhtml+xml, */*",
            "application/json; charset=utf-8")] //IE
        [InlineData("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
            "application/json; charset=utf-8")] // Firefox & Safari
        [InlineData("*/*", @"application/json; charset=utf-8")]
        [InlineData("text/html,*/*;q=0.8,application/xml;q=0.9",
            "application/json; charset=utf-8")]
        public async Task ObjectResult_SelectDefaultFormatter_OnAllMediaRangeAcceptHeaderMediaType(
            string acceptHeader,
            string expectedResponseContentType)
        {
            // Arrange
            var objectResult = new ObjectResult(new Person() { Name = "John" });
            var outputFormatters = new IOutputFormatter[] {
                new HttpNoContentOutputFormatter(),
                new StringOutputFormatter(),
                new JsonOutputFormatter(),
                new XmlDataContractSerializerOutputFormatter()
            };
            var response = GetMockHttpResponse();

            var actionContext = CreateMockActionContext(
                                    outputFormatters,
                                    response.Object,
                                    requestAcceptHeader: acceptHeader);

            // Act
            await objectResult.ExecuteResultAsync(actionContext);

            // Assert
            response.VerifySet(resp => resp.ContentType = expectedResponseContentType);
        }

        [ConditionalTheory]
        // Mono issue - https://github.com/aspnet/External/issues/18
        [FrameworkSkipCondition(RuntimeFrameworks.Mono)]
        [InlineData("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
            "application/xml; charset=utf-8")] // Chrome & Opera
        [InlineData("text/html, application/xhtml+xml, */*",
            "application/json; charset=utf-8")] //IE
        [InlineData("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
            "application/xml; charset=utf-8")] // Firefox & Safari
        [InlineData("*/*",
            "application/json; charset=utf-8")]
        [InlineData("text/html,*/*;q=0.8,application/xml;q=0.9",
            "application/xml; charset=utf-8")]
        public async Task ObjectResult_PerformsContentNegotiation_OnAllMediaRangeAcceptHeaderMediaType(
            string acceptHeader,
            string expectedResponseContentType)
        {
            // Arrange
            var objectResult = new ObjectResult(new Person() { Name = "John" });
            var outputFormatters = new IOutputFormatter[] {
                new HttpNoContentOutputFormatter(),
                new StringOutputFormatter(),
                new JsonOutputFormatter(),
                new XmlDataContractSerializerOutputFormatter()
            };
            var response = GetMockHttpResponse();

            var actionContext = CreateMockActionContext(
                                    outputFormatters,
                                    response.Object,
                                    requestAcceptHeader: acceptHeader,
                                    respectBrowserAcceptHeader: true);

            // Act
            await objectResult.ExecuteResultAsync(actionContext);

            // Assert
            response.VerifySet(resp => resp.ContentType = expectedResponseContentType);
        }

        [ConditionalTheory]
        // Mono issue - https://github.com/aspnet/External/issues/18
        [FrameworkSkipCondition(RuntimeFrameworks.Mono)]
        [InlineData("application/xml;q=0.9,text/plain;q=0.5", "application/xml; charset=utf-8", false)]
        [InlineData("application/xml;q=0.9,*/*;q=0.5", "application/json; charset=utf-8", false)]
        [InlineData("application/xml;q=0.9,text/plain;q=0.5", "application/xml; charset=utf-8", true)]
        [InlineData("application/xml;q=0.9,*/*;q=0.5", "application/xml; charset=utf-8", true)]
        public async Task ObjectResult_WildcardAcceptMediaType_AndExplicitResponseContentType(
            string acceptHeader,
            string expectedResponseContentType,
            bool respectBrowserAcceptHeader)
        {
            // Arrange
            var objectResult = new ObjectResult(new Person() { Name = "John" });
            objectResult.ContentTypes.Add(MediaTypeHeaderValue.Parse("application/xml"));
            objectResult.ContentTypes.Add(MediaTypeHeaderValue.Parse("application/json"));
            var outputFormatters = new IOutputFormatter[] {
                new HttpNoContentOutputFormatter(),
                new StringOutputFormatter(),
                new JsonOutputFormatter(),
                new XmlDataContractSerializerOutputFormatter()
            };
            var response = GetMockHttpResponse();

            var actionContext = CreateMockActionContext(
                                    outputFormatters,
                                    response.Object,
                                    acceptHeader,
                                    respectBrowserAcceptHeader: respectBrowserAcceptHeader);

            // Act
            await objectResult.ExecuteResultAsync(actionContext);

            // Assert
            response.VerifySet(resp => resp.ContentType = expectedResponseContentType);
        }

        [Theory]
        [InlineData("application/*", "application/*")]
        [InlineData("application/xml, application/*, application/json", "application/*")]
        [InlineData("application/*, application/json", "application/*")]

        [InlineData("*/*", "*/*")]
        [InlineData("application/xml, */*, application/json", "*/*")]
        [InlineData("*/*, application/json", "*/*")]
        public async Task ObjectResult_MatchAllContentType_Throws(string content, string invalidContentType)
        {
            // Arrange
            var contentTypes = content.Split(',');
            var objectResult = new ObjectResult(new Person() { Name = "John" });
            objectResult.ContentTypes = contentTypes.Select(contentType => MediaTypeHeaderValue.Parse(contentType))
                                                    .ToList();
            var actionContext = CreateMockActionContext();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => objectResult.ExecuteResultAsync(actionContext));

            var expectedMessage = string.Format("The content-type '{0}' added in the 'ContentTypes' property is " +
              "invalid. Media types which match all types or match all subtypes are not supported.",
              invalidContentType);
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public async Task ObjectResult_WithStringType_WritesTextPlain_Ignoring406Formatter()
        {
            // Arrange
            var expectedData = "Hello World!";
            var objectResult = new ObjectResult(expectedData);
            var outputFormatters = new IOutputFormatter[]
            {
                new HttpNotAcceptableOutputFormatter(),
                new StringOutputFormatter(),
                new JsonOutputFormatter()
            };

            var response = new Mock<HttpResponse>();
            var responseStream = new MemoryStream();
            response.SetupGet(r => r.Body).Returns(responseStream);

            var actionContext = CreateMockActionContext(
                                    outputFormatters,
                                    response.Object,
                                    requestAcceptHeader: "application/json");

            // Act
            await objectResult.ExecuteResultAsync(actionContext);

            // Assert
            response.VerifySet(r => r.ContentType = "text/plain; charset=utf-8");
            responseStream.Position = 0;
            var actual = new StreamReader(responseStream).ReadToEnd();
            Assert.Equal(expectedData, actual);
        }

        [Fact]
        public async Task ObjectResult_WithSingleContentType_Ignores406Formatter()
        {
            // Arrange
            var objectResult = new ObjectResult(new Person() { Name = "John" });
            objectResult.ContentTypes.Add(new MediaTypeHeaderValue("application/json"));
            var outputFormatters = new IOutputFormatter[]
            {
                new HttpNotAcceptableOutputFormatter(),
                new JsonOutputFormatter()
            };
            var response = new Mock<HttpResponse>();
            var responseStream = new MemoryStream();
            response.SetupGet(r => r.Body).Returns(responseStream);
            var expectedData = "{\"Name\":\"John\"}";

            var actionContext = CreateMockActionContext(
                                    outputFormatters,
                                    response.Object,
                                    requestAcceptHeader: "application/non-existing",
                                    requestContentType: "application/non-existing");

            // Act
            await objectResult.ExecuteResultAsync(actionContext);

            // Assert
            response.VerifySet(r => r.ContentType = "application/json; charset=utf-8");
            responseStream.Position = 0;
            var actual = new StreamReader(responseStream).ReadToEnd();
            Assert.Equal(expectedData, actual);
        }

        [Fact]
        public async Task ObjectResult_WithMultipleContentTypes_Ignores406Formatter()
        {
            // Arrange
            var objectResult = new ObjectResult(new Person() { Name = "John" });
            objectResult.ContentTypes.Add(new MediaTypeHeaderValue("application/foo"));
            objectResult.ContentTypes.Add(new MediaTypeHeaderValue("application/json"));
            var outputFormatters = new IOutputFormatter[]
            {
                new HttpNotAcceptableOutputFormatter(),
                new JsonOutputFormatter()
            };
            var response = new Mock<HttpResponse>();
            var responseStream = new MemoryStream();
            response.SetupGet(r => r.Body).Returns(responseStream);
            var expectedData = "{\"Name\":\"John\"}";

            var actionContext = CreateMockActionContext(
                                    outputFormatters,
                                    response.Object,
                                    requestAcceptHeader: "application/non-existing",
                                    requestContentType: "application/non-existing");

            // Act
            await objectResult.ExecuteResultAsync(actionContext);

            // Assert
            response.VerifySet(r => r.ContentType = "application/json; charset=utf-8");
            responseStream.Position = 0;
            var actual = new StreamReader(responseStream).ReadToEnd();
            Assert.Equal(expectedData, actual);
        }

        [Fact]
        public async Task ObjectResult_WithStream_DoesNotSetContentType_IfNotProvided()
        {
            // Arrange
            var objectResult = new ObjectResult(new MemoryStream(Encoding.UTF8.GetBytes("Name=James")));
            var outputFormatters = new IOutputFormatter[]
            {
                new StreamOutputFormatter(),
                new JsonOutputFormatter()
            };
            var response = new Mock<HttpResponse>();
            var responseStream = new MemoryStream();
            response.SetupGet(r => r.Body).Returns(responseStream);
            var expectedData = "Name=James";

            var actionContext = CreateMockActionContext(
                                    outputFormatters,
                                    response.Object,
                                    requestAcceptHeader: null,
                                    requestContentType: null);

            // Act
            await objectResult.ExecuteResultAsync(actionContext);

            // Assert
            response.VerifySet(r => r.ContentType = It.IsAny<string>(), Times.Never());
            responseStream.Position = 0;
            var actual = new StreamReader(responseStream).ReadToEnd();
            Assert.Equal(expectedData, actual);
        }

        [Fact]
        public async Task ObjectResult_WithStream_SetsExplicitContentType()
        {
            // Arrange
            var objectResult = new ObjectResult(new MemoryStream(Encoding.UTF8.GetBytes("Name=James")));
            objectResult.ContentTypes.Add(new MediaTypeHeaderValue("application/foo"));
            var outputFormatters = new IOutputFormatter[]
            {
                new StreamOutputFormatter(),
                new JsonOutputFormatter()
            };
            var response = new Mock<HttpResponse>();
            var responseStream = new MemoryStream();
            response.SetupGet(r => r.Body).Returns(responseStream);
            var expectedData = "Name=James";

            var actionContext = CreateMockActionContext(
                                    outputFormatters,
                                    response.Object,
                                    requestAcceptHeader: "application/json",
                                    requestContentType: null);

            // Act
            await objectResult.ExecuteResultAsync(actionContext);

            // Assert
            response.VerifySet(r => r.ContentType = "application/foo");
            responseStream.Position = 0;
            var actual = new StreamReader(responseStream).ReadToEnd();
            Assert.Equal(expectedData, actual);
        }

        private static ActionContext CreateMockActionContext(
            HttpResponse response = null,
            string requestAcceptHeader = "application/*",
            string requestContentType = "application/json",
            string requestAcceptCharsetHeader = "",
            bool respectBrowserAcceptHeader = false)
        {
            var formatters = new IOutputFormatter[] { new StringOutputFormatter(), new JsonOutputFormatter() };

            return CreateMockActionContext(
                                            formatters,
                                            response: response,
                                            requestAcceptHeader: requestAcceptHeader,
                                            requestContentType: requestContentType,
                                            requestAcceptCharsetHeader: requestAcceptCharsetHeader,
                                            respectBrowserAcceptHeader: respectBrowserAcceptHeader);
        }

        private static ActionContext CreateMockActionContext(
            IEnumerable<IOutputFormatter> outputFormatters,
            HttpResponse response = null,
            string requestAcceptHeader = "application/*",
            string requestContentType = "application/json",
            string requestAcceptCharsetHeader = "",
            bool respectBrowserAcceptHeader = false,
            bool setupActionBindingContext = true)
        {
            var httpContext = new Mock<HttpContext>();
            if (response != null)
            {
                httpContext.Setup(o => o.Response).Returns(response);
            }

            var content = "{name: 'Person Name', Age: 'not-an-age'}";
            var contentBytes = Encoding.UTF8.GetBytes(content);

            var request = new DefaultHttpContext().Request;
            request.Headers["Accept-Charset"] = requestAcceptCharsetHeader;
            request.Headers["Accept"] = requestAcceptHeader;
            request.ContentType = requestContentType;
            request.Body = new MemoryStream(contentBytes);

            httpContext.Setup(o => o.Features).Returns(new FeatureCollection());
            httpContext.Setup(o => o.Request).Returns(request);
            httpContext.Setup(o => o.RequestServices).Returns(GetServiceProvider());

            var optionsAccessor = new TestOptionsManager<MvcOptions>();
            foreach (var formatter in outputFormatters)
            {
                optionsAccessor.Value.OutputFormatters.Add(formatter);
            }

            optionsAccessor.Value.RespectBrowserAcceptHeader = respectBrowserAcceptHeader;
            httpContext.Setup(o => o.RequestServices.GetService(typeof(IOptions<MvcOptions>)))
                .Returns(optionsAccessor);
            httpContext.Setup(o => o.RequestServices.GetService(typeof(ILogger<ObjectResult>)))
                .Returns(new Mock<ILogger<ObjectResult>>().Object);

            ActionBindingContext actionBindingContext = null;
            if (setupActionBindingContext)
            {
                actionBindingContext = new ActionBindingContext { OutputFormatters = outputFormatters.ToList() };
            }

            httpContext.Setup(o => o.RequestServices.GetService(typeof(IActionBindingContextAccessor)))
                    .Returns(new ActionBindingContextAccessor() { ActionBindingContext = actionBindingContext });

            return new ActionContext(httpContext.Object, new RouteData(), new ActionDescriptor());
        }

        private static Mock<HttpResponse> GetMockHttpResponse()
        {
            var stream = new MemoryStream();
            var httpResponse = new Mock<HttpResponse>();
            httpResponse.SetupProperty<string>(o => o.ContentType);
            httpResponse.SetupGet(r => r.Body).Returns(stream);
            return httpResponse;
        }

        private static Mock<CannotWriteFormatter> GetMockFormatter()
        {
            var mockFormatter = new Mock<CannotWriteFormatter>();
            mockFormatter.Setup(o => o.CanWriteResult(It.IsAny<OutputFormatterContext>(),
                                                      It.IsAny<MediaTypeHeaderValue>()))
                         .Returns(true);

            mockFormatter.Setup(o => o.WriteAsync(It.IsAny<OutputFormatterContext>()))
                         .Returns(Task.FromResult<bool>(true))
                         .Verifiable();
            return mockFormatter;
        }

        private static IServiceProvider GetServiceProvider()
        {
            var options = new MvcOptions();
            var optionsAccessor = new Mock<IOptions<MvcOptions>>();
            optionsAccessor.SetupGet(o => o.Value).Returns(options);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddInstance(optionsAccessor.Object);
            return serviceCollection.BuildServiceProvider();
        }

        public class CannotWriteFormatter : IOutputFormatter
        {
            public virtual bool CanWriteResult(OutputFormatterContext context, MediaTypeHeaderValue contentType)
            {
                return false;
            }

            public virtual Task WriteAsync(OutputFormatterContext context)
            {
                throw new NotImplementedException();
            }
        }

        public class Person
        {
            public string Name { get; set; }
        }
    }
}