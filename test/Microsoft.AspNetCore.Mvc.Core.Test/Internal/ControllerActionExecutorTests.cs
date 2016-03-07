// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Testing;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class ControllerActionExecutorTests
    {
        private TestController _controller = new TestController();

        private delegate void MethodWithVoidReturnType();

        private delegate string SyncMethod(string s);

        private delegate Task MethodWithTaskReturnType(int i, string s);

        private delegate Task<int> MethodWithTaskOfIntReturnType(int i, string s);

        private delegate Task<Task<int>> MethodWithTaskOfTaskOfIntReturnType(int i, string s);

        public delegate TestController.TaskDerivedType MethodWithCustomTaskReturnType(int i, string s);

        private delegate TestController.TaskOfTDerivedType<int> MethodWithCustomTaskOfTReturnType(int i, string s);

        private delegate dynamic ReturnTaskAsDynamicValue(int i, string s);

        [Theory,
         InlineData(true),
         InlineData(false)]
        public async Task AsyncAction_WithVoidReturnType(bool usingExecutor)
        {
            // Arrange
            var methodWithVoidReturnType = new MethodWithVoidReturnType(TestController.VoidAction);
            var result = await ExecuteAction(usingExecutor, methodWithVoidReturnType, null, (IDictionary<string,object>)null);
            // Assert
            Assert.Same(null, result);
        }

        [Theory,
         InlineData(true),
         InlineData(false)]
        public async Task AsyncAction_TaskReturnType(bool usingExecutor)
        {
            // Arrange
            var inputParam1 = 1;
            var inputParam2 = "Second Parameter";
            var actionParameters = new Dictionary<string, object> { { "i", inputParam1 }, { "s", inputParam2 } };

            var methodWithTaskReturnType = new MethodWithTaskReturnType(_controller.TaskAction);
            var result = await ExecuteAction(usingExecutor, methodWithTaskReturnType, _controller, actionParameters);

            // Assert
            Assert.Same(null, result);
        }

        [Theory,
         InlineData(true),
         InlineData(false)]
        public async Task AsyncAction_TaskOfValueReturnType(bool usingExecutor)
        {
            // Arrange
            var inputParam1 = 1;
            var inputParam2 = "Second Parameter";
            var actionParameters = new Dictionary<string, object> { { "i", inputParam1 }, { "s", inputParam2 } };

            var methodWithTaskOfIntReturnType = new MethodWithTaskOfIntReturnType(_controller.TaskValueTypeAction);

            // Act
            var result = await ExecuteAction(usingExecutor, methodWithTaskOfIntReturnType, _controller, actionParameters);
            // Assert
            Assert.Equal(inputParam1, result);
        }

        [Theory,
         InlineData(true),
         InlineData(false)]
        public async Task AsyncAction_TaskOfTaskOfValueReturnType(bool usingExecutor)
        {
            // Arrange
            var inputParam1 = 1;
            var inputParam2 = "Second Parameter";
            var actionParameters = new Dictionary<string, object> { { "i", inputParam1 }, { "s", inputParam2 } };

            var methodWithTaskOfTaskOfIntReturnType = new MethodWithTaskOfTaskOfIntReturnType(_controller.TaskOfTaskAction);

            // Act
            var result = await (Task<int>)( await ExecuteAction(usingExecutor, methodWithTaskOfTaskOfIntReturnType, _controller, actionParameters));            

            // Assert
            Assert.Equal(inputParam1, result);
        }

        [Theory,
         InlineData(true),
         InlineData(false)]
        public async Task AsyncAction_WithAsyncKeywordThrows(bool usingExecutor)
        {
            // Arrange
            var inputParam1 = 1;
            var inputParam2 = "Second Parameter";
            var actionParameters = new Dictionary<string, object> { { "i", inputParam1 }, { "s", inputParam2 } };

            var methodWithTaskOfIntReturnType = new MethodWithTaskOfIntReturnType(_controller.TaskActionWithException);

            // Act and Assert
            await Assert.ThrowsAsync<NotImplementedException>(
                    () => ExecuteAction(usingExecutor,methodWithTaskOfIntReturnType,
                                                               _controller,
                                                               actionParameters));
        }

        [Theory,
         InlineData(true),
         InlineData(false)]
        public async Task AsyncAction_WithoutAsyncThrows(bool usingExecutor)
        {
            // Arrange
            var inputParam1 = 1;
            var inputParam2 = "Second Parameter";
            var actionParameters = new Dictionary<string, object> { { "i", inputParam1 }, { "s", inputParam2 } };

            var methodWithTaskOfIntReturnType = new MethodWithTaskOfIntReturnType(_controller.TaskActionWithExceptionWithoutAsync);

            // Act & Assert            
            await Assert.ThrowsAsync<NotImplementedException>(
                        () => ExecuteAction(usingExecutor,methodWithTaskOfIntReturnType,
                                                                   _controller,
                                                                   actionParameters));
        }

        [Theory,
         InlineData(true),
         InlineData(false)]
        public async Task AsyncAction_WithExceptionsAfterAwait(bool usingExecutor)
        {
            // Arrange
            var inputParam1 = 1;
            var inputParam2 = "Second Parameter";
            var actionParameters = new Dictionary<string, object> { { "i", inputParam1 }, { "s", inputParam2 } };

            var methodWithTaskOfIntReturnType = new MethodWithTaskOfIntReturnType(_controller.TaskActionThrowAfterAwait);
            var expectedException = "Argument Exception";

            // Act & Assert            
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => ExecuteAction(
                    usingExecutor,
                    methodWithTaskOfIntReturnType,
                    _controller,
                    actionParameters));
            Assert.Equal(expectedException, ex.Message);
        }

        [Theory,
         InlineData(true),
         InlineData(false)]
        public async Task SyncAction(bool usingExecutor)
        {
            // Arrange
            var inputString = "hello";
            var syncMethod = new SyncMethod(_controller.Echo);

            // Act
            var result = await ExecuteAction(
                                                usingExecutor,
                                                syncMethod,
                                                _controller,
                                                new Dictionary<string, object>() { { "input", inputString } });
            // Assert
            Assert.Equal(inputString, result);
        }

        [Theory,
         InlineData(true),
         InlineData(false)]
        public async Task SyncAction_WithException(bool usingExecutor)
        {
            // Arrange
            var inputString = "hello";
            var syncMethod = new SyncMethod(_controller.EchoWithException);

            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(
                        () => ExecuteAction(
                                                usingExecutor,
                                                syncMethod,
                                                _controller,
                                                new Dictionary<string, object>() { { "input", inputString } }));
        }

        [Theory,
         InlineData(true),
         InlineData(false)]
        public async Task ExecuteAsync_WithArgumentDictionary_DefaultValueAttributeUsed(bool usingExecutor)
        {
            // Arrange
            var syncMethod = new SyncMethod(_controller.EchoWithDefaultValue);

            // Act
            var result = await ExecuteAction(
                usingExecutor,
                syncMethod,
                _controller,
                new Dictionary<string, object>());

            // Assert
            Assert.Equal("hello", result);
        }

        [Theory,
         InlineData(true),
         InlineData(false)]
        public async Task ExecuteAsync_WithArgumentArray_DefaultValueAttributeIgnored(bool usingExecutor)
        {
            // Arrange
            var syncMethod = new SyncMethod(_controller.EchoWithDefaultValue);

            // Act
            var result = await ExecuteAction(
                usingExecutor,
                syncMethod,
                _controller,
                new object[] { null, });
            

            // Assert
            Assert.Null(result);
        }

        [Theory,
         InlineData(true),
         InlineData(false)]
        public async Task ExecuteAsync_WithArgumentDictionary_DefaultParameterValueUsed(bool usingExecutor)
        {
            // Arrange
            var syncMethod = new SyncMethod(_controller.EchoWithDefaultValueAndAttribute);

            // Act
            var result = await ExecuteAction(
                usingExecutor,
                syncMethod,
                _controller,
                new Dictionary<string, object>());

            // Assert
            Assert.Equal("world", result);
        }

        [Theory,
         InlineData(true),
         InlineData(false)]
        public async Task ExecuteAsync_WithArgumentDictionary_AnyValue_HasPrecedenceOverDefaults(bool usingExecutor)
        {
            // Arrange
            var syncMethod = new SyncMethod(_controller.EchoWithDefaultValueAndAttribute);

            // Act
            var result = await ExecuteAction(
                usingExecutor,
                syncMethod,
                _controller,
                new Dictionary<string, object>() { { "input", null } });

            // Assert
            Assert.Null(result);
        }

        [Theory,
         InlineData(true),
         InlineData(false)]
        public async Task AsyncAction_WithCustomTaskReturnTypeThrows(bool usingExecutor)
        {
            // Arrange
            var inputParam1 = 1;
            var inputParam2 = "Second Parameter";
            var actionParameters = new Dictionary<string, object> { { "i", inputParam1 }, { "s", inputParam2 } };

            // If it is an unrecognized derived type we throw an InvalidOperationException.
            var methodWithCutomTaskReturnType = new MethodWithCustomTaskReturnType(_controller.TaskActionWithCustomTaskReturnType);

            var expectedException = string.Format(
                CultureInfo.CurrentCulture,
                "The method 'TaskActionWithCustomTaskReturnType' on type '{0}' returned a Task instance even though it is not an asynchronous method.",
                typeof(TestController));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => ExecuteAction(
                    usingExecutor,
                    methodWithCutomTaskReturnType,
                    _controller,
                    actionParameters));
            Assert.Equal(expectedException, ex.Message);
        }

        [Theory,
         InlineData(true),
         InlineData(false)]
        public async Task AsyncAction_WithCustomTaskOfTReturnTypeThrows(bool usingExecutor)
        {
            // Arrange
            var inputParam1 = 1;
            var inputParam2 = "Second Parameter";
            var actionParameters = new Dictionary<string, object> { { "i", inputParam1 }, { "s", inputParam2 } };

            var methodWithCutomTaskOfTReturnType = new MethodWithCustomTaskOfTReturnType(_controller.TaskActionWithCustomTaskOfTReturnType);
            var expectedException = string.Format(
                CultureInfo.CurrentCulture,
                "The method 'TaskActionWithCustomTaskOfTReturnType' on type '{0}' returned a Task instance even though it is not an asynchronous method.",
                typeof(TestController));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => ExecuteAction(
                    usingExecutor,
                    methodWithCutomTaskOfTReturnType,
                    _controller,
                    actionParameters));
            Assert.Equal(expectedException, ex.Message);
        }

        [Theory,
         InlineData(true),
         InlineData(false)]
        public async Task AsyncAction_ReturningUnwrappedTaskThrows(bool usingExecutor)
        {
            // Arrange
            var inputParam1 = 1;
            var inputParam2 = "Second Parameter";
            var actionParameters = new Dictionary<string, object> { { "i", inputParam1 }, { "s", inputParam2 } };

            var methodWithUnwrappedTask = new MethodWithTaskReturnType(_controller.UnwrappedTask);

            var expectedException = string.Format(
                CultureInfo.CurrentCulture,
                "The method 'UnwrappedTask' on type '{0}' returned an instance of '{1}'. " +
                "Make sure to call Unwrap on the returned value to avoid unobserved faulted Task.",
                typeof(TestController),
                typeof(Task<Task>).FullName);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => ExecuteAction(
                    usingExecutor,
                    methodWithUnwrappedTask,
                    _controller,
                    actionParameters));
            Assert.Equal(expectedException, ex.Message);
        }

        [Theory,
         InlineData(true),
         InlineData(false)]
        public async Task AsyncAction_WithDynamicReturnTypeThrows(bool usingExecutor)
        {
            // Arrange
            var inputParam1 = 1;
            var inputParam2 = "Second Parameter";
            var actionParameters = new Dictionary<string, object> { { "i", inputParam1 }, { "s", inputParam2 } };

            var dynamicTaskMethod = new ReturnTaskAsDynamicValue(_controller.ReturnTaskAsDynamicValue);
            var expectedException = string.Format(
                CultureInfo.CurrentCulture,
                "The method 'ReturnTaskAsDynamicValue' on type '{0}' returned a Task instance even though it is not an asynchronous method.",
                typeof(TestController));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
               () => ExecuteAction(
                    usingExecutor,
                    dynamicTaskMethod,
                    _controller,
                    actionParameters));
            Assert.Equal(expectedException, ex.Message);
        }

        [Theory,
         InlineData(true),
         InlineData(false)]
        public async Task ParametersInRandomOrder(bool usingExecutor)
        {
            // Arrange
            var inputParam1 = 1;
            var inputParam2 = "Second Parameter";

            // Note that the order of parameters is reversed
            var actionParameters = new Dictionary<string, object> { { "s", inputParam2 }, { "i", inputParam1 } };
            var methodWithTaskOfIntReturnType = new MethodWithTaskOfIntReturnType(_controller.TaskValueTypeAction);

            // Act
            var result = await             ExecuteAction(
                                                        usingExecutor,
                                                        methodWithTaskOfIntReturnType,
                                                        _controller,
                                                        actionParameters);

            // Assert
            Assert.Equal(inputParam1, result);
        }

        [Theory,
         InlineData(true),
         InlineData(false)]
        public async Task InvalidParameterValueThrows(bool usingExecutor)
        {
            // Arrange
            var inputParam2 = "Second Parameter";

            var actionParameters = new Dictionary<string, object> { { "i", "Some Invalid Value" }, { "s", inputParam2 } };
            var methodWithTaskOfIntReturnType = new MethodWithTaskOfIntReturnType(_controller.TaskValueTypeAction);
            var message = TestPlatformHelper.IsMono ? "Object type {0} cannot be converted to target type: {1}" :
                                                      "Object of type '{0}' cannot be converted to type '{1}'.";
            var expectedException = string.Format(
                CultureInfo.CurrentCulture,
                message,
                typeof(string),
                typeof(int));

            // Act & Assert
            // If it is an unrecognized derived type we throw an InvalidOperationException.
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => ExecuteAction(
                        usingExecutor,
                        methodWithTaskOfIntReturnType,
                        _controller,
                        actionParameters));

            Assert.Equal(expectedException, ex.Message);
        }

        private async Task<object> ExecuteAction(bool usingExecutor, Delegate methodDelegate, TestController controller, IDictionary<string, object> actionParameters)
        {
            ObjectMethodExecutor executor = null;
            if (usingExecutor)
            {
                executor = new ObjectMethodExecutor(methodDelegate.GetMethodInfo(), controller.GetType().GetTypeInfo());
            }

            object result;
            if (usingExecutor)
            {
                result = await ControllerActionExecutor.ExecuteAsync(
                                                        executor,
                                                        controller,
                                                        actionParameters);
            }
            else
            {
                result = await ControllerActionExecutor.ExecuteAsync(
                                                        methodDelegate.GetMethodInfo(),
                                                        controller,
                                                        actionParameters);
            }

            return result;
        }

        private async Task<object> ExecuteAction(bool usingExecutor, Delegate methodDelegate, TestController controller, object[] actionParameters)
        {
            ObjectMethodExecutor executor = null;
            if (usingExecutor)
            {
                executor = new ObjectMethodExecutor(methodDelegate.GetMethodInfo(), controller.GetType().GetTypeInfo());
            }

            object result;
            if (usingExecutor)
            {
                result = await ControllerActionExecutor.ExecuteAsync(
                                                        executor,
                                                        controller,
                                                        actionParameters);
            }
            else
            {
                result = await ControllerActionExecutor.ExecuteAsync(
                                                        methodDelegate.GetMethodInfo(),
                                                        controller,
                                                        actionParameters);
            }

            return result;
        }

        public class TestController
        {
            public static void VoidAction()
            {
            }

#pragma warning disable 1998
            public async Task TaskAction(int i, string s)
            {
                return;
            }
#pragma warning restore 1998

#pragma warning disable 1998
            public async Task<int> TaskValueTypeAction(int i, string s)
            {
                return i;
            }
#pragma warning restore 1998

#pragma warning disable 1998
            public async Task<Task<int>> TaskOfTaskAction(int i, string s)
            {
                return TaskValueTypeAction(i, s);
            }
#pragma warning restore 1998

            public Task<int> TaskValueTypeActionWithoutAsync(int i, string s)
            {
                return TaskValueTypeAction(i, s);
            }

#pragma warning disable 1998
            public async Task<int> TaskActionWithException(int i, string s)
            {
                throw new NotImplementedException("Not Implemented Exception");
            }
#pragma warning restore 1998

            public Task<int> TaskActionWithExceptionWithoutAsync(int i, string s)
            {
                throw new NotImplementedException("Not Implemented Exception");
            }

            public async Task<int> TaskActionThrowAfterAwait(int i, string s)
            {
                await Task.Delay(500);
                throw new ArgumentException("Argument Exception");
            }

            public TaskDerivedType TaskActionWithCustomTaskReturnType(int i, string s)
            {
                return new TaskDerivedType();
            }

            public TaskOfTDerivedType<int> TaskActionWithCustomTaskOfTReturnType(int i, string s)
            {
                return new TaskOfTDerivedType<int>(1);
            }

            /// <summary>
            /// Returns a <see cref="Task{TResult}"/> instead of a <see cref="Task"/>. This should throw an
            /// <see cref="InvalidOperationException"/>.
            /// </summary>
            public Task UnwrappedTask(int i, string s)
            {
                return Task.Factory.StartNew(async () => await Task.Delay(50));
            }

            public string Echo(string input)
            {
                return input;
            }

            public string EchoWithException(string input)
            {
                throw new NotImplementedException();
            }

            public string EchoWithDefaultValue([DefaultValue("hello")] string input)
            {
                return input;
            }

            public string EchoWithDefaultValueAndAttribute([DefaultValue("hello")] string input = "world")
            {
                return input;
            }

            public dynamic ReturnTaskAsDynamicValue(int i, string s)
            {
                return Task.Factory.StartNew(() => i);
            }

            public class TaskDerivedType : Task
            {
                public TaskDerivedType()
                    : base(() => Console.WriteLine("In The Constructor"))
                {
                }
            }

            public class TaskOfTDerivedType<T> : Task<T>
            {
                public TaskOfTDerivedType(T input)
                    : base(() => input)
                {
                }
            }
        }
    }
}