// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Razor.Language;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    public class PageDirectiveFeatureTest
    {
        [Fact]
        public void TryGetPageDirective_WithPageDirectivePrefixedWithoutAtSymbol_DoesNotParse()
        {
            // Arrange
            var projectItem = new TestRazorProjectItem(@"page ""Some/Path/{value}""");

            // Act & Assert
            Assert.False(PageDirectiveFeature.TryGetPageDirective(projectItem, out var directive));
        }

        [Fact]
        public void TryGetPageDirective_WithPageDirectiveFollowedByNoSpaces_DoesNotParse()
        {
            // Arrange
            var projectItem = new TestRazorProjectItem(@"@page""Some/Path/{value}""
The rest of the thing");

            // Act & Assert
            Assert.False(PageDirectiveFeature.TryGetPageDirective(projectItem, out var directive));
        }

        [Fact]
        public void TryGetPageDirective_FindsTemplate()
        {
            // Arrange
            var projectItem = new TestRazorProjectItem(@"@page ""Some/Path/{value}""
The rest of the thing");

            // Act & Assert
            Assert.True(PageDirectiveFeature.TryGetPageDirective(projectItem, out var directive));
            Assert.Equal("Some/Path/{value}", directive.RouteTemplate);
            Assert.Null(directive.Name);
        }

        [Fact]
        public void TryGetPageDirective_NoNewLine()
        {
            // Arrange
            var projectItem = new TestRazorProjectItem(@"@page ""Some/Path/{value}""");

            // Act & Assert
            Assert.True(PageDirectiveFeature.TryGetPageDirective(projectItem, out var directive));
            Assert.Equal("Some/Path/{value}", directive.RouteTemplate);
            Assert.Null(directive.Name);
        }

        [Fact]
        public void TryGetPageDirective_JunkBeforeDirective()
        {
            // Arrange
            var projectItem = new TestRazorProjectItem(@"Not a directive @page ""Some/Path/{value}""");

            // Act & Assert
            Assert.False(PageDirectiveFeature.TryGetPageDirective(projectItem, out var directive));
        }

        [Fact]
        public void TryGetPageDirective_MultipleQuotes()
        {
            // Arrange
            var projectItem = new TestRazorProjectItem(@"@page """"template""""");

            // Act & Assert
            Assert.True(PageDirectiveFeature.TryGetPageDirective(projectItem, out var directive));
            Assert.Equal(@"""template""", directive.RouteTemplate);
            Assert.Null(directive.Name);
        }

        [Theory]
        [InlineData(@"""Some/Path/{value}")]
        [InlineData(@"Some/Path/{value}""")]
        public void TryGetPageDirective_RequiresBothQuotes(string inTemplate)
        {
            // Arrange
            var projectItem = new TestRazorProjectItem($@"@page {inTemplate}");

            // Act & Assert
            Assert.False(PageDirectiveFeature.TryGetPageDirective(projectItem, out var directive));
        }

        [Fact]
        public void TryGetPageDirective_NoQuotesAroundPath_IsNotTemplate()
        {
            // Arrange
            var projectItem = new TestRazorProjectItem(@"@page Some/Path/{value}");

            // Act & Assert
            Assert.False(PageDirectiveFeature.TryGetPageDirective(projectItem, out var directive));
        }

        [Fact]
        public void TryGetPageDirective_ParsePageName()
        {
            // Arrange
            var projectItem = new TestRazorProjectItem("@page \"Some/Path/{value}\" \"page-name\"");

            // Act & Assert
            Assert.True(PageDirectiveFeature.TryGetPageDirective(projectItem, out var directive));
            Assert.Equal("Some/Path/{value}", directive.RouteTemplate);
            Assert.Equal("page-name", directive.Name);
        }

        [Fact]
        public void TryGetPageDirective_DoesNotParseUnquotedPageName()
        {
            // Arrange
            var projectItem = new TestRazorProjectItem("@page \"Some/Path/{value}\" page-name");

            // Act & Assert
            Assert.False(PageDirectiveFeature.TryGetPageDirective(projectItem, out var directive));
        }

        [Fact]
        public void TryGetPageDirective_WrongNewLine()
        {
            // Arrange
            var wrongNewLine = Environment.NewLine == "\r\n" ? "\n" : "\r\n";

            var projectItem = new TestRazorProjectItem($"@page \"Some/Path/{{value}}\" {wrongNewLine}");

            // Act & Assert
            Assert.True(PageDirectiveFeature.TryGetPageDirective(projectItem, out var directive));
            Assert.Equal("Some/Path/{value}", directive.RouteTemplate);
            Assert.Null(directive.Name);
        }

        [Fact]
        public void TryGetPageDirective_NewLineBeforeDirective()
        {
            // Arrange
            var projectItem = new TestRazorProjectItem("\n @page \"Some/Path/{value}\"");

            // Act
            Assert.True(PageDirectiveFeature.TryGetPageDirective(projectItem, out var directive));
            Assert.Equal("Some/Path/{value}", directive.RouteTemplate);
            Assert.Null(directive.Name);
        }

        [Fact]
        public void TryGetPageDirective_WhitespaceBeforeDirective()
        {
            // Arrange
            var projectItem = new TestRazorProjectItem(@"   @page ""Some/Path/{value}""
");

            // Act & Assert
            Assert.True(PageDirectiveFeature.TryGetPageDirective(projectItem, out var directive));
            Assert.Equal("Some/Path/{value}", directive.RouteTemplate);
            Assert.Null(directive.Name);
        }

        [Fact(Skip = "Re-evaluate this scenario after we use Razor to parse this stuff")]
        public void TryGetPageDirective_JunkBeforeNewline()
        {
            // Arrange
            var projectItem = new TestRazorProjectItem(@"@page ""Some/Path/{value}"" things that are not the path
a new line");

            // Act & Assert
            Assert.True(PageDirectiveFeature.TryGetPageDirective(projectItem, out var directive));
            Assert.Empty(directive.RouteTemplate);
        }

        [Fact]
        public void TryGetPageDirective_Directive_WithoutPathOrContent()
        {
            // Arrange
            var projectItem = new TestRazorProjectItem(@"@page");

            // Act & Assert
            Assert.True(PageDirectiveFeature.TryGetPageDirective(projectItem, out var directive));
            Assert.Null(directive.RouteTemplate);
        }

        [Fact]
        public void TryGetPageDirective_DirectiveWithContent_WithoutPath()
        {
            // Arrange
            var projectItem = new TestRazorProjectItem(@"@page
Non-path things");

            // Act & Assert
            Assert.True(PageDirectiveFeature.TryGetPageDirective(projectItem, out var directive));
            Assert.Null(directive.RouteTemplate);
        }

        [Fact]
        public void TryGetPageDirective_NoDirective()
        {
            // Arrange
            var projectItem = new TestRazorProjectItem(@"This is junk
Nobody will use it");

            // Act & Assert
            Assert.False(PageDirectiveFeature.TryGetPageDirective(projectItem, out var directive));
            Assert.Null(directive.RouteTemplate);
        }

        [Fact]
        public void TryGetPageDirective_EmptyStream()
        {
            // Arrange
            var projectItem = new TestRazorProjectItem(string.Empty);

            // Act
            Assert.False(PageDirectiveFeature.TryGetPageDirective(projectItem, out var directive));
            Assert.Null(directive.RouteTemplate);
        }

        [Fact]
        public void TryGetPageDirective_NullProject()
        {
            // Arrange

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => PageDirectiveFeature.TryGetPageDirective(projectItem: null, directive: out var directive));
        }

        [Fact]
        public void TryGetPageDirective_NullStream()
        {
            // Arrange
            var projectItem = new TestRazorProjectItem(content: null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => PageDirectiveFeature.TryGetPageDirective(projectItem, out var directive));
        }
    }

    public class TestRazorProjectItem : RazorProjectItem
    {
        private string _content;

        public TestRazorProjectItem(string content)
        {
            _content = content;
        }

        public override string BasePath
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool Exists
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string Path
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string PhysicalPath
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Stream Read()
        {
            if (_content == null)
            {
                return null;
            }
            else
            {
                return new MemoryStream(Encoding.UTF8.GetBytes(_content));
            }
        }
    }
}
