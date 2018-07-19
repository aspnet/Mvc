// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.Mvc
{
    public class ObjectResultTests
    {
        [Fact]
        public void ObjectResult_Constructor()
        {
            // Arrange
            var input = "testInput";

            // Act
            var result = new ObjectResult(input);

            // Assert
            Assert.Equal(input, result.Value);
            Assert.Empty(result.ContentTypes);
            Assert.Empty(result.Formatters);
            Assert.Null(result.StatusCode);
            Assert.Null(result.DeclaredType);
        }

        [Fact]
        public async Task ObjectResult_ExecuteResultAsync_SetsStatusCode()
        {
            // Arrange
            var result = new ObjectResult("Hello")
            {
                StatusCode = 404,
                Formatters = new FormatterCollection<IOutputFormatter>()
                {
                    new NoOpOutputFormatter(),
                },
            };

            var actionContext = new ActionContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    RequestServices = CreateServices(),
                }
            };

            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert
            Assert.Equal(404, actionContext.HttpContext.Response.StatusCode);
        }

        [Fact]
        public async Task ObjectResult_ExecuteResultAsync_SetsProblemDetailsStatus()
        {
            // Arrange
            var modelState = new ModelStateDictionary();

            var details = new ValidationProblemDetails(modelState);

            var result = new ObjectResult(details)
            {
                StatusCode = StatusCodes.Status422UnprocessableEntity,
                Formatters = new FormatterCollection<IOutputFormatter>()
                {
                    new NoOpOutputFormatter(),
                },
            };

            var actionContext = new ActionContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    RequestServices = CreateServices(),
                }
            };

            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert
            Assert.Equal(StatusCodes.Status422UnprocessableEntity, details.Status.Value);
        }

        private static IServiceProvider CreateServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IActionResultExecutor<ObjectResult>>(new ObjectResultExecutor(
                new DefaultOutputFormatterSelector(Options.Create(new MvcOptions()), NullLoggerFactory.Instance),
                new TestHttpResponseStreamWriterFactory(),
                NullLoggerFactory.Instance));
            services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

            return services.BuildServiceProvider();
        }

        private class NoOpOutputFormatter : IOutputFormatter
        {
            public bool CanWriteResult(OutputFormatterCanWriteContext context)
            {
                return true;
            }

            public Task WriteAsync(OutputFormatterWriteContext context)
            {
                return Task.FromResult(0);
            }
        }
    }
}