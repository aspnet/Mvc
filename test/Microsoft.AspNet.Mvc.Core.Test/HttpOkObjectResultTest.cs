﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Internal;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Logging.Testing;
using Microsoft.Framework.OptionsModel;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class HttpOkObjectResultTest
    {
        public static TheoryData<object> ValuesData
        {
            get
            {
                return new TheoryData<object>
                {
                    null,
                    "Test string",
                    new Person
                    {
                        Id = 274,
                        Name = "George",
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(ValuesData))]
        public void HttpOkObjectResult_InitializesStatusCodeAndValue(object value)
        {
            // Arrange & Act
            var result = new HttpOkObjectResult(value);

            // Assert
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            Assert.Same(value, result.Value);
        }

        [Theory]
        [MemberData(nameof(ValuesData))]
        public async Task HttpOkObjectResult_SetsStatusCode(object value)
        {
            // Arrange
            var result = new HttpOkObjectResult(value);

            var httpContext = new DefaultHttpContext
            {
                RequestServices = CreateServices(),
            };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert
            Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
        }

        private IServiceProvider CreateServices()
        {
            var services = new ServiceCollection();
            services.Add(new ServiceDescriptor(
                typeof(ILogger<ObjectResult>),
                new Logger<ObjectResult>(NullLoggerFactory.Instance)));

            var optionsAccessor = new MockMvcOptionsAccessor();
            optionsAccessor.Options.OutputFormatters.Add(new JsonOutputFormatter());
            services.Add(new ServiceDescriptor(typeof(IOptions<MvcOptions>), optionsAccessor));

            var bindingContext = new ActionBindingContext
            {
                OutputFormatters = optionsAccessor.Options.OutputFormatters,
            };
            var bindingContextAccessor = new MockScopedInstance<ActionBindingContext>
            {
                Value = bindingContext,
            };
            services.Add(new ServiceDescriptor(typeof(IScopedInstance<ActionBindingContext>), bindingContextAccessor));

            return services.BuildServiceProvider();
        }

        private class Person
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
