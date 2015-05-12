// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if DNX451
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

            var httpContext = GetHttpContext(contentBytes, contentType: requestContentType);
            var formatterContext = new InputFormatterContext(httpContext, new ModelStateDictionary(), typeof(string));

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

            var httpContext = GetHttpContext(contentBytes);
            var context = new InputFormatterContext(httpContext, new ModelStateDictionary(), type);

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

            var httpContext = GetHttpContext(contentBytes);
            var context = new InputFormatterContext(httpContext, new ModelStateDictionary(), typeof(User));

            // Act
            var model = await formatter.ReadAsync(context);

            // Assert
            var userModel = Assert.IsType<User>(model);
            Assert.Equal("Person Name", userModel.Name);
            Assert.Equal(30, userModel.Age);
        }

        [Fact]
        public async Task ReadAsync_AddsModelValidationErrorsToModelState()
        {
            // Arrange
            var content = "{name: 'Person Name', Age: 'not-an-age'}";
            var formatter = new JsonInputFormatter();
            var contentBytes = Encoding.UTF8.GetBytes(content);

            var modelState = new ModelStateDictionary();
            var httpContext = GetHttpContext(contentBytes);

            var context = new InputFormatterContext(httpContext, modelState, typeof(User));

            // Act
            var model = await formatter.ReadAsync(context);

            // Assert
            Assert.Equal(
                "Could not convert string to decimal: not-an-age. Path 'Age', line 1, position 39.",
                modelState["Age"].Errors[0].Exception.Message);
        }

        [Fact]
        public async Task ReadAsync_UsesTryAddModelValidationErrorsToModelState()
        {
            // Arrange
            var content = "{name: 'Person Name', Age: 'not-an-age'}";
            var formatter = new JsonInputFormatter();
            var contentBytes = Encoding.UTF8.GetBytes(content);

            var modelState = new ModelStateDictionary();
            var httpContext = GetHttpContext(contentBytes);

            var context = new InputFormatterContext(httpContext, modelState, typeof(User));

            modelState.MaxAllowedErrors = 3;
            modelState.AddModelError("key1", "error1");
            modelState.AddModelError("key2", "error2");

            // Act
            var model = await formatter.ReadAsync(context);

            // Assert
            Assert.False(modelState.ContainsKey("age"));
            var error = Assert.Single(modelState[""].Errors);
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

            var jsonFormatter = new JsonInputFormatter();
            // by default we ignore missing members, so here explicitly changing it
            jsonFormatter.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Error;

            var modelState = new ModelStateDictionary();
            var httpContext = GetHttpContext(contentBytes, "application/json;charset=utf-8");

            var inputFormatterContext = new InputFormatterContext(httpContext, modelState, typeof(UserLogin));

            // Act
            var obj = await jsonFormatter.ReadAsync(inputFormatterContext);

            // Assert
            Assert.False(modelState.IsValid);

            var modelErrorMessage = modelState.Values.First().Errors[0].Exception.Message;
            Assert.Contains("Required property 'Password' not found in JSON", modelErrorMessage);
        }

        [Fact]
        public async Task CustomSerializerSettingsObject_TakesEffect()
        {
            // Arrange
            // missing password property here
            var contentBytes = Encoding.UTF8.GetBytes("{ \"UserName\" : \"John\"}");

            var jsonFormatter = new JsonInputFormatter();
            // by default we ignore missing members, so here explicitly changing it
            jsonFormatter.SerializerSettings = new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Error
            };

            var modelState = new ModelStateDictionary();
            var httpContext = GetHttpContext(contentBytes, "application/json;charset=utf-8");

            var inputFormatterContext = new InputFormatterContext(httpContext, modelState, typeof(UserLogin));

            // Act
            var obj = await jsonFormatter.ReadAsync(inputFormatterContext);

            // Assert
            Assert.False(modelState.IsValid);

            var modelErrorMessage = modelState.Values.First().Errors[0].Exception.Message;
            Assert.Contains("Required property 'Password' not found in JSON", modelErrorMessage);
        }

        [Fact]
        public async Task ThrowsException_OnSupplyingNull_ForRequiredValueType()
        {
            // Arrange
            var contentBytes = Encoding.UTF8.GetBytes("{\"Id\":\"null\",\"Name\":\"Programming C#\"}");
            var jsonFormatter = new JsonInputFormatter();

            var modelState = new ModelStateDictionary();
            var httpContext = GetHttpContext(contentBytes, "application/json;charset=utf-8");
            
            var inputFormatterContext = new InputFormatterContext(httpContext, modelState, typeof(Book));

            // Act
            var obj = await jsonFormatter.ReadAsync(inputFormatterContext);

            // Assert
            var book = obj as Book;
            Assert.NotNull(book);
            Assert.Equal(0, book.Id);
            Assert.Equal("Programming C#", book.Name);
            Assert.False(modelState.IsValid);

            Assert.Equal(1, modelState.Values.First().Errors.Count);
            var modelErrorMessage = modelState.Values.First().Errors[0].Exception.Message;
            Assert.Contains("Could not convert string to integer: null. Path 'Id'", modelErrorMessage);
        }

        [Theory]
        [InlineData(typeof(Book))]
        [InlineData(typeof(EBook))]
        public async Task Validates_RequiredAttribute_OnRegularAndInheritedProperties(Type type)
        {
            // Arrange
            var contentBytes = Encoding.UTF8.GetBytes("{ \"Name\" : \"Programming C#\"}");
            var jsonFormatter = new JsonInputFormatter();

            var modelState = new ModelStateDictionary();
            var httpContext = GetHttpContext(contentBytes, "application/json;charset=utf-8");
            
            var inputFormatterContext = new InputFormatterContext(httpContext, modelState, type);

            // Act
            var obj = await jsonFormatter.ReadAsync(inputFormatterContext);

            // Assert
            Assert.False(modelState.IsValid);
            Assert.Equal(1, modelState.Count);

            var modelErrorMessage = modelState.Values.First().Errors[0].Exception.Message;
            Assert.Contains("Required property 'Id' not found in JSON", modelErrorMessage);
        }

        [Fact]
        public async Task Validates_RequiredAttributeOnStructTypes()
        {
            // Arrange
            var contentBytes = Encoding.UTF8.GetBytes("{\"Longitude\":{}}");
            var jsonFormatter = new JsonInputFormatter();

            var modelState = new ModelStateDictionary();
            var httpContext = GetHttpContext(contentBytes, "application/json;charset=utf-8");
            
            var inputFormatterContext = new InputFormatterContext(httpContext, modelState, typeof(GpsCoordinate));

            // Act
            var obj = await jsonFormatter.ReadAsync(inputFormatterContext);

            // Assert
            Assert.False(modelState.IsValid);
            Assert.Equal(2, modelState.Count);
            var errorMessages = GetModelStateErrorMessages(modelState);
            Assert.Equal(3, errorMessages.Count());
            Assert.Contains(
                errorMessages,
                (errorMessage) => errorMessage.Contains("Required property 'Latitude' not found in JSON"));
            Assert.Contains(
                errorMessages,
                (errorMessage) => errorMessage.Contains("Required property 'X' not found in JSON"));
            Assert.Contains(
                errorMessages,
                (errorMessage) => errorMessage.Contains("Required property 'Y' not found in JSON"));
        }

        [Fact]
        public async Task Validation_DoesNotHappen_ForNonRequired_ValueTypeProperties()
        {
            // Arrange
            var contentBytes = Encoding.UTF8.GetBytes("{\"Name\":\"Seattle\"}");
            var jsonFormatter = new JsonInputFormatter();

            var modelState = new ModelStateDictionary();
            var httpContext = GetHttpContext(contentBytes, "application/json;charset=utf-8");
            
            var inputFormatterContext = new InputFormatterContext(httpContext, modelState, typeof(Location));

            // Act
            var obj = await jsonFormatter.ReadAsync(inputFormatterContext);

            // Assert
            Assert.True(modelState.IsValid);
            var location = obj as Location;
            Assert.NotNull(location);
            Assert.Equal(0, location.Id);
            Assert.Equal("Seattle", location.Name);
        }

        [Fact]
        public async Task Validation_DoesNotHappen_OnNullableValueTypeProperties()
        {
            // Arrange
            var contentBytes = Encoding.UTF8.GetBytes("{}");
            var jsonFormatter = new JsonInputFormatter();

            var modelState = new ModelStateDictionary();
            var httpContext = GetHttpContext(contentBytes, "application/json;charset=utf-8");

            var inputFormatterContext = new InputFormatterContext(httpContext, modelState, typeof(Venue));

            // Act
            var obj = await jsonFormatter.ReadAsync(inputFormatterContext);

            // Assert
            Assert.True(modelState.IsValid);
            var venue = obj as Venue;
            Assert.NotNull(venue);
            Assert.Null(venue.Location);
            Assert.Null(venue.NearByLocations);
            Assert.Null(venue.Name);
        }

        private static HttpContext GetHttpContext(
            byte[] contentBytes,
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

        private IEnumerable<string> GetModelStateErrorMessages(ModelStateDictionary modelStateDictionary)
        {
            var allErrorMessages = new List<string>();
            foreach (var keyModelStatePair in modelStateDictionary)
            {
                var key = keyModelStatePair.Key;
                var errors = keyModelStatePair.Value.Errors;
                if (errors != null && errors.Count > 0)
                {
                    foreach (var modelError in errors)
                    {
                        if (string.IsNullOrEmpty(modelError.ErrorMessage))
                        {
                            if (modelError.Exception != null)
                            {
                                allErrorMessages.Add(modelError.Exception.Message);
                            }
                        }
                        else
                        {
                            allErrorMessages.Add(modelError.ErrorMessage);
                        }
                    }
                }
            }

            return allErrorMessages;
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

        private class Book
        {
            [Required]
            public int Id { get; set; }

            [Required]
            public string Name { get; set; }
        }

        private class EBook : Book
        {
        }

        private struct Point
        {
            [Required]
            public int X { get; set; }

            [Required]
            public int Y { get; set; }
        }

        private class GpsCoordinate
        {
            [Required]
            public Point Latitude { get; set; }

            [Required]
            public Point Longitude { get; set; }
        }

        private class Location
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        private class Venue
        {
            [Required]
            public string Name { get; set; }

            [Required]
            public Point? Location { get; set; }

            [Required]
            public List<Point> NearByLocations { get; set; }
        }
    }
}
#endif
