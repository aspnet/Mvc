﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Internal;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Infrastructure
{
    public class ActionMethodExecutorTest
    {
        [Fact]
        public void ActionMethodExecutor_ExecutesVoidActions()
        {
            // Arrange
            var mapper = new ActionResultTypeMapper();
            var controller = new TestController();
            var objectMethodExecutor = GetExecutor(nameof(TestController.VoidAction));
            var actionMethodExecutor = ActionMethodExecutor.GetExecutor(objectMethodExecutor);

            // Act
            var valueTask = actionMethodExecutor.Execute(mapper, objectMethodExecutor, controller, Array.Empty<object>());

            // Assert
            Assert.True(controller.Executed);
            Assert.IsType<EmptyResult>(valueTask.Result);
        }

        [Fact]
        public void ActionMethodExecutor_ExecutesActionsReturningIActionResult()
        {
            // Arrange
            var mapper = new ActionResultTypeMapper();
            var controller = new TestController();
            var objectMethodExecutor = GetExecutor(nameof(TestController.ReturnIActionResult));
            var actionMethodExecutor = ActionMethodExecutor.GetExecutor(objectMethodExecutor);

            // Act
            var valueTask = actionMethodExecutor.Execute(mapper, objectMethodExecutor, controller, Array.Empty<object>());

            // Assert
            Assert.True(valueTask.IsCompleted);
            Assert.IsType<ContentResult>(valueTask.Result);
        }

        [Fact]
        public void ActionMethodExecutor_ExecutesActionsReturningSubTypeOfActionResult()
        {
            // Arrange
            var mapper = new ActionResultTypeMapper();
            var controller = new TestController();
            var objectMethodExecutor = GetExecutor(nameof(TestController.ReturnsIActionResultSubType));
            var actionMethodExecutor = ActionMethodExecutor.GetExecutor(objectMethodExecutor);

            // Act
            var valueTask = actionMethodExecutor.Execute(mapper, objectMethodExecutor, controller, Array.Empty<object>());

            // Assert
            Assert.IsType<ContentResult>(valueTask.Result);
        }

        [Fact]
        public void ActionMethodExecutor_ExecutesActionsReturningActionResultOfT()
        {
            // Arrange
            var mapper = new ActionResultTypeMapper();
            var controller = new TestController();
            var objectMethodExecutor = GetExecutor(nameof(TestController.ReturnsActionResultOfT));
            var actionMethodExecutor = ActionMethodExecutor.GetExecutor(objectMethodExecutor);

            // Act
            var valueTask = actionMethodExecutor.Execute(mapper, objectMethodExecutor, controller, Array.Empty<object>());

            // Assert
            var result = Assert.IsType<ObjectResult>(valueTask.Result);
            Assert.NotNull(result.Value);
            Assert.IsType<TestModel>(result.Value);
            Assert.Equal(typeof(TestModel), result.DeclaredType);
        }

        [Fact]
        public void ActionMethodExecutor_ExecutesActionsReturningModelAsModel()
        {
            // Arrange
            var mapper = new ActionResultTypeMapper();
            var controller = new TestController();
            var objectMethodExecutor = GetExecutor(nameof(TestController.ReturnsModelAsModel));
            var actionMethodExecutor = ActionMethodExecutor.GetExecutor(objectMethodExecutor);

            // Act
            var valueTask = actionMethodExecutor.Execute(mapper, objectMethodExecutor, controller, Array.Empty<object>());

            // Assert
            var result = Assert.IsType<ObjectResult>(valueTask.Result);
            Assert.NotNull(result.Value);
            Assert.IsType<TestModel>(result.Value);
            Assert.Equal(typeof(TestModel), result.DeclaredType);
        }

        [Fact]
        public void ActionMethodExecutor_ExecutesActionsReturningModelAsObject()
        {
            // Arrange
            var mapper = new ActionResultTypeMapper();
            var controller = new TestController();
            var objectMethodExecutor = GetExecutor(nameof(TestController.ReturnModelAsObject));
            var actionMethodExecutor = ActionMethodExecutor.GetExecutor(objectMethodExecutor);

            // Act
            var valueTask = actionMethodExecutor.Execute(mapper, objectMethodExecutor, controller, Array.Empty<object>());

            // Assert
            var result = Assert.IsType<ObjectResult>(valueTask.Result);
            Assert.NotNull(result.Value);
            Assert.IsType<TestModel>(result.Value);
            Assert.Equal(typeof(object), result.DeclaredType);
        }

        [Fact]
        public void ActionMethodExecutor_ExecutesActionsReturningActionResultAsObject()
        {
            // Arrange
            var mapper = new ActionResultTypeMapper();
            var controller = new TestController();
            var objectMethodExecutor = GetExecutor(nameof(TestController.ReturnsIActionResultSubType));
            var actionMethodExecutor = ActionMethodExecutor.GetExecutor(objectMethodExecutor);

            // Act
            var valueTask = actionMethodExecutor.Execute(mapper, objectMethodExecutor, controller, Array.Empty<object>());

            // Assert
            Assert.IsType<ContentResult>(valueTask.Result);
        }

        [Fact]
        public void ActionMethodExecutor_ExecutesActionsReturnTask()
        {
            // Arrange
            var mapper = new ActionResultTypeMapper();
            var controller = new TestController();
            var objectMethodExecutor = GetExecutor(nameof(TestController.ReturnsTask));
            var actionMethodExecutor = ActionMethodExecutor.GetExecutor(objectMethodExecutor);

            // Act
            var valueTask = actionMethodExecutor.Execute(mapper, objectMethodExecutor, controller, Array.Empty<object>());

            // Assert
            Assert.True(controller.Executed);
            Assert.IsType<EmptyResult>(valueTask.Result);
        }

        [Fact]
        public void ActionMethodExecutorExecutesActionsAsynchronouslyReturningIActionResult()
        {
            // Arrange
            var mapper = new ActionResultTypeMapper();
            var controller = new TestController();
            var objectMethodExecutor = GetExecutor(nameof(TestController.ReturnIActionResultAsync));
            var actionMethodExecutor = ActionMethodExecutor.GetExecutor(objectMethodExecutor);

            // Act
            var valueTask = actionMethodExecutor.Execute(mapper, objectMethodExecutor, controller, Array.Empty<object>());

            // Assert
            Assert.IsType<StatusCodeResult>(valueTask.Result);
        }

        [Fact]
        public async Task ActionMethodExecutor_ExecutesActionsAsynchronouslyReturningActionResultSubType()
        {
            // Arrange
            var mapper = new ActionResultTypeMapper();
            var controller = new TestController();
            var objectMethodExecutor = GetExecutor(nameof(TestController.ReturnIActionResultAsync));
            var actionMethodExecutor = ActionMethodExecutor.GetExecutor(objectMethodExecutor);

            // Act
            var valueTask = actionMethodExecutor.Execute(mapper, objectMethodExecutor, controller, Array.Empty<object>());

            // Assert
            await valueTask;
            Assert.IsType<StatusCodeResult>(valueTask.Result);
        }

        [Fact]
        public void ActionMethodExecutor_ExecutesActionsAsynchronouslyReturningModel()
        {
            // Arrange
            var mapper = new ActionResultTypeMapper();
            var controller = new TestController();
            var objectMethodExecutor = GetExecutor(nameof(TestController.ReturnsModelAsModelAsync));
            var actionMethodExecutor = ActionMethodExecutor.GetExecutor(objectMethodExecutor);

            // Act
            var valueTask = actionMethodExecutor.Execute(mapper, objectMethodExecutor, controller, Array.Empty<object>());

            // Assert
            var result = Assert.IsType<ObjectResult>(valueTask.Result);
            Assert.NotNull(result.Value);
            Assert.IsType<TestModel>(result.Value);
            Assert.Equal(typeof(TestModel), result.DeclaredType);
        }

        [Fact]
        public void ActionMethodExecutor_ExecutesActionsAsynchronouslyReturningModelAsObject()
        {
            // Arrange
            var mapper = new ActionResultTypeMapper();
            var controller = new TestController();
            var objectMethodExecutor = GetExecutor(nameof(TestController.ReturnsModelAsObjectAsync));
            var actionMethodExecutor = ActionMethodExecutor.GetExecutor(objectMethodExecutor);

            // Act
            var valueTask = actionMethodExecutor.Execute(mapper, objectMethodExecutor, controller, Array.Empty<object>());

            // Assert
            var result = Assert.IsType<ObjectResult>(valueTask.Result);
            Assert.NotNull(result.Value);
            Assert.IsType<TestModel>(result.Value);
            Assert.Equal(typeof(object), result.DeclaredType);
        }

        [Fact]
        public void ActionMethodExecutor_ExecutesActionsAsynchronouslyReturningIActionResultAsObject()
        {
            // Arrange
            var mapper = new ActionResultTypeMapper();
            var controller = new TestController();
            var objectMethodExecutor = GetExecutor(nameof(TestController.ReturnIActionResultAsObjectAsync));
            var actionMethodExecutor = ActionMethodExecutor.GetExecutor(objectMethodExecutor);

            // Act
            var valueTask = actionMethodExecutor.Execute(mapper, objectMethodExecutor, controller, Array.Empty<object>());

            // Assert
            Assert.IsType<OkResult>(valueTask.Result);
        }

        [Fact]
        public void ActionMethodExecutor_ExecutesActionsAsynchronouslyReturningActionResultOfT()
        {
            // Arrange
            var mapper = new ActionResultTypeMapper();
            var controller = new TestController();
            var objectMethodExecutor = GetExecutor(nameof(TestController.ReturnActionResultOFTAsync));
            var actionMethodExecutor = ActionMethodExecutor.GetExecutor(objectMethodExecutor);

            // Act
            var valueTask = actionMethodExecutor.Execute(mapper, objectMethodExecutor, controller, Array.Empty<object>());

            // Assert
            var result = Assert.IsType<ObjectResult>(valueTask.Result);
            Assert.NotNull(result.Value);
            Assert.IsType<TestModel>(result.Value);
            Assert.Equal(typeof(TestModel), result.DeclaredType);
        }

        [Fact]
        public void ActionMethodExecutor_ThrowsIfIConvertFromIActionResult_ReturnsNull()
        {
            // Arrange
            var mapper = new ActionResultTypeMapper();
            var controller = new TestController();
            var objectMethodExecutor = GetExecutor(nameof(TestController.ReturnsCustomConvertibleFromIActionResult));
            var actionMethodExecutor = ActionMethodExecutor.GetExecutor(objectMethodExecutor);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(
                () => actionMethodExecutor.Execute(mapper, objectMethodExecutor, controller, Array.Empty<object>()));

            Assert.Equal($"Cannot return null from an action method with a return type of '{typeof(CustomConvertibleFromAction)}'.", ex.Message);
        }

        private static ObjectMethodExecutor GetExecutor(string methodName)
        {
            var type = typeof(TestController);
            var methodInfo = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(methodInfo);
            return ObjectMethodExecutor.Create(methodInfo, type.GetTypeInfo());
        }

        private class TestController
        {
            public bool Executed { get; set; }

            public void VoidAction() => Executed = true;

            public IActionResult ReturnIActionResult() => new ContentResult();

            public ContentResult ReturnsIActionResultSubType() => new ContentResult();

            public ActionResult<TestModel> ReturnsActionResultOfT() => new ActionResult<TestModel>(new TestModel());

            public CustomConvertibleFromAction ReturnsCustomConvertibleFromIActionResult() => new CustomConvertibleFromAction();

            public TestModel ReturnsModelAsModel() => new TestModel();

            public object ReturnModelAsObject() => new TestModel();

            public object ReturnIActionResultAsObject() => new RedirectResult("/foo");

            public Task ReturnsTask()
            {
                Executed = true;
                return Task.CompletedTask;
            }

            public Task<IActionResult> ReturnIActionResultAsync() => Task.FromResult((IActionResult)new StatusCodeResult(201));

            public Task<StatusCodeResult> ReturnsIActionResultSubTypeAsync() => Task.FromResult(new StatusCodeResult(200));

            public Task<TestModel> ReturnsModelAsModelAsync() => Task.FromResult(new TestModel());

            public Task<object> ReturnsModelAsObjectAsync() => Task.FromResult((object)new TestModel());

            public Task<object> ReturnIActionResultAsObjectAsync() => Task.FromResult((object)new OkResult());

            public Task<ActionResult<TestModel>> ReturnActionResultOFTAsync() => Task.FromResult(new ActionResult<TestModel>(new TestModel()));
        }

        private class TestModel
        {
        }

        private class CustomConvertibleFromAction : IConvertToActionResult
        {
            public IActionResult Convert() => null;
        }
    }
}
