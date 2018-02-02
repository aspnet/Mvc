// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Razor.Test.TagHelpers
{
    public class DefaultTagHelperComponentContextInitializerTest
    {
        [Fact]
        public void InitializesViewContext()
        {
            // Arrange
            var tagHelperComponent = new TestTagHelperComponent();
            var viewContext = CreateViewContext(new DefaultHttpContext());
            var viewDataValue = "Value";
            viewContext.ViewData.Add("TestData", viewDataValue);
            var contextInitializer = new DefaultTagHelperComponentContextInitializer();

            // Act
            contextInitializer.InitializeViewContext(tagHelperComponent, viewContext);

            // Assert
            Assert.Same(viewContext, tagHelperComponent.ViewContext);
        }

        private class TestTagHelperComponent : ITagHelperComponent
        {
            public int Order => 1;

            [ViewContext]
            public ViewContext ViewContext { get; set; }

            public void Init(TagHelperContext context)
            {
                context.Items["Key"] = "Value";
            }

            public Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
            {
                return Task.CompletedTask;
            }
        }

        private static ViewContext CreateViewContext(HttpContext httpContext)
        {
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var metadataProvider = new EmptyModelMetadataProvider();
            var viewData = new ViewDataDictionary(metadataProvider, new ModelStateDictionary());
            var viewContext = new ViewContext(
                actionContext,
                Mock.Of<IView>(),
                viewData,
                Mock.Of<ITempDataDictionary>(),
                TextWriter.Null,
                new HtmlHelperOptions());

            return viewContext;
        }
    }
}
