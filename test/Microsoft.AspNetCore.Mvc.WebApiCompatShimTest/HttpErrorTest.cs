// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using Xunit;

namespace System.Web.Http.Dispatcher
{
    public class HttpErrorTest
    {
        public static IEnumerable<object[]> ErrorKeyValue
        {
            get
            {
                var httpError = new HttpError();
                yield return new object[] { httpError, (Func<string>)(() => httpError.Message), "Message", "Message_Value" };
                yield return new object[] { httpError, (Func<string>)(() => httpError.MessageDetail), "MessageDetail", "MessageDetail_Value" };
                yield return new object[] { httpError, (Func<string>)(() => httpError.ExceptionMessage), "ExceptionMessage", "ExceptionMessage_Value" };
                yield return new object[] { httpError, (Func<string>)(() => httpError.ExceptionType), "ExceptionType", "ExceptionType_Value" };
                yield return new object[] { httpError, (Func<string>)(() => httpError.StackTrace), "StackTrace", "StackTrace_Value" };
            }
        }

        public static IEnumerable<object[]> HttpErrors
        {
            get
            {
                yield return new[] { new HttpError() };
                yield return new[] { new HttpError("error") };
                yield return new[] { new HttpError(new NotImplementedException(), true) };

                var modelState = new ModelStateDictionary();
                modelState.AddModelError("key", "error");
                yield return new[] { new HttpError(modelState, true) };
            }
        }

        [Fact]
        public void StringConstructor_AddsCorrectDictionaryItems()
        {
            var error = new HttpError("something bad happened");

            Assert.Contains(new KeyValuePair<string, object>("Message", "something bad happened"), error);
        }

        [Fact]
        public void ExceptionConstructorWithDetail_AddsCorrectDictionaryItems()
        {
            var error = new HttpError(new ArgumentException("error", new Exception()), true);

            Assert.Contains(new KeyValuePair<string, object>("Message", "An error has occurred."), error);
            Assert.Contains(new KeyValuePair<string, object>("ExceptionMessage", "error"), error);
            Assert.Contains(new KeyValuePair<string, object>("ExceptionType", "System.ArgumentException"), error);
            Assert.True(error.ContainsKey("StackTrace"));
            Assert.True(error.ContainsKey("InnerException"));
            Assert.IsType<HttpError>(error["InnerException"]);
        }

        [Fact]
        public void ModelStateConstructorWithDetail_AddsCorrectDictionaryItems()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            var provider = new EmptyModelMetadataProvider();
            var metadata = provider.GetMetadataForProperty(typeof(string), nameof(string.Length));
            modelState.AddModelError("[0].Name", "error1");
            modelState.AddModelError("[0].Name", "error2");
            modelState.AddModelError("[0].Address", "error");
            modelState.AddModelError("[2].Name", new Exception("OH NO"), metadata);

            // Act
            var error = new HttpError(modelState, true);

            // Assert
            var modelStateError = error["ModelState"] as HttpError;

            Assert.Contains(new KeyValuePair<string, object>("Message", "The request is invalid."), error);
            Assert.Contains("error1", modelStateError["[0].Name"] as IEnumerable<string>);
            Assert.Contains("error2", modelStateError["[0].Name"] as IEnumerable<string>);
            Assert.Contains("error", modelStateError["[0].Address"] as IEnumerable<string>);
            Assert.True(modelStateError.ContainsKey("[2].Name"));
            Assert.Contains("OH NO", modelStateError["[2].Name"] as IEnumerable<string>);
        }

        [Fact]
        public void ExceptionConstructorWithoutDetail_AddsCorrectDictionaryItems()
        {
            var error = new HttpError(new ArgumentException("error", new Exception()), false);

            Assert.Contains(new KeyValuePair<string, object>("Message", "An error has occurred."), error);
            Assert.False(error.ContainsKey("ExceptionMessage"));
            Assert.False(error.ContainsKey("ExceptionType"));
            Assert.False(error.ContainsKey("StackTrace"));
            Assert.False(error.ContainsKey("InnerException"));
        }

