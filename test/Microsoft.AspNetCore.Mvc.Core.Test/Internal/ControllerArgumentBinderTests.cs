// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Test;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Internal
{ 
    public class ControllerArgumentBinderTests
    {
        [Fact]
        public async Task BindActionArgumentsAsync_DoesNotAddActionArguments_IfBinderReturnsNull()
        {
            // Arrange
            var actionDescriptor = GetActionDescriptor();
            actionDescriptor.Parameters.Add(
                new ParameterDescriptor
                {
                    Name = "foo",
                    ParameterType = typeof(object),
                    BindingInfo = new BindingInfo(),
                });

            var binder = new Mock<IModelBinder>();
            binder
                .Setup(b => b.BindModelAsync(It.IsAny<DefaultModelBindingContext>()))
                .Returns(TaskCache.CompletedTask);

            var controllerContext = GetControllerContext(actionDescriptor);
            controllerContext.ModelBinders.Add(binder.Object);
            controllerContext.ValueProviders.Add(new SimpleValueProvider());

            var modelMetadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            var argumentBinder = GetArgumentBinder();

            // Act
            var result = await argumentBinder
                .BindActionArgumentsAsync(controllerContext, new TestController());

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task BindActionArgumentsAsync_DoesNotAddActionArguments_IfBinderDoesNotSetModel()
        {
            // Arrange
            var actionDescriptor = GetActionDescriptor();
            actionDescriptor.Parameters.Add(
                new ParameterDescriptor
                {
                    Name = "foo",
                    ParameterType = typeof(object),
                    BindingInfo = new BindingInfo(),
                });

            var binder = new Mock<IModelBinder>();
            binder
                .Setup(b => b.BindModelAsync(It.IsAny<DefaultModelBindingContext>()))
                .Returns(ModelBindingResult.FailedAsync(string.Empty));

            var controllerContext = GetControllerContext(actionDescriptor);
            controllerContext.ModelBinders.Add(binder.Object);
            controllerContext.ValueProviders.Add(new SimpleValueProvider());

            var argumentBinder = GetArgumentBinder();
            var modelMetadataProvider = TestModelMetadataProvider.CreateDefaultProvider();

            // Act
            var result = await argumentBinder
                .BindActionArgumentsAsync(controllerContext, new TestController());

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task BindActionArgumentsAsync_AddsActionArguments_IfBinderReturnsNotNull()
        {
            // Arrange
            Func<object, int> method = foo => 1;
            var actionDescriptor = GetActionDescriptor();
            actionDescriptor.Parameters.Add(
                new ParameterDescriptor
                {
                    Name = "foo",
                    ParameterType = typeof(string),
                    BindingInfo = new BindingInfo(),
                });

            var value = "Hello world";
            var metadataProvider = new EmptyModelMetadataProvider();

            var binder = new Mock<IModelBinder>();
            binder
                .Setup(b => b.BindModelAsync(It.IsAny<DefaultModelBindingContext>()))
                .Callback((ModelBindingContext context) =>
                {
                    context.ModelMetadata = metadataProvider.GetMetadataForType(typeof(string));
                    context.Result = ModelBindingResult.Success(string.Empty, value);
                })
                .Returns(TaskCache.CompletedTask);

            var controllerContext = GetControllerContext(actionDescriptor);
            controllerContext.ModelBinders.Add(binder.Object);
            controllerContext.ValueProviders.Add(new SimpleValueProvider());

            var argumentBinder = GetArgumentBinder();

            // Act
            var result = await argumentBinder
                .BindActionArgumentsAsync(controllerContext, new TestController());

            // Assert
            Assert.Equal(1, result.Count);
            Assert.Equal(value, result["foo"]);
        }

        [Fact]
        public async Task BindActionArgumentsAsync_CallsValidator_IfModelBinderSucceeds()
        {
            // Arrange
            var actionDescriptor = GetActionDescriptor();
            actionDescriptor.Parameters.Add(
                new ParameterDescriptor
                {
                    Name = "foo",
                    ParameterType = typeof(object),
                });

            var controllerContext = GetControllerContext(actionDescriptor, "Hello");

            var mockValidator = new Mock<IObjectModelValidator>(MockBehavior.Strict);
            mockValidator
                .Setup(o => o.Validate(
                    It.IsAny<ActionContext>(),
                    It.IsAny<IModelValidatorProvider>(),
                    It.IsAny<ValidationStateDictionary>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()));

            var argumentBinder = GetArgumentBinder(mockValidator.Object);

            // Act
            var result = await argumentBinder.BindActionArgumentsAsync(controllerContext, new TestController());

            // Assert
            mockValidator
                .Verify(o => o.Validate(
                    It.IsAny<ActionContext>(),
                    It.IsAny<IModelValidatorProvider>(),
                    It.IsAny<ValidationStateDictionary>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()),
            Times.Once());
        }

        [Fact]
        public async Task BindActionArgumentsAsync_DoesNotCallValidator_IfModelBinderFails()
        {
            // Arrange
            Func<object, int> method = foo => 1;
            var actionDescriptor = GetActionDescriptor();
            actionDescriptor.Parameters.Add(
                new ParameterDescriptor
                {
                    Name = "foo",
                    ParameterType = typeof(object),
                    BindingInfo = new BindingInfo(),
                });

            var binder = new Mock<IModelBinder>();
            binder
                .Setup(b => b.BindModelAsync(It.IsAny<DefaultModelBindingContext>()))
                .Returns(TaskCache.CompletedTask);

            var controllerContext = GetControllerContext(actionDescriptor);
            controllerContext.ModelBinders.Add(binder.Object);
            controllerContext.ValueProviders.Add(new SimpleValueProvider());

            var mockValidator = new Mock<IObjectModelValidator>(MockBehavior.Strict);
            mockValidator
                .Setup(o => o.Validate(
                    It.IsAny<ActionContext>(),
                    It.IsAny<IModelValidatorProvider>(),
                    It.IsAny<ValidationStateDictionary>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()));

            var argumentBinder = GetArgumentBinder(mockValidator.Object);

            // Act
            var result = await argumentBinder.BindActionArgumentsAsync(controllerContext, new TestController());

            // Assert
            mockValidator
                .Verify(o => o.Validate(
                    It.IsAny<ActionContext>(),
                    It.IsAny<IModelValidatorProvider>(),
                    It.IsAny<ValidationStateDictionary>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()),
                Times.Never());
        }

        [Fact]
        public async Task BindActionArgumentsAsync_CallsValidator_ForControllerProperties_IfModelBinderSucceeds()
        {
            // Arrange
            var actionDescriptor = GetActionDescriptor();
            actionDescriptor.BoundProperties.Add(
                new ParameterDescriptor
                {
                    Name = nameof(TestController.StringProperty),
                    ParameterType = typeof(string),
                });

            var controllerContext = GetControllerContext(actionDescriptor, "Hello");

            var mockValidator = new Mock<IObjectModelValidator>(MockBehavior.Strict);
            mockValidator
                .Setup(o => o.Validate(
                    It.IsAny<ActionContext>(),
                    It.IsAny<IModelValidatorProvider>(),
                    It.IsAny<ValidationStateDictionary>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()));

            var argumentBinder = GetArgumentBinder(mockValidator.Object);

            // Act
            var result = await argumentBinder.BindActionArgumentsAsync(controllerContext, new TestController());

            // Assert
            mockValidator
                .Verify(o => o.Validate(
                    It.IsAny<ActionContext>(),
                    It.IsAny<IModelValidatorProvider>(),
                    It.IsAny<ValidationStateDictionary>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()),
                Times.Once());
        }

        [Fact]
        public async Task BindActionArgumentsAsync_DoesNotCallValidator_ForControllerProperties_IfModelBinderFails()
        {
            // Arrange
            Func<object, int> method = foo => 1;
            var actionDescriptor = GetActionDescriptor();
            actionDescriptor.BoundProperties.Add(
                new ParameterDescriptor
                {
                    Name = nameof(TestController.StringProperty),
                    ParameterType = typeof(string),
                });

            var binder = new Mock<IModelBinder>();
            binder
                .Setup(b => b.BindModelAsync(It.IsAny<DefaultModelBindingContext>()))
                .Returns(TaskCache.CompletedTask);

            var controllerContext = GetControllerContext(actionDescriptor);
            controllerContext.ModelBinders.Add(binder.Object);
            controllerContext.ValueProviders.Add(new SimpleValueProvider());

            var mockValidator = new Mock<IObjectModelValidator>(MockBehavior.Strict);
            mockValidator
                .Setup(o => o.Validate(
                    It.IsAny<ActionContext>(),
                    It.IsAny<IModelValidatorProvider>(),
                    It.IsAny<ValidationStateDictionary>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()));

            var argumentBinder = GetArgumentBinder(mockValidator.Object);

            // Act
            var result = await argumentBinder.BindActionArgumentsAsync(controllerContext, new TestController());

            // Assert
            mockValidator
                .Verify(o => o.Validate(
                    It.IsAny<ActionContext>(),
                    It.IsAny<IModelValidatorProvider>(),
                    It.IsAny<ValidationStateDictionary>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()),
                Times.Never());
        }

        [Fact]
        public async Task BindActionArgumentsAsync_SetsControllerProperties_ForReferenceTypes()
        {
            // Arrange
            var actionDescriptor = GetActionDescriptor();
            actionDescriptor.BoundProperties.Add(
                new ParameterDescriptor
                {
                    Name = nameof(TestController.StringProperty),
                    BindingInfo = new BindingInfo(),
                    ParameterType = typeof(string)
                });

            var controllerContext = GetControllerContext(actionDescriptor, "Hello");
            var argumentBinder = GetArgumentBinder();
            var controller = new TestController();

            // Act
            var result = await argumentBinder.BindActionArgumentsAsync(controllerContext, controller);

            // Assert
            Assert.Equal("Hello", controller.StringProperty);
            Assert.Equal(new List<string> { "goodbye" }, controller.CollectionProperty);
            Assert.Null(controller.UntouchedProperty);
        }

        [Fact]
        public async Task BindActionArgumentsAsync_AddsToCollectionControllerProperties()
        {
            // Arrange
            var actionDescriptor = GetActionDescriptor();
            actionDescriptor.BoundProperties.Add(
                new ParameterDescriptor
                {
                    Name = nameof(TestController.CollectionProperty),
                    BindingInfo = new BindingInfo(),
                    ParameterType = typeof(ICollection<string>),
                });

            var expected = new List<string> { "Hello", "World", "!!" };
            var controllerContext = GetControllerContext(actionDescriptor, expected);

            var argumentBinder = GetArgumentBinder();
            var controller = new TestController();

            // Act
            var result = await argumentBinder.BindActionArgumentsAsync(controllerContext, controller);

            // Assert
            Assert.Equal(expected, controller.CollectionProperty);
            Assert.Null(controller.StringProperty);
            Assert.Null(controller.UntouchedProperty);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task BindActionArgumentsAsync_DoesNotSetNullValues_ForNonNullableProperties(bool isModelSet)
        {
            // Arrange
            var actionDescriptor = GetActionDescriptor();
            actionDescriptor.BoundProperties.Add(
                new ParameterDescriptor
                {
                    Name = nameof(TestController.NonNullableProperty),
                    BindingInfo = new BindingInfo() { BindingSource = BindingSource.Custom },
                    ParameterType = typeof(int)
                });

            var binder = new StubModelBinder(ModelBindingResult.Success(string.Empty, model: null));

            var controllerContext = GetControllerContext(actionDescriptor);
            controllerContext.ModelBinders.Add(binder);
            controllerContext.ValueProviders.Add(new SimpleValueProvider());

            var argumentBinder = GetArgumentBinder();
            var controller = new TestController();

            // Some non default value.
            controller.NonNullableProperty = -1;

            // Act
            var result = await argumentBinder.BindActionArgumentsAsync(controllerContext, controller);

            // Assert
            Assert.Equal(-1, controller.NonNullableProperty);
        }

        [Fact]
        public async Task BindActionArgumentsAsync_SetsNullValues_ForNullableProperties()
        {
            // Arrange
            var actionDescriptor = GetActionDescriptor();
            actionDescriptor.BoundProperties.Add(
                new ParameterDescriptor
                {
                    Name = "NullableProperty",
                    BindingInfo = new BindingInfo() { BindingSource = BindingSource.Custom },
                    ParameterType = typeof(int?)
                });

            var binder = new StubModelBinder(ModelBindingResult.Success(key: string.Empty, model: null));

            var controllerContext = GetControllerContext(actionDescriptor);
            controllerContext.ModelBinders.Add(binder);
            controllerContext.ValueProviders.Add(new SimpleValueProvider());

            var argumentBinder = GetArgumentBinder();
            var controller = new TestController();

            // Some non default value.
            controller.NullableProperty = -1;

            // Act
            var result = await argumentBinder.BindActionArgumentsAsync(controllerContext, controller);

            // Assert
            Assert.Null(controller.NullableProperty);
        }

        // property name, property type, property accessor, input value, expected value
        public static TheoryData<string, Type, Func<object, object>, object, object> SkippedPropertyData
        {
            get
            {
                return new TheoryData<string, Type, Func<object, object>, object, object>
                {
                    {
                        nameof(TestController.ArrayProperty),
                        typeof(string[]),
                        controller => ((TestController)controller).ArrayProperty,
                        new string[] { "hello", "world" },
                        new string[] { "goodbye" }
                    },
                    {
                        nameof(TestController.CollectionProperty),
                        typeof(ICollection<string>),
                        controller => ((TestController)controller).CollectionProperty,
                        null,
                        new List<string> { "goodbye" }
                    },
                    {
                        nameof(TestController.NonCollectionProperty),
                        typeof(Person),
                        controller => ((TestController)controller).NonCollectionProperty,
                        new Person { Name = "Fred" },
                        new Person { Name = "Ginger" }
                    },
                    {
                        nameof(TestController.NullCollectionProperty),
                        typeof(ICollection<string>),
                        controller => ((TestController)controller).NullCollectionProperty,
                        new List<string> { "hello", "world" },
                        null
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(SkippedPropertyData))]
        public async Task BindActionArgumentsAsync_SkipsReadOnlyControllerProperties(
            string propertyName,
            Type propertyType,
            Func<object, object> propertyAccessor,
            object inputValue,
            object expectedValue)
        {
            // Arrange
            var actionDescriptor = GetActionDescriptor();
            actionDescriptor.BoundProperties.Add(
                new ParameterDescriptor
                {
                    Name = propertyName,
                    BindingInfo = new BindingInfo(),
                    ParameterType = propertyType,
                });

            var controllerContext = GetControllerContext(actionDescriptor, inputValue);
            var argumentBinder = GetArgumentBinder();
            var controller = new TestController();

            // Act
            var result = await argumentBinder.BindActionArgumentsAsync(controllerContext, controller);

            // Assert
            Assert.Equal(expectedValue, propertyAccessor(controller));
            Assert.Null(controller.StringProperty);
            Assert.Null(controller.UntouchedProperty);
        }

        [Fact]
        public async Task BindActionArgumentsAsync_SetsMultipleControllerProperties()
        {
            // Arrange
            var boundPropertyTypes = new Dictionary<string, Type>
            {
                { nameof(TestController.ArrayProperty), typeof(string[]) },                // Skipped
                { nameof(TestController.CollectionProperty), typeof(List<string>) },
                { nameof(TestController.NonCollectionProperty), typeof(Person) },          // Skipped
                { nameof(TestController.NullCollectionProperty), typeof(List<string>) },   // Skipped
                { nameof(TestController.StringProperty), typeof(string) },
            };
            var inputPropertyValues = new Dictionary<string, object>
            {
                { nameof(TestController.ArrayProperty), new string[] { "hello", "world" } },
                { nameof(TestController.CollectionProperty), new List<string> { "hello", "world" } },
                { nameof(TestController.NonCollectionProperty), new Person { Name = "Fred" } },
                { nameof(TestController.NullCollectionProperty), new List<string> { "hello", "world" } },
                { nameof(TestController.StringProperty), "Hello" },
            };
            var expectedPropertyValues = new Dictionary<string, object>
            {
                { nameof(TestController.ArrayProperty), new string[] { "goodbye" } },
                { nameof(TestController.CollectionProperty), new List<string> { "hello", "world" } },
                { nameof(TestController.NonCollectionProperty), new Person { Name = "Ginger" } },
                { nameof(TestController.NullCollectionProperty), null },
                { nameof(TestController.StringProperty), "Hello" },
            };

            var actionDescriptor = GetActionDescriptor();
            foreach (var keyValuePair in boundPropertyTypes)
            {
                actionDescriptor.BoundProperties.Add(
                    new ParameterDescriptor
                    {
                        Name = keyValuePair.Key,
                        BindingInfo = new BindingInfo(),
                        ParameterType = keyValuePair.Value,
                    });
            }

            var controllerContext = GetControllerContext(actionDescriptor);
            var argumentBinder = GetArgumentBinder();
            var controller = new TestController();

            var binder = new StubModelBinder(bindingContext =>
            {
                // BindingContext.ModelName will be string.Empty here. This is a 'fallback to empty prefix'
                // because the value providers have no data.
                object model;
                if (inputPropertyValues.TryGetValue(bindingContext.FieldName, out model))
                {
                    bindingContext.Result = ModelBindingResult.Success(bindingContext.ModelName, model);
                }
                else
                {
                    bindingContext.Result = ModelBindingResult.Failed(bindingContext.ModelName);
                }
            });
            controllerContext.ModelBinders.Add(binder.Object);
            controllerContext.ValueProviders.Add(new SimpleValueProvider());

            // Act
            var result = await argumentBinder.BindActionArgumentsAsync(controllerContext, controller);

            // Assert
            Assert.Equal(new string[] { "goodbye" }, controller.ArrayProperty);                 // Skipped
            Assert.Equal(new List<string> { "hello", "world" }, controller.CollectionProperty);
            Assert.Equal(new Person { Name = "Ginger" }, controller.NonCollectionProperty);     // Skipped
            Assert.Null(controller.NullCollectionProperty);                                     // Skipped
            Assert.Null(controller.UntouchedProperty);                                          // Not bound
            Assert.Equal("Hello", controller.StringProperty);
        }

        private static ControllerContext GetControllerContext(ControllerActionDescriptor descriptor = null)
        {
            var context = new ControllerContext()
            {
                ActionDescriptor = descriptor ?? GetActionDescriptor(),
                HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData(),
            };

            return context;
        }

        private static ControllerActionDescriptor GetActionDescriptor()
        {
            Func<object, int> method = foo => 1;
            return new ControllerActionDescriptor
            {
                MethodInfo = method.GetMethodInfo(),
                ControllerTypeInfo = typeof(TestController).GetTypeInfo(),
                BoundProperties = new List<ParameterDescriptor>(),
                Parameters = new List<ParameterDescriptor>()
            };
        }

        private static ControllerContext GetControllerContext(ControllerActionDescriptor descriptor = null, object model = null)
        {
            var context = new ControllerContext()
            {
                ActionDescriptor = descriptor ?? GetActionDescriptor(),
                HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData(),
            };

            var binder = new Mock<IModelBinder>();
            binder.Setup(b => b.BindModelAsync(It.IsAny<DefaultModelBindingContext>()))
                  .Returns<DefaultModelBindingContext>(mbc =>
                  {
                      mbc.Result = ModelBindingResult.Success(string.Empty, model);
                      return TaskCache.CompletedTask;
                  });

            context.ModelBinders.Add(binder.Object);
            context.ValueProviders.Add(new SimpleValueProvider());
            return context;
        }

        private static ControllerArgumentBinder GetArgumentBinder(IObjectModelValidator validator = null)
        {
            if (validator == null)
            {
                validator = CreateMockValidator();
            }

            return new ControllerArgumentBinder(
                TestModelMetadataProvider.CreateDefaultProvider(),
                validator);
        }

        private static IObjectModelValidator CreateMockValidator()
        {
            var mockValidator = new Mock<IObjectModelValidator>(MockBehavior.Strict);
            mockValidator
                .Setup(o => o.Validate(
                    It.IsAny<ActionContext>(),
                    It.IsAny<IModelValidatorProvider>(),
                    It.IsAny<ValidationStateDictionary>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()));
            return mockValidator.Object;
        }

        // No need for bind-related attributes on properties in this controller class. Properties are added directly
        // to the BoundProperties collection, bypassing usual requirements.
        private class TestController
        {
            public string UntouchedProperty { get; set; }

            public string[] ArrayProperty { get; } = new string[] { "goodbye" };

            public ICollection<string> CollectionProperty { get; } = new List<string> { "goodbye" };

            public Person NonCollectionProperty { get; } = new Person { Name = "Ginger" };

            public ICollection<string> NullCollectionProperty { get; private set; }

            public string StringProperty { get; set; }

            public int NonNullableProperty { get; set; }

            public int? NullableProperty { get; set; }
        }

        private class Person : IEquatable<Person>, IEquatable<object>
        {
            public string Name { get; set; }

            public bool Equals(Person other)
            {
                return other != null && string.Equals(Name, other.Name, StringComparison.Ordinal);
            }

            bool IEquatable<object>.Equals(object obj)
            {
                return Equals(obj as Person);
            }
        }

        private class CustomBindingSourceAttribute : Attribute, IBindingSourceMetadata
        {
            public BindingSource BindingSource { get { return BindingSource.Custom; } }
        }

        private class ValueProviderMetadataAttribute : Attribute, IBindingSourceMetadata
        {
            public BindingSource BindingSource { get { return BindingSource.Query; } }
        }
    }
}
