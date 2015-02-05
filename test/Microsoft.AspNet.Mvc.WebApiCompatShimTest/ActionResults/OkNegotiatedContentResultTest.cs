﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if ASPNET50
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.Http.Core;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.OptionsModel;
using Moq;
using Xunit;

namespace System.Web.Http
{
    public class OkNegotiatedContentResultTest
    {
        [Fact]
        public async Task OkNegotiatedContentResult_SetsStatusCode()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = CreateServices();

            var stream = new MemoryStream();
            httpContext.Response.Body = stream;

            var context = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var result = new OkNegotiatedContentResult<Product>(new Product());

            // Act
            await result.ExecuteResultAsync(context);

            // Assert
            Assert.Equal(200, context.HttpContext.Response.StatusCode);
        }

        private IServiceProvider CreateServices()
        {
            var services = new Mock<IServiceProvider>(MockBehavior.Strict);

            var formatters = new Mock<IOutputFormattersProvider>(MockBehavior.Strict);
            formatters
                .SetupGet(f => f.OutputFormatters)
                .Returns(new List<IOutputFormatter>() { new JsonOutputFormatter(), });

            services
                .Setup(s => s.GetService(typeof(IOutputFormattersProvider)))
                .Returns(formatters.Object);

            var options = new Mock<IOptions<MvcOptions>>();
            options.SetupGet(o => o.Options)
                       .Returns(new MvcOptions());

            services.Setup(s => s.GetService(typeof(IOptions<MvcOptions>)))
                       .Returns(options.Object);

            return services.Object;
        }

        private class Product
        {
            public int Id { get; set; }

            public string Name { get; set; }
        };
    }
}
#endif