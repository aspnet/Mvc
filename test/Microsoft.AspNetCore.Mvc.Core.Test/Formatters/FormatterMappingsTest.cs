// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.TestCommon;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Xunit;


namespace Microsoft.AspNetCore.Mvc.Formatters
{
    public class FormatterMappingsTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void FormatterMappings_GetMediaTypeMappingForFormat_ThrowsForInvalidFormats(string format)
        {
            // Arrange
            var options = new FormatterMappings();

            // Act & Assert
            Assert.Throws<ArgumentException>("format", () => options.GetMediaTypeMappingForFormat(format));
        }

        [Theory]
        [InlineData(".xml", "application/xml", "xml")]
        [InlineData("json", "application/json", "JSON")]
        [InlineData(".foo", "text/foo", "Foo")]
        [InlineData(".Json", "application/json", "json")]
        [InlineData("FOo", "text/foo", "FOO")]
        public void FormatterMappings_SetFormatMapping_DiffSetGetFormat(string setFormat, string contentType, string getFormat)
        {
            // Arrange
            var options = new FormatterMappings();
            options.SetMediaTypeMappingForFormat(setFormat, MediaTypeHeaderValue.Parse(contentType));

            // Act 
            var returnMediaType = options.GetMediaTypeMappingForFormat(getFormat);

            // Assert
            MediaTypeAssert.Equal(contentType, returnMediaType);
        }

        [Fact]
        public void FormatterMappings_Invalid_Period()
        {
            // Arrange
            var options = new FormatterMappings();
            var format = ".";
            var expected = string.Format(@"The format provided is invalid '{0}'. A format must be a non-empty file-" +
                "extension, optionally prefixed with a '.' character.", format);

            // Act and assert
            var exception = Assert.Throws<ArgumentException>(() => options.SetMediaTypeMappingForFormat(
                format,
                MediaTypeHeaderValue.Parse("application/xml")));
            Assert.Equal(expected, exception.Message);
        }

        [Fact]
        public void FormatterMappings_SetFormatMapping_FormatEmpty()
        {
            // Arrange
            var options = new FormatterMappings();
            var format = "";

            // Act and assert
            var exception = Assert.Throws<ArgumentException>(() => options.SetMediaTypeMappingForFormat(
                format,
                MediaTypeHeaderValue.Parse("application/xml")));
            Assert.Equal("format", exception.ParamName);
        }


        [Theory]
        [InlineData("application/*")]
        [InlineData("*/json")]
        [InlineData("*/*")]
        public void FormatterMappings_Wildcardformat(string format)
        {
            // Arrange
            var options = new FormatterMappings();
            var expected = string.Format(@"The media type ""{0}"" is not valid. MediaTypes containing wildcards (*) " +
                "are not allowed in formatter mappings.", format);

            // Act and assert
            var exception = Assert.Throws<ArgumentException>(() => options.SetMediaTypeMappingForFormat(
                "star",
                MediaTypeHeaderValue.Parse(format)));
            Assert.Equal(expected, exception.Message);
        }

        [Theory]
        [InlineData(".xml", true)]
        [InlineData("json", true)]
        [InlineData(".foo", true)]
        [InlineData(".Json", true)]
        [InlineData("FOo", true)]
        [InlineData("bar", true)]
        [InlineData("baz", false)]
        [InlineData(".baz", false)]
        [InlineData("BAZ", false)]
        public void FormatterMappings_ClearFormatMapping(string format, bool expected)
        {
            // Arrange
            var options = new FormatterMappings();
            var mediaType = MediaTypeHeaderValue.Parse("application/xml");
            options.SetMediaTypeMappingForFormat("xml", mediaType);
            mediaType = MediaTypeHeaderValue.Parse("application/json");
            options.SetMediaTypeMappingForFormat("json", mediaType);
            mediaType = MediaTypeHeaderValue.Parse("application/foo");
            options.SetMediaTypeMappingForFormat("foo", mediaType);
            mediaType = MediaTypeHeaderValue.Parse("application/bar");
            options.SetMediaTypeMappingForFormat("bar", mediaType);

            // Act 
            var cleared = options.ClearMediaTypeMappingForFormat(format);

            // Assert
            Assert.Equal(expected, cleared);
            Assert.Null(options.GetMediaTypeMappingForFormat(format));
        }
    }
}