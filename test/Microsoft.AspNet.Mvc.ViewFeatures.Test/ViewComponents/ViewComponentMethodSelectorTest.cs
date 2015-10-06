// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNet.Mvc.ViewComponents
{
    public class ViewComponentMethodSelectorTest
    {
        [Theory]
        [InlineData(typeof(ViewComponentWithSyncInvoke), new object[0])]
        [InlineData(typeof(ViewComponentWithSyncInvoke), new object[] { "" })]
        [InlineData(typeof(ViewComponentWithSyncInvoke), new object[] { 42 })]
        [InlineData(typeof(ViewComponentWithAsyncInvoke), new object[] { 42, false })]
        [InlineData(typeof(ViewComponentWithNonPublicNonInstanceInvokes), new object[] { })]
        [InlineData(typeof(ViewComponentWithNonPublicNonInstanceInvokes), new object[] { "" })]
        public void FindAsyncMethod_ReturnsNull_IfMatchCannotBeFound(Type type, object[] args)
        {
            // Arrange
            var typeInfo = type.GetTypeInfo();

            // Act
            var method = ViewComponentMethodSelector.FindAsyncMethod(typeInfo, args);

            // Assert
            Assert.Null(method);
        }

        [Theory]
        [InlineData(typeof(ViewComponentWithAsyncInvoke), new object[0])]
        [InlineData(typeof(ViewComponentWithSyncInvoke), new object[] { "" })]
        [InlineData(typeof(ViewComponentWithAsyncInvoke), new object[] { "" })]
        [InlineData(typeof(ViewComponentWithSyncInvoke), new object[] { 42 })]
        [InlineData(typeof(ViewComponentWithAsyncInvoke), new object[] { "", 42 })]
        [InlineData(typeof(ViewComponentWithNonPublicNonInstanceInvokes), new object[] { })]
        [InlineData(typeof(ViewComponentWithNonPublicNonInstanceInvokes), new object[] { "" })]
        [InlineData(typeof(BaseClass), new object[] { })]
        public void FindSyncMethod_ReturnsNull_IfMatchCannotBeFound(Type type, object[] args)
        {
            // Arrange
            var typeInfo = type.GetTypeInfo();

            // Act
            var method = ViewComponentMethodSelector.FindSyncMethod(typeInfo, args);

            // Assert
            Assert.Null(method);
        }

        [Theory]
        [InlineData(new object[] { new object[] { "Hello" } })]
        [InlineData(new object[] { new object[] { 4 } })]
        [InlineData(new object[] { new object[] { "", 5 } })]
        public void FindAsyncMethod_ThrowsIfInvokeAsyncDoesNotHaveCorrectReturnType(object[] args)
        {
            // Arrange
            var typeInfo = typeof(TypeWithInvalidInvokeAsync).GetTypeInfo();

            // Act and Assert
            var ex = Assert.Throws<InvalidOperationException>(
                () => ViewComponentMethodSelector.FindAsyncMethod(typeInfo, args));
            Assert.Equal("The async view component method 'InvokeAsync' should be declared to return Task<T>.",
                ex.Message);
        }

        [Theory]
        [InlineData(new object[] { 4 }, "The view component method 'Invoke' should be declared to return a value.")]
        [InlineData(new object[] { "" }, "The view component method 'Invoke' should not return a Task.")]
        public void FindSyncMethod_ThrowsIfInvokeSyncDoesNotHaveCorrectReturnType(object[] args, string expectedMessage)
        {
            // Arrange
            var typeInfo = typeof(TypeWithInvalidInvokeSync).GetTypeInfo();

            // Act and Assert
            var ex = Assert.Throws<InvalidOperationException>(
                () => ViewComponentMethodSelector.FindSyncMethod(typeInfo, args));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Theory]
        [InlineData(new object[] { "", }, "1")]
        [InlineData(new object[] { "", 2 }, "2")]
        [InlineData(new object[] { "", 0, 1 }, "3")]
        [InlineData(new object[] { 1, false, 1 }, "4")]
        public void FindAsyncMethod_ReturnsMethodMatchingParameters(object[] args, string expectedId)
        {
            // Arrange
            var typeInfo = typeof(ViewComponentWithAsyncInvoke).GetTypeInfo();

            // Act
            var method = ViewComponentMethodSelector.FindAsyncMethod(typeInfo, args);

            // Assert
            Assert.NotNull(method);
            var data = method.GetCustomAttribute<MethodDataAttribute>();
            Assert.Equal(expectedId, data.Data);
        }

        [Theory]
        [InlineData(typeof(ViewComponentWithSyncInvoke), new object[] { }, "1")]
        [InlineData(typeof(ViewComponentWithSyncInvoke), new object[] { 2, 3 }, "2")]
        [InlineData(typeof(ViewComponentWithSyncInvoke), new object[] { "", 0, true }, "3")]
        [InlineData(typeof(DerivedClass), new object[] { }, "Derived1")]
#if !DNXCORE50
        [InlineData(typeof(DerivedAgain), new object[] { "" }, "Derived2")]
#endif
        public void FindSyncMethod_ReturnsMethodMatchingParameters(Type type, object[] args, string expectedId)
        {
            // Arrange
            var typeInfo = type.GetTypeInfo();

            // Act
            var method = ViewComponentMethodSelector.FindSyncMethod(typeInfo, args);

            // Assert
            Assert.NotNull(method);
            var data = method.GetCustomAttribute<MethodDataAttribute>();
            Assert.Equal(expectedId, data.Data);
        }

        private class ViewComponentWithSyncInvoke
        {
            [MethodData("1")]
            public int Invoke() => 3;

            [MethodData("2")]
            public int Invoke(int a, int? b) => a + b.Value;

            [MethodData("3")]
            public int Invoke(string a, int b, bool? c) => 3;
        }

        private class ViewComponentWithAsyncInvoke
        {
            [MethodData("1")]
            public Task<string> InvokeAsync(string value) => Task.FromResult(value.ToUpperInvariant());

            [MethodData("2")]
            public Task<string> InvokeAsync(string a, int b) => Task.FromResult(a + b);

            [MethodData("3")]
            public Task<string> InvokeAsync(string a, int? b, int c) => Task.FromResult(a + b + c);

            [MethodData("4")]
            public Task<string> InvokeAsync(int? a, bool? b, int c) => Task.FromResult(a.ToString() + b + c);

            [MethodData("4")]
            public Task<string> InvokeAsync(object value) => Task.FromResult(value.ToString());
        }

        private class ViewComponentWithNonPublicNonInstanceInvokes
        {
            public static int Invoke() => 1;

            private int Invoke(string a) => 2;

            public static Task<int> InvokeAsync() => Task.FromResult(3);

            protected Task<string> InvokeAsync(string a) => Task.FromResult(a);
        }
        
        public class BaseClass
        {
            [MethodData("Base")]
            public static int Invoke() => 1;
        }

        public class DerivedClass : BaseClass
        {
            [MethodData("Derived1")]
            public new int Invoke() => 1;

            [MethodData("Derived2")]
            public int Invoke(string x) => 2;
        }

        public class DerivedAgain : DerivedClass
        {
            [MethodData("DerivedAgain")]
            public new static int Invoke(string x) => 2;
        }

        private class TypeWithInvalidInvokeAsync
        {
            public Task InvokeAsync(string value) => Task.FromResult(value);

            public void InvokeAsync(int value)
            {

            }

            public long InvokeAsync(string a, int b) => b;
        }

        private class TypeWithInvalidInvokeSync
        {
            public Task Invoke(string value) => Task.FromResult(value);

            public void Invoke(int value)
            {
            }
        }

        private class MethodDataAttribute : Attribute
        {
            public MethodDataAttribute(string data)
            {
                Data = data;
            }

            public string Data { get; }
        }
    }
}