        [Fact]
        public void ModelStateConstructorWithoutDetail_AddsCorrectDictionaryItems()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            var provider = new EmptyModelMetadataProvider();
            var metadata = provider.GetMetadataForProperty(typeof(string), nameof(string.Length));
            modelState.AddModelError("[0].Name", "error1");
            modelState.AddModelError("[0].Name", "error2");
            modelState.AddModelError("[0].Address", "error");
            modelState.AddModelError("[2].Name", new Exception("OH NO"), metadata);

            // Act
            var error = new HttpError(modelState, false);

            // Assert
            var modelStateError = error["ModelState"] as HttpError;

            Assert.Contains(new KeyValuePair<string, object>("Message", "The request is invalid."), error);
            Assert.Contains("error1", modelStateError["[0].Name"] as IEnumerable<string>);
            Assert.Contains("error2", modelStateError["[0].Name"] as IEnumerable<string>);
            Assert.Contains("error", modelStateError["[0].Address"] as IEnumerable<string>);
            Assert.True(modelStateError.ContainsKey("[2].Name"));
            Assert.DoesNotContain("OH NO", modelStateError["[2].Name"] as IEnumerable<string>);
        }

        [Fact]
        public async Task HttpError_Roundtrips_WithJsonFormatter()
        {
            var error = new HttpError("error") { { "ErrorCode", 42 }, { "Data", new[] { "a", "b", "c" } } };
            MediaTypeFormatter formatter = new JsonMediaTypeFormatter();
            var stream = new MemoryStream();

            await formatter.WriteToStreamAsync(typeof(HttpError), error, stream, content: null, transportContext: null);
            stream.Position = 0;
            var result = await formatter.ReadFromStreamAsync(typeof(HttpError), stream, content: null, formatterLogger: null);

            var roundtrippedError = Assert.IsType<HttpError>(result);
            Assert.NotNull(roundtrippedError);
            Assert.Equal("error", roundtrippedError.Message);
            Assert.Equal(42L, roundtrippedError["ErrorCode"]);
            var data = roundtrippedError["Data"] as JArray;
            Assert.Equal(3, data.Count);
            Assert.Contains("a", data);
            Assert.Contains("b", data);
            Assert.Contains("c", data);
        }

        [Fact]
        public async Task HttpError_Roundtrips_WithXmlFormatter()
        {
            var error = new HttpError("error") { { "ErrorCode", 42 }, { "Data", new[] { "a", "b", "c" } } };
            MediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            var stream = new MemoryStream();

            await formatter.WriteToStreamAsync(typeof(HttpError), error, stream, content: null, transportContext: null);
            stream.Position = 0;
            var result = await formatter.ReadFromStreamAsync(typeof(HttpError), stream, content: null, formatterLogger: null);

            var roundtrippedError = Assert.IsType<HttpError>(result);
            Assert.NotNull(roundtrippedError);
            Assert.Equal("error", roundtrippedError.Message);
            Assert.Equal("42", roundtrippedError["ErrorCode"]);
            Assert.Equal("a b c", roundtrippedError["Data"]);
        }

        [Fact]
        public async Task HttpErrorWithWhitespace_Roundtrips_WithXmlFormatter()
        {
            var message = "  foo\n bar  \n ";
            var error = new HttpError(message);
            MediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            var stream = new MemoryStream();

            await formatter.WriteToStreamAsync(typeof(HttpError), error, stream, content: null, transportContext: null);
            stream.Position = 0;
            var result = await formatter.ReadFromStreamAsync(typeof(HttpError), stream, content: null, formatterLogger: null);

            var roundtrippedError = Assert.IsType<HttpError>(result);
            Assert.NotNull(roundtrippedError);
            Assert.Equal(message, roundtrippedError.Message);
        }

        [Fact]
        public async Task HttpError_Roundtrips_WithXmlSerializer()
        {
            var error = new HttpError("error") { { "ErrorCode", 42 }, { "Data", new[] { "a", "b", "c" } } };
            MediaTypeFormatter formatter = new XmlMediaTypeFormatter() { UseXmlSerializer = true };
            var stream = new MemoryStream();

            await formatter.WriteToStreamAsync(typeof(HttpError), error, stream, content: null, transportContext: null);
            stream.Position = 0;
            var result = await formatter.ReadFromStreamAsync(typeof(HttpError), stream, content: null, formatterLogger: null);

            var roundtrippedError = Assert.IsType<HttpError>(result);
            Assert.NotNull(roundtrippedError);
            Assert.Equal("error", roundtrippedError.Message);
            Assert.Equal("42", roundtrippedError["ErrorCode"]);
            Assert.Equal("a b c", roundtrippedError["Data"]);
        }

        [Fact]
        public async Task HttpErrorForInnerException_Serializes_WithXmlSerializer()
        {
            var error = new HttpError(new ArgumentException("error", new Exception("innerError")), includeErrorDetail: true);
            MediaTypeFormatter formatter = new XmlMediaTypeFormatter() { UseXmlSerializer = true };
            var stream = new MemoryStream();

            await formatter.WriteToStreamAsync(typeof(HttpError), error, stream, content: null, transportContext: null);
            stream.Position = 0;
            var serializedError = new StreamReader(stream).ReadToEnd();

            Assert.NotNull(serializedError);
            Assert.Equal(
                "<Error><Message>An error has occurred.</Message><ExceptionMessage>error</ExceptionMessage><ExceptionType>System.ArgumentException</ExceptionType><StackTrace /><InnerException><Message>An error has occurred.</Message><ExceptionMessage>innerError</ExceptionMessage><ExceptionType>System.Exception</ExceptionType><StackTrace /></InnerException></Error>",
                serializedError);
        }

        [Fact]
        public async Task HttpErrorWithModelState_Roundtrips_WithJsonFormatter()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddModelError(string.Empty, "Root error");
            modelState.AddModelError("key1", "key1 error 1");
            modelState.AddModelError("key1", "key1 error 2");
            modelState.AddModelError("key2", "key2 error");

            var error = new HttpError(modelState, includeErrorDetail: false);
            var formatter = new JsonMediaTypeFormatter();
            var stream = new MemoryStream();

            // Act
            await formatter.WriteToStreamAsync(typeof(HttpError), error, stream, content: null, transportContext: null);
            stream.Position = 0;
            var result = await formatter.ReadFromStreamAsync(typeof(HttpError), stream, content: null, formatterLogger: null);

            // Assert
            var roundtrippedError = Assert.IsType<HttpError>(result);
            Assert.Equal(2, roundtrippedError.Count);
            Assert.Equal("The request is invalid.", roundtrippedError.Message);

            var innerError = roundtrippedError.ModelState;
            Assert.NotNull(innerError);
            Assert.Collection(
                innerError,
                kvp =>
                {
                    Assert.Empty(kvp.Key);
                    var errors = Assert.IsType<string[]>(kvp.Value);
                    var message = Assert.Single(errors);
                    Assert.Equal("Root error", message);
                },
                kvp =>
                {
                    Assert.Equal("key1", kvp.Key);
                    var errors = Assert.IsType<string[]>(kvp.Value);
                    Assert.Collection(
                        errors,
                        message => Assert.Equal("key1 error 1", message),
                        message => Assert.Equal("key1 error 2", message));
                },
                kvp =>
                {
                    Assert.Equal("key2", kvp.Key);
                    var errors = Assert.IsType<string[]>(kvp.Value);
                    var message = Assert.Single(errors);
                    Assert.Equal("key2 error", message);
                });
        }

        [Fact]
        public async Task HttpErrorWithModelState_Roundtrips_WithXmlFormatter()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddModelError(string.Empty, "Root error");
            modelState.AddModelError("key1", "key1 error 1");
            modelState.AddModelError("key1", "key1 error 2");
            modelState.AddModelError("key2", "key2 error");

            var error = new HttpError(modelState, includeErrorDetail: false);
            var formatter = new XmlMediaTypeFormatter();
            var stream = new MemoryStream();

            // Act
            await formatter.WriteToStreamAsync(typeof(HttpError), error, stream, content: null, transportContext: null);
            stream.Position = 0;
            var text = await new StreamReader(stream).ReadToEndAsync();
            stream.Position = 0;
            var result = await formatter.ReadFromStreamAsync(typeof(HttpError), stream, content: null, formatterLogger: null);

            // Assert
            var roundtrippedError = Assert.IsType<HttpError>(result);
            Assert.Equal(2, roundtrippedError.Count);
            Assert.Equal("The request is invalid.", roundtrippedError.Message);

            var innerError = roundtrippedError.ModelState;
            Assert.NotNull(innerError);
            Assert.Collection(
                innerError,
                kvp =>
                {
                    Assert.Empty(kvp.Key);
                    var messages = Assert.IsType<string>(kvp.Value);
                    Assert.Equal("Root error", messages);
                },
                kvp =>
                {
                    Assert.Equal("key1", kvp.Key);
                    var messages = Assert.IsType<string>(kvp.Value);
                    Assert.Equal("key1 error 1 key1 error 2", messages);
                },
                kvp =>
                {
                    Assert.Equal("key2", kvp.Key);
                    var messages = Assert.IsType<string>(kvp.Value);
                    Assert.Equal("key2 error", messages);
                });
        }


        [Fact]
        public async Task HttpErrorWithModelState_Roundtrips_WithXmlSerializer()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddModelError(string.Empty, "Root error");
            modelState.AddModelError("key1", "key1 error 1");
            modelState.AddModelError("key1", "key1 error 2");
            modelState.AddModelError("key2", "key2 error");

            var error = new HttpError(modelState, includeErrorDetail: false);
            var formatter = new XmlMediaTypeFormatter { UseXmlSerializer = true };
            var stream = new MemoryStream();

            // Act
            await formatter.WriteToStreamAsync(typeof(HttpError), error, stream, content: null, transportContext: null);
            stream.Position = 0;
            var result = await formatter.ReadFromStreamAsync(typeof(HttpError), stream, content: null, formatterLogger: null);

            // Assert
            var roundtrippedError = Assert.IsType<HttpError>(result);
            Assert.Equal(2, roundtrippedError.Count);
            Assert.Equal("The request is invalid.", roundtrippedError.Message);

            var innerError = roundtrippedError.ModelState;
            Assert.NotNull(innerError);
            Assert.Collection(
                innerError,
                kvp =>
                {
                    Assert.Empty(kvp.Key);
                    var messages = Assert.IsType<string>(kvp.Value);
                    Assert.Equal("Root error", messages);
                },
                kvp =>
                {
                    Assert.Equal("key1", kvp.Key);
                    var messages = Assert.IsType<string>(kvp.Value);
                    Assert.Equal("key1 error 1 key1 error 2", messages);
                },
                kvp =>
                {
                    Assert.Equal("key2", kvp.Key);
                    var messages = Assert.IsType<string>(kvp.Value);
                    Assert.Equal("key2 error", messages);
                });
        }

        [Fact]
        public async Task HttpErrorWithInnerExceptions_Roundtrips_WithJsonFormatter()
        {
            // Arrange
            var error = new HttpError(
                new ArgumentException("error1", new ArgumentException("error2", new ArgumentException("error3"))),
                includeErrorDetail: true);
            var formatter = new JsonMediaTypeFormatter();
            var stream = new MemoryStream();

            // Act
            await formatter.WriteToStreamAsync(typeof(HttpError), error, stream, content: null, transportContext: null);
            stream.Position = 0;
            var result = await formatter.ReadFromStreamAsync(typeof(HttpError), stream, content: null, formatterLogger: null);

            // Assert
            var roundtrippedError = Assert.IsType<HttpError>(result);
            Assert.Equal(5, roundtrippedError.Count);
            Assert.Equal("An error has occurred.", roundtrippedError.Message);
            Assert.Equal("error1", roundtrippedError.ExceptionMessage);
            Assert.Equal(typeof(ArgumentException).FullName, roundtrippedError.ExceptionType);
            Assert.Contains(HttpErrorKeys.StackTraceKey, roundtrippedError.Keys);
            Assert.Null(roundtrippedError.StackTrace);

            roundtrippedError = roundtrippedError.InnerException;
            Assert.NotNull(roundtrippedError);
            Assert.Equal(5, roundtrippedError.Count);
            Assert.Equal("An error has occurred.", roundtrippedError.Message);
            Assert.Equal("error2", roundtrippedError.ExceptionMessage);
            Assert.Equal(typeof(ArgumentException).FullName, roundtrippedError.ExceptionType);
            Assert.Contains(HttpErrorKeys.StackTraceKey, roundtrippedError.Keys);
            Assert.Null(roundtrippedError.StackTrace);

            roundtrippedError = roundtrippedError.InnerException;
            Assert.NotNull(roundtrippedError);
            Assert.Equal(4, roundtrippedError.Count);
            Assert.Equal("An error has occurred.", roundtrippedError.Message);
            Assert.Equal("error3", roundtrippedError.ExceptionMessage);
            Assert.Equal(typeof(ArgumentException).FullName, roundtrippedError.ExceptionType);
            Assert.Contains(HttpErrorKeys.StackTraceKey, roundtrippedError.Keys);
            Assert.Null(roundtrippedError.StackTrace);
        }

        [Fact]
        public async Task HttpErrorWithInnerExceptions_Roundtrips_WithXmlFormatter()
        {
            // Arrange
            var error = new HttpError(
                new ArgumentException("error1", new ArgumentException("error2", new ArgumentException("error3"))),
                includeErrorDetail: true);
            var formatter = new XmlMediaTypeFormatter();
            var stream = new MemoryStream();

            // Act
            await formatter.WriteToStreamAsync(typeof(HttpError), error, stream, content: null, transportContext: null);
            stream.Position = 0;
            var text = await new StreamReader(stream).ReadToEndAsync();
            stream.Position = 0;
            var result = await formatter.ReadFromStreamAsync(typeof(HttpError), stream, content: null, formatterLogger: null);

            // Assert
            var roundtrippedError = Assert.IsType<HttpError>(result);
            Assert.Equal(5, roundtrippedError.Count);
            Assert.Equal("An error has occurred.", roundtrippedError.Message);
            Assert.Equal("error1", roundtrippedError.ExceptionMessage);
            Assert.Equal(typeof(ArgumentException).FullName, roundtrippedError.ExceptionType);
            Assert.Contains(HttpErrorKeys.StackTraceKey, roundtrippedError.Keys);
            Assert.Empty(roundtrippedError.StackTrace);

            roundtrippedError = roundtrippedError.InnerException;
            Assert.NotNull(roundtrippedError);
            Assert.Equal(5, roundtrippedError.Count);
            Assert.Equal("An error has occurred.", roundtrippedError.Message);
            Assert.Equal("error2", roundtrippedError.ExceptionMessage);
            Assert.Equal(typeof(ArgumentException).FullName, roundtrippedError.ExceptionType);
            Assert.Contains(HttpErrorKeys.StackTraceKey, roundtrippedError.Keys);
            Assert.Empty(roundtrippedError.StackTrace);

            roundtrippedError = roundtrippedError.InnerException;
            Assert.NotNull(roundtrippedError);
            Assert.Equal(4, roundtrippedError.Count);
            Assert.Equal("An error has occurred.", roundtrippedError.Message);
            Assert.Equal("error3", roundtrippedError.ExceptionMessage);
            Assert.Equal(typeof(ArgumentException).FullName, roundtrippedError.ExceptionType);
            Assert.Contains(HttpErrorKeys.StackTraceKey, roundtrippedError.Keys);
            Assert.Empty(roundtrippedError.StackTrace);
        }


        [Fact]
        public async Task HttpErrorWithInnerExceptions_Roundtrips_WithXmlSerializer()
        {
            // Arrange
            var error = new HttpError(
                new ArgumentException("error1", new ArgumentException("error2", new ArgumentException("error3"))),
                includeErrorDetail: true);
                        var formatter = new XmlMediaTypeFormatter { UseXmlSerializer = true };
            var stream = new MemoryStream();

            // Act
            await formatter.WriteToStreamAsync(typeof(HttpError), error, stream, content: null, transportContext: null);
            stream.Position = 0;
            var result = await formatter.ReadFromStreamAsync(typeof(HttpError), stream, content: null, formatterLogger: null);

            // Assert
            var roundtrippedError = Assert.IsType<HttpError>(result);
            Assert.Equal(5, roundtrippedError.Count);
            Assert.Equal("An error has occurred.", roundtrippedError.Message);
            Assert.Equal("error1", roundtrippedError.ExceptionMessage);
            Assert.Equal(typeof(ArgumentException).FullName, roundtrippedError.ExceptionType);
            Assert.Contains(HttpErrorKeys.StackTraceKey, roundtrippedError.Keys);
            Assert.Empty(roundtrippedError.StackTrace);

            roundtrippedError = roundtrippedError.InnerException;
            Assert.NotNull(roundtrippedError);
            Assert.Equal(5, roundtrippedError.Count);
            Assert.Equal("An error has occurred.", roundtrippedError.Message);
            Assert.Equal("error2", roundtrippedError.ExceptionMessage);
            Assert.Equal(typeof(ArgumentException).FullName, roundtrippedError.ExceptionType);
            Assert.Contains(HttpErrorKeys.StackTraceKey, roundtrippedError.Keys);
            Assert.Empty(roundtrippedError.StackTrace);

            roundtrippedError = roundtrippedError.InnerException;
            Assert.NotNull(roundtrippedError);
            Assert.Equal(4, roundtrippedError.Count);
            Assert.Equal("An error has occurred.", roundtrippedError.Message);
            Assert.Equal("error3", roundtrippedError.ExceptionMessage);
            Assert.Equal(typeof(ArgumentException).FullName, roundtrippedError.ExceptionType);
            Assert.Contains(HttpErrorKeys.StackTraceKey, roundtrippedError.Keys);
            Assert.Empty(roundtrippedError.StackTrace);
        }

        [Fact]
        public void GetPropertyValue_GetsValue_IfTypeMatches()
        {
            var error = new HttpError
            {
                ["key"] = "x"
            };

            Assert.Equal("x", error.GetPropertyValue<string>("key"));
            Assert.Equal("x", error.GetPropertyValue<object>("key"));
        }

        [Fact]
        public void GetPropertyValue_GetsDefault_IfTypeDoesNotMatch()
        {
            var error = new HttpError
            {
                ["key"] = "x"
            };

            Assert.Null(error.GetPropertyValue<Uri>("key"));
            Assert.Equal(0, error.GetPropertyValue<int>("key"));
        }

        [Fact]
        public void GetPropertyValue_GetsDefault_IfPropertyMissing()
        {
            var error = new HttpError();

            Assert.Null(error.GetPropertyValue<string>("key"));
            Assert.Equal(0, error.GetPropertyValue<int>("key"));
        }

        [Theory]
        [MemberData(nameof(ErrorKeyValue))]
        public void HttpErrorStringProperties_UseCorrectHttpErrorKey(HttpError httpError, Func<string> productUnderTest, string key, string actualValue)
        {
            // Arrange
            httpError[key] = actualValue;

            // Act
            var expectedValue = productUnderTest.Invoke();

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void HttpErrorProperty_InnerException_UsesCorrectHttpErrorKey()
        {
            // Arrange
            var error = new HttpError(new ArgumentException("error", new Exception()), true);

            // Act
            var innerException = error.InnerException;

            // Assert
            Assert.Same(error["InnerException"], innerException);
        }

        [Fact]
        public void HttpErrorProperty_ModelState_UsesCorrectHttpErrorKey()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("[0].Name", "error1");
            var error = new HttpError(modelState, true);

            // Act
            var actualModelStateError = error.ModelState;

            // Assert
            Assert.Same(error["ModelState"], actualModelStateError);
        }

        [Theory]
        [MemberData(nameof(HttpErrors))]
        public void HttpErrors_UseCaseInsensitiveComparer(HttpError httpError)
        {
            // Arrange
            var lowercaseKey = "abcd";
            var uppercaseKey = "ABCD";

            httpError[lowercaseKey] = "error";

            // Act & Assert
            Assert.True(httpError.ContainsKey(lowercaseKey));
            Assert.True(httpError.ContainsKey(uppercaseKey));
        }
    }
}
