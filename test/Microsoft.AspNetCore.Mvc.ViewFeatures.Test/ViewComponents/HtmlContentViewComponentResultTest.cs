// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.WebEncoders.Testing;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc
{
    public class HtmlContentViewComponentResultTest
    {
        [Fact]
        public void Execute_WritesData_PreEncoded()
        {
            // Arrange
            var buffer = new MemoryStream();
            var viewComponentContext = GetViewComponentContext(Mock.Of<IView>(), buffer);

            var result = new HtmlContentViewComponentResult(new HtmlString("<Test />"));

            // Act
            result.Execute(viewComponentContext);
            buffer.Position = 0;

            // Assert
            Assert.Equal("<Test />", new StreamReader(buffer).ReadToEnd());
        }

        private static ViewComponentContext GetViewComponentContext(IView view, Stream stream)
        {
            var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider());
            var viewContext = new ViewContext(
                actionContext,
                view,
                viewData,
                new TempDataDictionary(actionContext.HttpContext, new SessionStateTempDataProvider()),
                TextWriter.Null,
                new HtmlHelperOptions());

            var writer = new StreamWriter(stream) { AutoFlush = true };

            var viewComponentDescriptor = new ViewComponentDescriptor()
            {
                TypeInfo = typeof(object).GetTypeInfo(),
            };

            var viewComponentContext = new ViewComponentContext(
                viewComponentDescriptor,
                new Dictionary<string, object>(),
                new HtmlTestEncoder(),
                viewContext,
                writer);

            return viewComponentContext;
        }
    }
}