// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Internal;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Microsoft.AspNet.Mvc.Formatters
{
    public class NoContentFormatterTests
    {
        public static IEnumerable<object[]> OutputFormatterContextValues_CanWriteType
        {
            get
            {
                // object value, bool useDeclaredTypeAsString, bool expectedCanwriteResult, bool useNonNullContentType
                yield return new object[] { "valid value", true, false, true };
                yield return new object[] { "valid value", false, false, true };
                yield return new object[] { "", false, false, true };
                yield return new object[] { "", true, false, true };
                yield return new object[] { null, true, true, true };
                yield return new object[] { null, false, true, true };
                yield return new object[] { null, false, true, false };
                yield return new object[] { new object(), false, false, true };
                yield return new object[] { 1232, false, false, true };
                yield return new object[] { 1232, false, false, false };
            }
        }

        [Theory]
        [MemberData(nameof(OutputFormatterContextValues_CanWriteType))]
        public void CanWriteResult_ByDefault_ReturnsTrue_IfTheValueIsNull(
            object value,
            bool declaredTypeAsString,
            bool expectedCanwriteResult,
            bool useNonNullContentType)
        {
            // Arrange
            var typeToUse = declaredTypeAsString ? typeof(string) : typeof(object);
            var formatterContext = new OutputFormatterContext()
            {
                Object = value,
                DeclaredType = typeToUse,
                HttpContext = null,
            };
            var contetType = useNonNullContentType ? MediaTypeHeaderValue.Parse("text/plain") : null;
            var formatter = new HttpNoContentOutputFormatter();

            // Act
            var actualCanWriteResult = formatter.CanWriteResult(formatterContext, contetType);

            // Assert
            Assert.Equal(expectedCanwriteResult, actualCanWriteResult);
        }

        [Theory]
        [InlineData(typeof(void))]
        [InlineData(typeof(Task))]
        public void CanWriteResult_ReturnsTrue_IfReturnTypeIsVoidOrTask(Type declaredType)
        {
            // Arrange
            var formatterContext = new OutputFormatterContext()
            {
                Object = "Something non null.",
                DeclaredType = declaredType,
                HttpContext = null,
            };
            var contetType = MediaTypeHeaderValue.Parse("text/plain");
            var formatter = new HttpNoContentOutputFormatter();

            // Act
            var actualCanWriteResult = formatter.CanWriteResult(formatterContext, contetType);

            // Assert
            Assert.True(actualCanWriteResult);
        }

        [Theory]
        [InlineData(null, true, true)]
        [InlineData(null, false, false)]
        [InlineData("some value", true, false)]
        public void
            CanWriteResult_ReturnsTrue_IfReturnValueIsNullAndTreatNullValueAsNoContentIsNotSet(string value,
                                                                                      bool treatNullValueAsNoContent,
                                                                                      bool expectedCanwriteResult)
        {
            // Arrange
            var formatterContext = new OutputFormatterContext()
            {
                Object = value,
                DeclaredType = typeof(string),
                HttpContext = null,
            };

            var contetType = MediaTypeHeaderValue.Parse("text/plain");
            var formatter = new HttpNoContentOutputFormatter()
            {
                TreatNullValueAsNoContent = treatNullValueAsNoContent
            };

            // Act
            var actualCanWriteResult = formatter.CanWriteResult(formatterContext, contetType);

            // Assert
            Assert.Equal(expectedCanwriteResult, actualCanWriteResult);
        }

        [Fact]
        public async Task WriteAsync_WritesTheStatusCode204()
        {
            // Arrange
            var defaultHttpContext = new DefaultHttpContext();
            var formatterContext = new OutputFormatterContext()
            {
                Object = null,
                HttpContext = defaultHttpContext,
            };

            var formatter = new HttpNoContentOutputFormatter();

            // Act
            await formatter.WriteAsync(formatterContext);

            // Assert
            Assert.Equal(StatusCodes.Status204NoContent, defaultHttpContext.Response.StatusCode);
        }

        [Fact]
        public async Task WriteAsync_ContextStatusCodeSet_WritesSameStatusCode()
        {
            // Arrange
            var defaultHttpContext = new DefaultHttpContext();
            var formatterContext = new OutputFormatterContext()
            {
                Object = null,
                HttpContext = defaultHttpContext,
                StatusCode = StatusCodes.Status201Created
            };

            var formatter = new HttpNoContentOutputFormatter();

            // Act
            await formatter.WriteAsync(formatterContext);

            // Assert
            Assert.Equal(StatusCodes.Status201Created, defaultHttpContext.Response.StatusCode);
        }
    }
}
