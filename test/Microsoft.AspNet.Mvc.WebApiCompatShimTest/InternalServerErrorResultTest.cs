// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Internal;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Abstractions;
using Microsoft.AspNet.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace System.Web.Http
{
    public class InternalServerErrorResultTest
    {
        [Fact]
        public async Task InternalServerErrorResult_SetsStatusCode()
        {
            // Arrange
            var context = new ActionContext(GetHttpContext(), new RouteData(), new ActionDescriptor());
            var result = new InternalServerErrorResult();

            // Act
            await result.ExecuteResultAsync(context);

            // Assert
            Assert.Equal(StatusCodes.Status500InternalServerError, context.HttpContext.Response.StatusCode);
        }

        private static IServiceCollection CreateServices()
        {
            var services = new ServiceCollection();

            services.AddTransient<ILoggerFactory, LoggerFactory>();

            return services;
        }

        private static HttpContext GetHttpContext()
        {
            var services = CreateServices();

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = services.BuildServiceProvider();

            return httpContext;
        }
    }
}
