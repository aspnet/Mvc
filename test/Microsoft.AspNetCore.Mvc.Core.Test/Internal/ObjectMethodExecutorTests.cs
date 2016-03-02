// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Testing;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Core.Test.Internal
{
    public class ObjectMethodExecutorTests
    {
        private TestObject _targetObject = new TestObject();
        private TypeInfo targetTypeInfo = typeof(TestObject).GetTypeInfo();

        [Fact]
        public void ExecuteStaticVoidMethod()
        {
            var executor = GetExecutorForMethod("StaticVoidMethod");
            var result = executor.Execute(null, null);
            Assert.Same(null, result);
        }

        [Fact]
        public void ExecuteStaticValueMethod()
        {
            var executor = GetExecutorForMethod("StaticValueMethod");
            var result = executor.Execute(null, new object[] { 10 });
            Assert.Equal(10, (int)result);
        }

        [Fact]
        public void ExecuteStaticMethodThrowsException()
        {
            var executor = GetExecutorForMethod("StaticMethodThrowsException");
            Assert.Throws<NotImplementedException>(
                        () => executor.Execute(null, new object[] { 10 }));
            
        }

        [Fact]
        public void ExecuteValueMethod()
        {
            var executor = GetExecutorForMethod("ValueMethod");
            var result = executor.Execute(_targetObject, new object[] { 10 , 20 });
            Assert.Equal(30, (int)result);
        }

        [Fact]
        public void ExecuteVoidValueMethod()
        {
            var executor = GetExecutorForMethod("VoidValueMethod");
            var result = executor.Execute(_targetObject, new object[] { 10 });
            Assert.Same(null, result);
        }

        [Fact]
        public void ExecuteValueMethodWithReturnType()
        {
            var executor = GetExecutorForMethod("ValueMethodWithReturnType");
            var result = executor.Execute(_targetObject, new object[] { 10 });
            Assert.True(result is TestObject);
            TestObject resultObject = (TestObject)result;
            Assert.Equal("Hello", resultObject.value);
        }

        [Fact]
        public void ExecuteValueMethodUpdateValue()
        {
            var executor = GetExecutorForMethod("ValueMethodUpdateValue");
            TestObject parameter = new TestObject();
            var result = executor.Execute(_targetObject, new object[] { parameter });
            Assert.True(result is TestObject);
            TestObject resultObject = (TestObject)result;
            Assert.Equal("HelloWorld", resultObject.value);
        }

        [Fact]
        public void ExecuteValueMethodWithReturnTypeThrowsException()
        {
            var executor = GetExecutorForMethod("ValueMethodWithReturnTypeThrowsException");
            TestObject parameter = new TestObject();
            Assert.Throws<NotImplementedException>(
                        () => executor.Execute(_targetObject, new object[] { parameter }));
        }

        private ObjectMethodExecutor GetExecutorForMethod(string methodName)
        {
            var method = typeof(TestObject).GetMethod(methodName);
            var executor = new ObjectMethodExecutor(method, targetTypeInfo);
            return executor;
        }

        public class TestObject
        {            
            public string value;
            public static void StaticVoidMethod()
            {

            }
            public static int StaticValueMethod(int i)
            {
                return i;
            }

            public static int StaticMethodThrowsException(int i)
            {
                throw new NotImplementedException("Not Implemented Exception");
            }

            public int ValueMethod(int i, int j)
            {
                return i+j;
            }

            public void VoidValueMethod(int i)
            {
                
            }
            public TestObject ValueMethodWithReturnType(int i)
            {
                return new TestObject() { value = "Hello" }; ;
            }

            public TestObject ValueMethodWithReturnTypeThrowsException(TestObject i)
            {
                throw new NotImplementedException("Not Implemented Exception");
            }

            public TestObject ValueMethodUpdateValue(TestObject parameter)
            {
                parameter.value = "HelloWorld";
                return parameter;
            }
        }
    }
}
