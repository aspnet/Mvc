// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    public class ControllerBoundPropertyFilterApplicationModelProviderTest
    {
        [Fact]
        public void OnProvidersExecuting_AddsFilter()
        {
            // Arrange
            var type = GetType();
            var provider = CreateProvider();
            var context = CreateContext(type);

            // Act
            provider.OnProvidersExecuting(context);

            // Assert
            var controller = Assert.Single(context.Result.Controllers);
            Assert.Collection(
                controller.Filters,
                f => Assert.IsType<ControllerBoundPropertyFilter>(f));
        }

        private static ControllerBoundPropertyFilterApplicationModelProvider CreateProvider()
        {
            return new ControllerBoundPropertyFilterApplicationModelProvider(
                Options.Create(new MvcViewOptions()),
                new TempDataDictionaryFactory(new TestTempDataProvider()),
                new ControllerViewDataDictionaryFactory(new EmptyModelMetadataProvider()));
        }

        private static ApplicationModelProviderContext CreateContext(Type type)
        {
            return new ApplicationModelProviderContext(new[] { type.GetTypeInfo() })
            {
                Result =
                {
                    Controllers =
                    {
                        new ControllerModel(type.GetTypeInfo(), Array.Empty<object>()),
                    }
                }
            };
        }
    }
}
