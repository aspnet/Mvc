﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.WebEncoders.Testing;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewComponents
{
    public class ViewComponentContextTest
    {
        [Fact]
        public void Constructor_PerformsDefensiveCopies()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider());
            var viewContext = new ViewContext(
                actionContext,
                NullView.Instance,
                viewData,
                new TempDataDictionary(httpContext, new SessionStateTempDataProvider()),
                TextWriter.Null,
                new HtmlHelperOptions());

            var viewComponentDescriptor = new ViewComponentDescriptor();

            // Act
            var viewComponentContext = new ViewComponentContext(
                viewComponentDescriptor,
                new Dictionary<string, object>(),
                new HtmlTestEncoder(),
                viewContext,
                TextWriter.Null);

            // Assert
            // New ViewContext but initial View and TextWriter copied over.
            Assert.NotSame(viewContext, viewComponentContext.ViewContext);
            Assert.Same(viewContext.View, viewComponentContext.ViewContext.View);
            Assert.Same(viewContext.Writer, viewComponentContext.ViewContext.Writer);

            // Double-check the convenience properties.
            Assert.Same(viewComponentContext.ViewContext.ViewData, viewComponentContext.ViewData);
            Assert.Same(viewComponentContext.ViewContext.Writer, viewComponentContext.Writer);

            // New VDD instance but initial ModelMetadata copied over.
            Assert.NotSame(viewData, viewComponentContext.ViewData);
            Assert.Same(viewData.ModelMetadata, viewComponentContext.ViewData.ModelMetadata);
        }

        public static TheoryData<object, Type> IncompatibleModelData
        {
            get
            {
                // Small "anything but int" grab bag of instances and expected types.
                return new TheoryData<object, Type>
                {
                    { null, typeof(object) },
                    { true, typeof(bool) },
                    { 43.78, typeof(double) },
                    { "test string", typeof(string) },
                    { new List<int>(), typeof(List<int>) },
                    { new List<string>(), typeof(List<string>) },
                };
            }
        }

        [Theory]
        [MemberData(nameof(IncompatibleModelData))]
        public void ViewDataModelSetter_DoesNotThrow_IfValueIncompatibleWithSourceDeclaredType(
            object model,
            Type expectedType)
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var viewData = new ViewDataDictionary<int>(new EmptyModelMetadataProvider());
            var viewContext = new ViewContext(
                actionContext,
                NullView.Instance,
                viewData,
                new TempDataDictionary(httpContext, new SessionStateTempDataProvider()),
                TextWriter.Null,
                new HtmlHelperOptions());

            var viewComponentDescriptor = new ViewComponentDescriptor();
            var viewComponentContext = new ViewComponentContext(
                viewComponentDescriptor,
                new Dictionary<string, object>(),
                new HtmlTestEncoder(),
                viewContext,
                TextWriter.Null);

            // Act (does not throw)
            // Non-ints can be assigned despite type restrictions in the source ViewDataDictionary.
            viewComponentContext.ViewData.Model = model;

            // Assert
            Assert.Equal(expectedType, viewComponentContext.ViewData.ModelMetadata.ModelType);
        }
    }
}
