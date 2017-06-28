﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection
{
    public class MvcRazorPagesMvcBuilderExtensionsTest
    {
        [Fact]
        public void AddRazorPagesOptions_AddsApplicationModelConventions()
        {
            // Arrange
            var services = new ServiceCollection().AddOptions();
            var applicationModelConvention = Mock.Of<IPageApplicationModelConvention>();
            var routeModelConvention = Mock.Of<IPageRouteModelConvention>();
            var builder = new MvcBuilder(services, new ApplicationPartManager());
            builder.AddRazorPagesOptions(options =>
            {
                options.ApplicationModelConventions.Add(applicationModelConvention);
                options.RouteModelConventions.Add(routeModelConvention);
            });
            var serviceProvider = services.BuildServiceProvider();
            var accessor = serviceProvider.GetRequiredService<IOptions<RazorPagesOptions>>();

            // Act & Assert
            var conventions = accessor.Value.ApplicationModelConventions;

            // Assert
            Assert.Collection(conventions,
                convention => Assert.Same(applicationModelConvention, convention));
        }
    }
}
