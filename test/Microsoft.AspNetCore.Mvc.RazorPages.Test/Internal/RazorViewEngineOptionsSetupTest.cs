// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    public class RazorPagesRazorViewEngineOptionsSetupTest
    {
        [Fact]
        public void Configure_AddsPageViewLocationFormats_WhenPagesRootIsAppRoot()
        {
            // Arrange
            var expected = new[]
            {
                "/{1}/{0}.cshtml",
                "/Views/Shared/{0}.cshtml",
                "/Shared/{0}.cshtml",
            };

            var razorPagesOptions = new RazorPagesOptions
            {
                RootDirectory = "/"
            };
            var viewEngineOptions = new RazorViewEngineOptions();
            var setup = new RazorPagesRazorViewEngineOptionsSetup(
                new TestOptionsManager<RazorPagesOptions>(razorPagesOptions));

            // Act
            setup.Configure(viewEngineOptions);

            // Assert
            Assert.Equal(expected, viewEngineOptions.PageViewLocationFormats);
        }

        [Fact]
        public void Configure_AddsPageViewLocationFormats_WithDefaultPagesRoot()
        {
            // Arrange
            var expected = new[]
            {
                "/Pages/{1}/{0}.cshtml",
                "/Views/Shared/{0}.cshtml",
                "/Pages/Shared/{0}.cshtml",
            };

            var razorPagesOptions = new RazorPagesOptions();
            var viewEngineOptions = new RazorViewEngineOptions();
            var setup = new RazorPagesRazorViewEngineOptionsSetup(
                new TestOptionsManager<RazorPagesOptions>(razorPagesOptions));

            // Act
            setup.Configure(viewEngineOptions);

            // Assert
            Assert.Equal(expected, viewEngineOptions.PageViewLocationFormats);
        }

        [Fact]
        public void Configure_RegistersPageViewLocationExpander()
        {
            // Arrange
            var viewEngineOptions = new RazorViewEngineOptions();
            var setup = new RazorPagesRazorViewEngineOptionsSetup(new TestOptionsManager<RazorPagesOptions>());

            // Act
            setup.Configure(viewEngineOptions);

            // Assert
            Assert.Collection(
                viewEngineOptions.ViewLocationExpanders,
                expander => Assert.IsType<PageViewLocationExpander>(expander));
        }
    }
}
