// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Testing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.WebEncoders.Testing;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Localization.Test
{
    public class HtmlLocalizerTest
    {
        [Fact]
        public void HtmlLocalizer_UseIndexer_ReturnsLocalizedHtmlString()
        {
            // Arrange
            var stringLocalizer = new TestStringLocalizer();

            var htmlLocalizer = new HtmlLocalizer(stringLocalizer);

            // Act
            var actualLocalizedHtmlString = htmlLocalizer["Test <i>html</i>"];

            // Assert
            Assert.Equal("Test <i>html</i>", actualLocalizedHtmlString.Name);
            Assert.Equal("Hello Test <i>html</i>", actualLocalizedHtmlString.Value);
        }

        [Fact]
        public void HtmlLocalizer_UseIndexerWithArguments_ReturnsLocalizedHtmlString()
        {
            // Arrange
            var stringLocalizer = new TestStringLocalizer();

            var htmlLocalizer = new HtmlLocalizer(stringLocalizer);

            // Act
            var actualLocalizedHtmlString = htmlLocalizer["Name {0}", "test"];

            // Assert
            Assert.Equal("Name {0}", actualLocalizedHtmlString.Name);
            Assert.Equal("Hello Name {0}", actualLocalizedHtmlString.Value);
            using (var writer = new StringWriter())
            {
                actualLocalizedHtmlString.WriteTo(writer, new HtmlTestEncoder());
                Assert.Equal("Hello Name HtmlEncode[[test]]", writer.ToString());
            }
        }

        public static IEnumerable<object[]> HtmlData
        {
            get
            {
                yield return new object[] { "Bonjour {0} {{{{ }}", new object[] { "test" }, "Bonjour HtmlEncode[[test]] {{ }" };
                yield return new object[] { "Bonjour {{0}}", new object[] { "{0}" }, "Bonjour {0}" };
                yield return new object[] { "Bonjour {0:x}", new object[] { 10 }, "Bonjour HtmlEncode[[a]]" };
                yield return new object[] { "Bonjour {0:x}}}", new object[] { 10 }, "Bonjour HtmlEncode[[x}]]" };
                yield return new object[] { "Bonjour {{0:x}}", new object[] { 10 }, "Bonjour {0:x}" };
                yield return new object[] { "{{ Bonjour {{{0:x}}}", new object[] { 10 }, "{ Bonjour {HtmlEncode[[x}]]" };
                yield return new object[] { "}} Bonjour {{{0:x}}}", new object[] { 10 }, "} Bonjour {HtmlEncode[[x}]]" };
                yield return new object[] { "}} Bonjour", new object[] { }, "} Bonjour" };
                yield return new object[] { "{{ {0} }}", new object[] { 10 }, "{ HtmlEncode[[10]] }" };
                yield return new object[] {
                    "Bonjour {{{0:x}}} {1:yyyy}",
                    new object[] { 10, new DateTime(2015, 10, 10) },
                    "Bonjour {HtmlEncode[[x}]] HtmlEncode[[2015]]"
                };
                yield return new object[] {
                    "Bonjour {{{0:x}}} Bienvenue {{1:yyyy}}",
                    new object[] { 10, new DateTime(2015, 10, 10) },
                    "Bonjour {HtmlEncode[[x}]] Bienvenue {1:yyyy}"
                };
                yield return new object[] { // padding happens after encoding
                    "Bonjour {0,6} Bienvenue {{1:yyyy}}",
                    new object[] { 10, new DateTime(2015, 10, 10) },
                    "Bonjour HtmlEncode[[10]] Bienvenue {1:yyyy}"
                };
                yield return new object[] { // padding happens after encoding
                    "Bonjour {0,20} Bienvenue {{1:yyyy}}",
                    new object[] { 10, new DateTime(2015, 10, 10) },
                    "Bonjour     HtmlEncode[[10]] Bienvenue {1:yyyy}"
                };
                yield return new object[] { "{0:{{000}}}", new object[] { 10 }, "HtmlEncode[[{010}]]" };
                yield return new object[] {
                    "Bonjour {0:'{{characters that should be escaped}}b'###'b'}",
                    new object[] { 10 },
                    "Bonjour HtmlEncode[[{characters that should be escaped}b10b]]"
                };
            }
        }

        [Theory]
        [MemberData(nameof(HtmlData))]
        public void HtmlLocalizer_HtmlWithArguments_ReturnsLocalizedHtml(
            string format,
            object[] arguments,
            string expectedText)
        {
            // Arrange
            var localizedString = new LocalizedString("Hello", format);

            var stringLocalizer = new Mock<IStringLocalizer>();
            stringLocalizer.Setup(s => s["Hello"]).Returns(localizedString);

            var htmlLocalizer = new HtmlLocalizer(stringLocalizer.Object);

            // Act
            var localizedHtmlString = htmlLocalizer.GetHtml("Hello", arguments);

            // Assert
            Assert.NotNull(localizedHtmlString);
            Assert.Equal(format, localizedHtmlString.Value);
            using (var writer = new StringWriter())
            {
                localizedHtmlString.WriteTo(writer, new HtmlTestEncoder());
                Assert.Equal(expectedText, writer.ToString());
            }
        }

        public static TheoryData<string> InvalidResourceStringData
        {
            get
            {
                return new TheoryData<string>
                {
                    "{0",
                    "{"
                };
            }
        }

        [Theory]
        [ReplaceCulture]
        [MemberData(nameof(InvalidResourceStringData))]
        public void HtmlLocalizer_HtmlWithInvalidResourceString_ContentThrowsException(string format)
        {
            // Arrange
            var stringLocalizer = new TestStringLocalizer();

            var htmlLocalizer = new HtmlLocalizer(stringLocalizer);
            var content = htmlLocalizer.GetHtml(format, new object[] { });

            // Act
            var exception = Assert.Throws<FormatException>(
                () => content.WriteTo(TextWriter.Null, new HtmlTestEncoder()));

            // Assert
            Assert.NotNull(exception);
            Assert.Equal("Input string was not in a correct format.", exception.Message);
        }

        /// <summary>
        /// This tests that LocalizedString instances containing curly braces (e.g. JSON or code snippets) can be used without a FormatException when no arguments are provided.
        /// </summary>
        [Fact]
        public void HtmlLocalizer_HtmlWithJsonText_ReturnsLocalizedHtmlString()
        {
            // Arrange
            var stringLocalizer = new TestStringLocalizer();

            var htmlLocalizer = new HtmlLocalizer(stringLocalizer);

            // Act
            var localizedHtmlString = htmlLocalizer.GetHtml("example JSON: {\"foo\":\"bar\"}");

            // Assert
            Assert.Equal("example JSON: {\"foo\":\"bar\"}", localizedHtmlString.Name);
            Assert.Equal("Hello example JSON: {\"foo\":\"bar\"}", localizedHtmlString.Value);
            using (var writer = new StringWriter())
            {
                localizedHtmlString.WriteTo(writer, new HtmlTestEncoder());
                Assert.Equal("Hello example JSON: {\"foo\":\"bar\"}", writer.ToString());
            }
        }

        [Fact]
        public void HtmlLocalizer_GetString_ReturnsLocalizedString()
        {
            // Arrange
            var stringLocalizer = new TestStringLocalizer();

            var htmlLocalizer = new HtmlLocalizer(stringLocalizer);

            // Act
            var actualLocalizedString = htmlLocalizer.GetString("John");

            // Assert
            Assert.Equal("Hello John", actualLocalizedString.Value);
        }

        [Fact]
        public void HtmlLocalizer_GetStringWithArguments_ReturnsLocalizedString()
        {
            // Arrange
            var stringLocalizer = new TestStringLocalizer();

            var htmlLocalizer = new HtmlLocalizer(stringLocalizer);

            // Act
            var actualLocalizedString = htmlLocalizer.GetString("John", "Doe");

            // Assert
            Assert.Equal("Hello John Doe", actualLocalizedString.Value);
        }

        [Fact]
        public void HtmlLocalizer_Html_ReturnsLocalizedHtmlString()
        {
            // Arrange
            var stringLocalizer = new TestStringLocalizer();

            var htmlLocalizer = new HtmlLocalizer(stringLocalizer);

            // Act
            var actualLocalizedHtmlString = htmlLocalizer.GetHtml("John");

            // Assert
            Assert.Equal("Hello John", actualLocalizedHtmlString.Value);
        }

        [Fact]
        public void HtmlLocalizer_WithCulture_ReturnsLocalizedHtmlString()
        {
            // Arrange
            var stringLocalizer = new TestStringLocalizer();

            var htmlLocalizer = new HtmlLocalizer(stringLocalizer);

            // Act
            var actualLocalizedHtmlString = htmlLocalizer.WithCulture(new CultureInfo("fr"))["John"];

            // Assert
            Assert.Equal("Bonjour John", actualLocalizedHtmlString.Value);
        }

        [Fact]
        public void HtmlLocalizer_GetAllStrings_ReturnsAllLocalizedStrings()
        {
            // Arrange
            var stringLocalizer = new TestStringLocalizer();

            var htmlLocalizer = new HtmlLocalizer(stringLocalizer);

            // Act
            var allLocalizedStrings = htmlLocalizer.GetAllStrings(includeParentCultures: false).ToList();

            //Assert
            Assert.Equal(1, allLocalizedStrings.Count);
            Assert.Equal("World", allLocalizedStrings.First().Value);
        }

        [Fact]
        public void HtmlLocalizer_GetAllStringsIncludeParentCulture_ReturnsAllLocalizedStrings()
        {
            // Arrange
            var stringLocalizer = new TestStringLocalizer();

            var htmlLocalizer = new HtmlLocalizer(stringLocalizer);

            // Act
            var allLocalizedStrings = htmlLocalizer.GetAllStrings().ToList();

            //Assert
            Assert.Equal(2, allLocalizedStrings.Count);
            Assert.Equal("World", allLocalizedStrings[0].Value);
            Assert.Equal("Bar", allLocalizedStrings[1].Value);
        }
    }
}
