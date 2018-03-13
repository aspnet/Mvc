// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    public class PageBoundPropertyFilterApplicationModelProviderTest
    {
        [Fact]
        public void OnProvidersExecuting_AddsFilter()
        {
            // Arrange
            var provider = CreateProvider();
            var actionDescriptor = new PageActionDescriptor();
            var pageType = typeof(TestPage).GetTypeInfo();
            var modelType = typeof(TestPageModel).GetTypeInfo();
            var applicationModel = new PageApplicationModel(actionDescriptor, modelType, Array.Empty<object>())
            {
                PageType = pageType,
            };
            var context = new PageApplicationModelProviderContext(actionDescriptor, pageType)
            {
                PageApplicationModel = applicationModel
            };

            // Act
            provider.OnProvidersExecuting(context);

            // Assert
            Assert.Collection(
                applicationModel.Filters,
                filter => Assert.IsType<PageBoundPropertyFilter>(filter));
        }

        private static PageBoundPropertyFilterApplicationModelProvider CreateProvider()
        {
            var options = Options.Create(new MvcViewOptions());
            var tempDataFactory = Mock.Of<ITempDataDictionaryFactory>();
            return new PageBoundPropertyFilterApplicationModelProvider(options, tempDataFactory);
        }

        private class TestPage : Page
        {
            public override Task ExecuteAsync()
            {
                throw new System.NotImplementedException();
            }
        }

        private class TestPageModel : PageModel
        {
        }
    }
}
