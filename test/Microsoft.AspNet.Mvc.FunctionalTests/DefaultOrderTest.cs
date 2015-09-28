// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Abstractions;
using Microsoft.AspNet.Mvc.ActionConstraints;
using Microsoft.AspNet.Mvc.ApiExplorer;
using Microsoft.AspNet.Mvc.Controllers;
using Microsoft.AspNet.Mvc.Filters;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    // Tests that various MVC services have the correct order.
    public class DefaultOrderTest : IClassFixture<MvcTestFixture<BasicWebSite.Startup>>
    {
        public DefaultOrderTest(MvcTestFixture<BasicWebSite.Startup> fixture)
        {
            Client = fixture.Client;
        }

        public HttpClient Client { get; }

        [Theory]
        [InlineData(typeof(IActionDescriptorProvider), typeof(ControllerActionDescriptorProvider), -1000)]
        [InlineData(typeof(IActionInvokerProvider), null, -1000)]
        [InlineData(typeof(IApiDescriptionProvider), null, -1000)]
        [InlineData(typeof(IFilterProvider), null, -1000)]
        [InlineData(typeof(IActionConstraintProvider), null, -1000)]
        public async Task ServiceOrder_GetOrder(Type serviceType, Type actualType, int order)
        {
            // Arrange
            var url = "http://localhost/Order/GetServiceOrder?serviceType=" + serviceType.AssemblyQualifiedName;

            if (actualType != null)
            {
                url += "&actualType=" + actualType.AssemblyQualifiedName;
            }

            // Act
            var response = await Client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(order, int.Parse(content));
        }
    }
}