// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if ASPNET50
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.ModelBinding;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class JsonInputFormatterTest
    {

        [Theory]
        [InlineData("application/json", true)]
        [InlineData("application/*", true)]
        [InlineData("*/*", true)]
        [InlineData("text/json", true)]
        [InlineData("text/*", true)]
        [InlineData("text/xml", false)]
        [InlineData("application/xml", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("invalid", false)]
        public void CanRead_ReturnsTrueForAnySupportedContentType(string requestContentType, bool expectedCanRead)
        {
            // Arrange
            var formatter = new JsonInputFormatter();
            var contentBytes = Encoding.UTF8.GetBytes("content");

            var actionContext = GetActionContext(contentBytes, contentType: requestContentType);
            var formatterContext = new InputFormatterContext(actionContext, typeof(string));

            // Act
            var result = formatter.CanRead(formatterContext);

            // Assert
            Assert.Equal(expectedCanRead, result);
        }

        [Fact]
        public void DefaultMediaType_ReturnsApplicationJson()
        {
            // Arrange
            var formatter = new JsonInputFormatter();

            // Act
            var mediaType = formatter.SupportedMediaTypes[0];

            // Assert
            Assert.Equal("application/json", mediaType.ToString());
        }

        public static IEnumerable<object[]> JsonFormatterReadSimpleTypesData
        {
            get
            {
                yield return new object[] { "100", typeof(int), 100 };
                yield return new object[] { "'abcd'", typeof(string), "abcd" };
                yield return new object[] { "'2012-02-01 12:45 AM'", typeof(DateTime),
                                            new DateTime(2012, 02, 01, 00, 45, 00) };
            }
        }

        [Theory]
        [MemberData(nameof(JsonFormatterReadSimpleTypesData))]
        public async Task JsonFormatterReadsSimpleTypes(string content, Type type, object expected)
        {
            // Arrange
            var formatter = new JsonInputFormatter();
            var contentBytes = Encoding.UTF8.GetBytes(content);

            var actionContext = GetActionContext(contentBytes);
            var context = new InputFormatterContext(actionContext, type);

            // Act
            var model = await formatter.ReadAsync(context);

            // Assert
            Assert.Equal(expected, model);
        }

        [Fact]
        public async Task JsonFormatterReadsComplexTypes()
        {
            // Arrange
            var content = "{name: 'Person Name', Age: '30'}";
            var formatter = new JsonInputFormatter();
            var contentBytes = Encoding.UTF8.GetBytes(content);

            var actionContext = GetActionContext(contentBytes);
            var metadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(User));
            var context = new InputFormatterContext(actionContext, metadata.ModelType);

            // Act
            var model = await formatter.ReadAsync(context);

            // Assert
            var userModel = Assert.IsType<User>(model);
            Assert.Equal("Person Name", userModel.Name);
            Assert.Equal(30, userModel.Age);
        }

        [Fact]
        public async Task ReadAsync_ThrowsOnDeserializationErrors()
        {
            // Arrange
            var content = "{name: 'Person Name', Age: 'not-an-age'}";
            var formatter = new JsonInputFormatter();
            var contentBytes = Encoding.UTF8.GetBytes(content);

            var httpContext = GetActionContext(contentBytes);
            var metadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(User));
            var context = new InputFormatterContext(httpContext, metadata.ModelType);

            // Act and Assert
            await Assert.ThrowsAsync<JsonReaderException>(() => formatter.ReadAsync(context));
        }

        [Fact]
        public async Task ReadAsync_AddsModelValidationErrorsToModelState_WhenCaptureErrorsIsSet()
        {
            // Arrange
            var content = "{name: 'Person Name', Age: 'not-an-age'}";
            var formatter = new JsonInputFormatter { CaptureDeserilizationErrors = true };
            var contentBytes = Encoding.UTF8.GetBytes(content);

            var actionContext = GetActionContext(contentBytes);
            var metadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(User));
            var context = new InputFormatterContext(actionContext, metadata.ModelType);

            // Act
            var model = await formatter.ReadAsync(context);

            // Assert
            Assert.Equal("Could not convert string to decimal: not-an-age. Path 'Age', line 1, position 39.",
                         actionContext.ModelState["Age"].Errors[0].Exception.Message);
        }

        [Fact]
        public async Task ReadAsync_UsesTryAddModelValidationErrorsToModelState_WhenCaptureErrorsIsSet()
        {
            // Arrange
            var content = "{name: 'Person Name', Age: 'not-an-age'}";
            var formatter = new JsonInputFormatter { CaptureDeserilizationErrors = true };
            var contentBytes = Encoding.UTF8.GetBytes(content);

            var actionContext = GetActionContext(contentBytes);
            var metadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(User));
            var context = new InputFormatterContext(actionContext, metadata.ModelType);
            actionContext.ModelState.MaxAllowedErrors = 3;
            actionContext.ModelState.AddModelError("key1", "error1");
            actionContext.ModelState.AddModelError("key2", "error2");

            // Act
            var model = await formatter.ReadAsync(context);

            // Assert
            Assert.False(actionContext.ModelState.ContainsKey("age"));
            var error = Assert.Single(actionContext.ModelState[""].Errors);
            Assert.IsType<TooManyModelErrorsException>(error.Exception);
        }

        [Fact]
        public void Creates_SerializerSettings_ByDefault()
        {
            // Arrange
            // Act
            var jsonFormatter = new JsonInputFormatter();

            // Assert
            Assert.NotNull(jsonFormatter.SerializerSettings);
        }

        [Fact]
        public async Task ChangesTo_DefaultSerializerSettings_TakesEffect()
        {
            // Arrange
            // missing password property here
            var contentBytes = Encoding.UTF8.GetBytes("{ \"UserName\" : \"John\"}");

            var jsonFormatter = new JsonInputFormatter() { CaptureDeserilizationErrors = true };
            // by default we ignore missing members, so here explicitly changing it
            jsonFormatter.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Error;

            var actionContext = GetActionContext(contentBytes, "application/json;charset=utf-8");
            var metadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(UserLogin));
            var inputFormatterContext = new InputFormatterContext(actionContext, metadata.ModelType);

            // Act
            var obj = await jsonFormatter.ReadAsync(inputFormatterContext);

            // Assert
            Assert.False(actionContext.ModelState.IsValid);

            var modelErrorMessage = actionContext.ModelState.Values.First().Errors[0].Exception.Message;
            Assert.Contains("Required property 'Password' not found in JSON", modelErrorMessage);
        }

        [Fact]
        public async Task CustomSerializerSettingsObject_TakesEffect()
        {
            // Arrange
            // missing password property here
            var contentBytes = Encoding.UTF8.GetBytes("{ \"UserName\" : \"John\"}");

            var jsonFormatter = new JsonInputFormatter() { CaptureDeserilizationErrors = true };
            // by default we ignore missing members, so here explicitly changing it
            jsonFormatter.SerializerSettings = new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Error
            };

            var actionContext = GetActionContext(contentBytes, "application/json;charset=utf-8");
            var metadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(UserLogin));
            var inputFormatterContext = new InputFormatterContext(actionContext, metadata.ModelType);

            // Act
            var obj = await jsonFormatter.ReadAsync(inputFormatterContext);

            // Assert
            Assert.False(actionContext.ModelState.IsValid);

            var modelErrorMessage = actionContext.ModelState.Values.First().Errors[0].Exception.Message;
            Assert.Contains("Required property 'Password' not found in JSON", modelErrorMessage);
        }

        private static ActionContext GetActionContext(byte[] contentBytes,
                                                 string contentType = "application/xml")
        {
            return new ActionContext(GetHttpContext(contentBytes, contentType),
                                     new AspNet.Routing.RouteData(),
                                     new ActionDescriptor());
        }

        private static HttpContext GetHttpContext(byte[] contentBytes,
                                                        string contentType = "application/json")
        {
            var request = new Mock<HttpRequest>();
            var headers = new Mock<IHeaderDictionary>();
            request.SetupGet(r => r.Headers).Returns(headers.Object);
            request.SetupGet(f => f.Body).Returns(new MemoryStream(contentBytes));
            request.SetupGet(f => f.ContentType).Returns(contentType);

            var httpContext = new Mock<HttpContext>();
            httpContext.SetupGet(c => c.Request).Returns(request.Object);
            httpContext.SetupGet(c => c.Request).Returns(request.Object);
            return httpContext.Object;
        }

        private sealed class User
        {
            public string Name { get; set; }

            public decimal Age { get; set; }
        }

        private sealed class UserLogin
        {
            [JsonProperty(Required = Required.Always)]
            public string UserName { get; set; }

            [JsonProperty(Required = Required.Always)]
            public string Password { get; set; }
        }
    }
}
#endif
