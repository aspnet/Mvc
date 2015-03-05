// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Core;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class BodyModelBinderTests
    {
        [Fact]
        public async Task BindModel_CallsSelectedInputFormatterOnce()
        {
            // Arrange
            var mockInputFormatter = new Mock<IInputFormatter>();
            mockInputFormatter.Setup(o => o.ReadAsync(It.IsAny<InputFormatterContext>()))
                              .Returns(Task.FromResult<object>(new Person()))
                              .Verifiable();

            var provider = new TestModelMetadataProvider();
            provider.ForType<Person>().BindingDetails(d => d.BindingSource = BindingSource.Body);

            var bindingContext = GetBindingContext(typeof(Person), metadataProvider: provider);

            var binder = GetBodyBinder(mockInputFormatter.Object);

            // Act
            var binderResult = await binder.BindModelAsync(bindingContext);

            // Assert
            mockInputFormatter.Verify(v => v.ReadAsync(It.IsAny<InputFormatterContext>()), Times.Once);
            Assert.NotNull(binderResult);
            Assert.True(binderResult.IsModelSet);
        }

        [Fact]
        public async Task BindModel_NoInputFormatterFound_SetsModelStateError()
        {
            // Arrange
            var provider = new TestModelMetadataProvider();
            provider.ForType<Person>().BindingDetails(d => d.BindingSource = BindingSource.Body);

            var bindingContext = GetBindingContext(typeof(Person), metadataProvider: provider);

            var binder = bindingContext.OperationBindingContext.ModelBinder;

            // Act
            var binderResult = await binder.BindModelAsync(bindingContext);

            // Assert

            // Returns true because it understands the metadata type.
            Assert.NotNull(binderResult);
            Assert.False(binderResult.IsModelSet);
            Assert.Null(binderResult.Model);
            Assert.True(bindingContext.ModelState.ContainsKey("someName"));
        }

        [Fact]
        public async Task BindModel_IsGreedy()
        {
            // Arrange
            var provider = new TestModelMetadataProvider();
            provider.ForType<Person>().BindingDetails(d => d.BindingSource = BindingSource.Body);

            var bindingContext = GetBindingContext(typeof(Person), metadataProvider: provider);

            var binder = bindingContext.OperationBindingContext.ModelBinder;

            // Act
            var binderResult = await binder.BindModelAsync(bindingContext);

            // Assert
            Assert.NotNull(binderResult);
            Assert.False(binderResult.IsModelSet);
        }

        [Fact]
        public async Task BindModel_IsGreedy_IgnoresWrongSource()
        {
            // Arrange
            var provider = new TestModelMetadataProvider();
            provider.ForType<Person>().BindingDetails(d => d.BindingSource = BindingSource.Header);

            var bindingContext = GetBindingContext(typeof(Person), metadataProvider: provider);

            var binder = bindingContext.OperationBindingContext.ModelBinder;

            // Act
            var binderResult = await binder.BindModelAsync(bindingContext);

            // Assert
            Assert.Null(binderResult);
        }

        [Fact]
        public async Task BindModel_IsGreedy_IgnoresMetadataWithNoSource()
        {
            // Arrange
            var provider = new TestModelMetadataProvider();
            provider.ForType<Person>().BindingDetails(d => d.BindingSource = null);

            var bindingContext = GetBindingContext(typeof(Person), metadataProvider: provider);

            var binder = bindingContext.OperationBindingContext.ModelBinder;

            // Act
            var binderResult = await binder.BindModelAsync(bindingContext);

            // Assert
            Assert.Null(binderResult);
        }

        [Fact]
        public async Task CustomFormatterDeserializationException_AddedToModelState()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("Bad data!"));
            httpContext.Request.ContentType = "text/xyz";

            var provider = new TestModelMetadataProvider();
            provider.ForType<Person>().BindingDetails(d => d.BindingSource = BindingSource.Body);

            var bindingContext = GetBindingContext(
                typeof(Person),
                inputFormatter: new XyzFormatter(),
                httpContext: httpContext,
                metadataProvider: provider);

            var binder = bindingContext.OperationBindingContext.ModelBinder;

            // Act
            var binderResult = await binder.BindModelAsync(bindingContext);

            // Assert

            // Returns true because it understands the metadata type.
            Assert.NotNull(binderResult);
            Assert.False(binderResult.IsModelSet);
            Assert.Null(binderResult.Model);
            Assert.True(bindingContext.ModelState.ContainsKey("someName"));
            var errorMessage = bindingContext.ModelState["someName"].Errors[0].Exception.Message;
            Assert.Equal("Your input is bad!", errorMessage);
        }

        private static ModelBindingContext GetBindingContext(
            Type modelType,
            IInputFormatter inputFormatter = null,
            HttpContext httpContext = null,
            IModelMetadataProvider metadataProvider = null)
        {
            if (httpContext == null)
            {
                httpContext = new DefaultHttpContext();
            }

            if (metadataProvider == null)
            {
                metadataProvider = new EmptyModelMetadataProvider();
            }

            var operationBindingContext = new OperationBindingContext
            {
                ModelBinder = GetBodyBinder(httpContext, inputFormatter),
                MetadataProvider = metadataProvider,
                HttpContext = httpContext,
            };

            var bindingContext = new ModelBindingContext
            {
                ModelMetadata = metadataProvider.GetMetadataForType(modelType),
                ModelName = "someName",
                ValueProvider = Mock.Of<IValueProvider>(),
                ModelState = new ModelStateDictionary(),
                OperationBindingContext = operationBindingContext,
            };

            return bindingContext;
        }

        private static BodyModelBinder GetBodyBinder(IInputFormatter inputFormatter)
        {
            return GetBodyBinder(new DefaultHttpContext(), inputFormatter);
        }

        private static BodyModelBinder GetBodyBinder(HttpContext httpContext, IInputFormatter inputFormatter)
        {
            var actionContext = CreateActionContext(httpContext);
            var inputFormatterSelector = new Mock<IInputFormatterSelector>();
            inputFormatterSelector
                .Setup(o => o.SelectFormatter(
                    It.IsAny<IReadOnlyList<IInputFormatter>>(),
                    It.IsAny<InputFormatterContext>()))
                .Returns(inputFormatter);

            var bodyValidationPredicatesProvider = new Mock<IValidationExcludeFiltersProvider>();
            bodyValidationPredicatesProvider.SetupGet(o => o.ExcludeFilters)
                                             .Returns(new List<IExcludeTypeValidationFilter>());

            var bindingContext = new ActionBindingContext()
            {
                InputFormatters = new List<IInputFormatter>(),
            };

            var bindingContextAccessor = new MockScopedInstance<ActionBindingContext>()
            {
                Value = bindingContext,
            };

            var binder = new BodyModelBinder(
                actionContext,
                bindingContextAccessor,
                inputFormatterSelector.Object,
                bodyValidationPredicatesProvider.Object);

            return binder;
        }

        private static IScopedInstance<ActionContext> CreateActionContext(HttpContext context)
        {
            return CreateActionContext(context, (new Mock<IRouter>()).Object);
        }

        private static IScopedInstance<ActionContext> CreateActionContext(HttpContext context, IRouter router)
        {
            var routeData = new RouteData();
            routeData.Routers.Add(router);

            var actionContext = new ActionContext(context,
                                                  routeData,
                                                  new ActionDescriptor());
            var contextAccessor = new Mock<IScopedInstance<ActionContext>>();
            contextAccessor.SetupGet(c => c.Value)
                           .Returns(actionContext);
            return contextAccessor.Object;
        }

        private class Person
        {
            public string Name { get; set; }
        }

        private class XyzFormatter : InputFormatter
        {
            public XyzFormatter()
            {
                SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/xyz"));
                SupportedEncodings.Add(Encoding.UTF8);
            }

            protected override bool CanReadType(Type type)
            {
                return true;
            }

            public override Task<object> ReadRequestBodyAsync(InputFormatterContext context)
            {
                throw new InvalidOperationException("Your input is bad!");
            }
        }
    }
}