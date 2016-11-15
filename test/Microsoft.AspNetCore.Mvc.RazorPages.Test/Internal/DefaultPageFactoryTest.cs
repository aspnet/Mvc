// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Reflection;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    public class DefaultPageFactoryTest
    {
        [Fact]
        public void CreatePage_ThrowsIfActivatedInstanceIsNotAnInstanceOfRazorPage()
        {
            // Arrange
            var pageContext = new PageContext
            {
                ActionDescriptor = new PageActionDescriptor
                {
                    PageTypeInfo = typeof(object).GetTypeInfo(),
                }
            };
            var pageActivator = CreateActivator();
            var pageFactory = CreatePageFactory();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => pageFactory.CreatePage(pageContext));
            Assert.Equal(
                $"Page created by '{pageActivator.GetType()}' must be an instance of '{typeof(Page)}'.",
                ex.Message);
        }

        [Fact]
        public void CreatePage_SetsPageContext()
        {
            // Arrange
            var pageContext = new PageContext
            {
                ActionDescriptor = new PageActionDescriptor
                {
                    PageTypeInfo = typeof(TestPage).GetTypeInfo(),
                }
            };
            var pageFactory = CreatePageFactory();

            // Act
            var instance = pageFactory.CreatePage(pageContext);

            // Assert
            var testPage = Assert.IsType<TestPage>(instance);
            Assert.Same(pageContext, testPage.PageContext);
        }

        [Fact]
        public void CreatePage_SetsPropertiesWithRazorInject()
        {
            // Arrange
            var pageContext = new PageContext
            {
                ActionDescriptor = new PageActionDescriptor
                {
                    PageTypeInfo = typeof(TestPage).GetTypeInfo(),
                }
            };
            var urlHelperFactory = new Mock<IUrlHelperFactory>();
            var urlHelper = Mock.Of<IUrlHelper>();
            urlHelperFactory.Setup(f => f.GetUrlHelper(pageContext))
                .Returns(urlHelper)
                .Verifiable();
            var htmlEncoder = HtmlEncoder.Create();

            var pageFactory = CreatePageFactory(
                urlHelperFactory: urlHelperFactory.Object,
                htmlEncoder: htmlEncoder);

            // Act
            var instance = pageFactory.CreatePage(pageContext);

            // Assert
            var testPage = Assert.IsType<TestPage>(instance);
            Assert.Same(urlHelper, testPage.UrlHelper);
            Assert.Same(htmlEncoder, testPage.HtmlEncoder);
            Assert.NotNull(testPage.ViewData);
        }

        [Fact]
        public void CreatePage_SetViewDataWithModelTypeWhenNotNull()
        {
            // Arrange
            var pageContext = new PageContext
            {
                ActionDescriptor = new PageActionDescriptor
                {
                    PageTypeInfo = typeof(ViewDataTestPage).GetTypeInfo(),
                },
                ModelType = typeof(ViewDataTestPageModel).GetTypeInfo(),
            };

            var pageFactory = CreatePageFactory();

            // Act
            var instance = pageFactory.CreatePage(pageContext);

            // Assert
            var testPage = Assert.IsType<ViewDataTestPage>(instance);
            Assert.NotNull(testPage.ViewData);
        }

        private static DefaultPageFactory CreatePageFactory(
            IPageActivator pageActivator = null,
            IModelMetadataProvider provider = null,
            IUrlHelperFactory urlHelperFactory = null,
            IJsonHelper jsonHelper = null,
            DiagnosticSource diagnosticSource = null,
            HtmlEncoder htmlEncoder = null,
            IModelExpressionProvider modelExpressionProvider = null)
        {
            return new DefaultPageFactory(
                pageActivator ?? CreateActivator(),
                provider ?? Mock.Of<IModelMetadataProvider>(),
                urlHelperFactory ?? Mock.Of<IUrlHelperFactory>(),
                jsonHelper ?? Mock.Of<IJsonHelper>(),
                diagnosticSource ?? new DiagnosticListener("Microsoft.AspNetCore.Mvc.RazorPages"),
                htmlEncoder ?? HtmlEncoder.Default,
                modelExpressionProvider ?? Mock.Of<IModelExpressionProvider>());
        }

        private static IPageActivator CreateActivator()
        {
            var activator = new Mock<IPageActivator>();
            activator.Setup(a => a.Create(It.IsAny<PageContext>()))
                .Returns((PageContext context) =>
                {
                    return Activator.CreateInstance(context.ActionDescriptor.PageTypeInfo.AsType());
                });
            return activator.Object;
        }

        private class TestPage : Page
        {
            [RazorInject]
            public IUrlHelper UrlHelper { get; set; }

            [RazorInject]
            public HtmlEncoder HtmlEncoder { get; set; }

            [RazorInject]
            public ViewDataDictionary<TestPage> ViewData { get; set; }
        }

        private class ViewDataTestPage : Page
        {
            [RazorInject]
            public ViewDataDictionary<ViewDataTestPageModel> ViewData { get; set; }
        }

        private class ViewDataTestPageModel
        {
        }
    }
}
