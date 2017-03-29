﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Razor.Evolution;
using Microsoft.Extensions.FileProviders;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Razor.Host
{
    public class MvcRazorTemplateEngineTest
    {
        [Fact]
        public void GetDefaultImports_IncludesDefaultImports()
        {
            // Arrange
            var expectedImports = new[]
            {
                "@using System",
                "@using System.Linq",
                "@using System.Collections.Generic",
                "@using Microsoft.AspNetCore.Mvc",
                "@using Microsoft.AspNetCore.Mvc.Rendering",
                "@using Microsoft.AspNetCore.Mvc.ViewFeatures",
            };
            var mvcRazorTemplateEngine = new MvcRazorTemplateEngine(
                RazorEngine.Create(),
                GetRazorProject(new TestFileProvider()));

            // Act
            var imports = mvcRazorTemplateEngine.Options.DefaultImports;

            // Assert
            var importContent = GetContent(imports)
                .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                .Where(line => line.StartsWith("@using"));
            Assert.Equal(expectedImports, importContent);
        }

        [Fact]
        public void GetDefaultImports_IncludesDefaulInjects()
        {
            // Arrange
            var expectedImports = new[]
            {
                "@inject global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<TModel> Html",
                "@inject global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json",
                "@inject global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component",
                "@inject global::Microsoft.AspNetCore.Mvc.IUrlHelper Url",
                "@inject global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider",
            };
            var mvcRazorTemplateEngine = new MvcRazorTemplateEngine(
                RazorEngine.Create(),
                GetRazorProject(new TestFileProvider()));

            // Act
            var imports = mvcRazorTemplateEngine.Options.DefaultImports;

            // Assert
            var importContent = GetContent(imports)
                .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                .Where(line => line.StartsWith("@inject"));
            Assert.Equal(expectedImports, importContent);
        }

        [Fact]
        public void GetDefaultImports_IncludesUrlTagHelper()
        {
            // Arrange
            var mvcRazorTemplateEngine = new MvcRazorTemplateEngine(
                RazorEngine.Create(),
                GetRazorProject(new TestFileProvider()));

            // Act
            var imports = mvcRazorTemplateEngine.Options.DefaultImports;

            // Assert
            var importContent = GetContent(imports)
                .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                .Where(line => line.StartsWith("@addTagHelper"));

            Assert.Contains("@addTagHelper Microsoft.AspNetCore.Mvc.Razor.TagHelpers.UrlResolutionTagHelper, Microsoft.AspNetCore.Mvc.Razor",
                importContent);
            Assert.Contains("@addTagHelper Microsoft.AspNetCore.Mvc.Razor.TagHelpers.HeadTagHelper, Microsoft.AspNetCore.Mvc.Razor",
                importContent);
            Assert.Contains("@addTagHelper Microsoft.AspNetCore.Mvc.Razor.TagHelpers.BodyTagHelper, Microsoft.AspNetCore.Mvc.Razor",
                importContent);
        }

        [Fact]
        public void CreateCodeDocument_SetsRelativePathOnOutput()
        {
            // Arrange
            var path = "/Views/Home/Index.cshtml";
            var fileProvider = new TestFileProvider();
            fileProvider.AddFile(path, "Hello world");
            var mvcRazorTemplateEngine = new MvcRazorTemplateEngine(
                RazorEngine.Create(),
                GetRazorProject(fileProvider));

            // Act
            var codeDocument = mvcRazorTemplateEngine.CreateCodeDocument(path);

            // Assert
            Assert.Equal(path, codeDocument.GetRelativePath());
        }

        private string GetContent(RazorSourceDocument imports)
        {
            var contentChars = new char[imports.Length];
            imports.CopyTo(0, contentChars, 0, imports.Length);
            return new string(contentChars);
        }

        private static DefaultRazorProject GetRazorProject(IFileProvider fileProvider)
        {
            var fileProviderAccessor = new Mock<IRazorViewEngineFileProviderAccessor>();
            fileProviderAccessor.SetupGet(f => f.FileProvider)
                .Returns(fileProvider);

            return new DefaultRazorProject(fileProviderAccessor.Object);
        }
    }
}
