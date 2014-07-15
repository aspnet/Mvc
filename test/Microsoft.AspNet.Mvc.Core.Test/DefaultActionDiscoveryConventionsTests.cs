// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.AspNet.Mvc.Test.TestControllers;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultActionDiscoveryConventionsTests
    {
        [Theory]
        [InlineData("GetFromDerived", true)]
        [InlineData("NewMethod", true)] // "NewMethod" is a public method declared with keyword "new".
        [InlineData("GetFromBase", true)]
        public void IsValidActionMethod_WithInheritedMethods(string methodName, bool expected)
        {
            // Arrange
            var conventions = new DefaultActionDiscoveryConventions();
            var method = typeof(DerivedController).GetMethod(methodName);
            Assert.NotNull(method);

            // Act
            var isValid = conventions.IsValidActionMethod(method);

            // Assert
            Assert.Equal(expected, isValid);
        }

        [Fact]
        public void IsValidActionMethod_OverridenMethodControllerClass()
        {
            // Arrange
            var conventions = new DefaultActionDiscoveryConventions();
            var method = typeof(BaseController).GetMethod("Redirect");
            Assert.NotNull(method);

            // Act
            var isValid = conventions.IsValidActionMethod(method);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void IsValidActionMethod_PrivateMethod_FromUserDefinedController()
        {
            // Arrange
            var conventions = new DefaultActionDiscoveryConventions();
            var method = typeof(DerivedController).GetMethod(
                "PrivateMethod",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(method);

            // Act
            var isValid = conventions.IsValidActionMethod(method);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void IsValidActionMethod_OperatorOverloadingMethod_FromOperatorOverloadingController()
        {
            // Arrange
            var conventions = new DefaultActionDiscoveryConventions();
            var method = typeof(OperatorOverloadingController).GetMethod("op_Addition");
            Assert.NotNull(method);
            Assert.True(method.IsSpecialName);

            // Act
            var isValid = conventions.IsValidActionMethod(method);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void IsValidActionMethod_GenericMethod_FromUserDefinedController()
        {
            // Arrange
            var conventions = new DefaultActionDiscoveryConventions();
            var method = typeof(DerivedController).GetMethod("GenericMethod");
            Assert.NotNull(method);

            // Act
            var isValid = conventions.IsValidActionMethod(method);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void IsValidActionMethod_OverridenNonActionMethod()
        {
            // Arrange
            var conventions = new DefaultActionDiscoveryConventions();
            var method = typeof(DerivedController).GetMethod("OverridenNonActionMethod");
            Assert.NotNull(method);

            // Act
            var isValid = conventions.IsValidActionMethod(method);

            // Assert
            Assert.False(isValid);
        }

        [Theory]
        [InlineData("Equals")]
        [InlineData("GetHashCode")]
        [InlineData("MemberwiseClone")]
        [InlineData("ToString")]
        public void IsValidActionMethod_OverriddenMethodsFromObjectClass(string methodName)
        {
            // Arrange
            var conventions = new DefaultActionDiscoveryConventions();
            var method = typeof(DerivedController).GetMethod(
                methodName, 
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(method);

            // Act
            var isValid = conventions.IsValidActionMethod(method);

            // Assert
            Assert.False(isValid);
        }

        [Theory]
        [InlineData("StaticMethod")]
        [InlineData("ProtectedStaticMethod")]
        [InlineData("PrivateStaticMethod")]
        public void IsValidActionMethod_StaticMethods(string methodName)
        {
            // Arrange
            var conventions = new DefaultActionDiscoveryConventions();
            var method = typeof(DerivedController).GetMethod(
                methodName, 
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.NotNull(method);

            // Act
            var isValid = conventions.IsValidActionMethod(method);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void IsController_UserDefinedClass()
        {
            // Arrange
            var conventions = new DefaultActionDiscoveryConventions();
            var typeInfo = typeof(BaseController).GetTypeInfo();

            // Act
            var isController = conventions.IsController(typeInfo);

            // Assert
            Assert.True(isController);
        }

        [Fact]
        public void IsController_FrameworkControllerClass()
        {
            // Arrange
            var conventions = new DefaultActionDiscoveryConventions();
            var typeInfo = typeof(Controller).GetTypeInfo();

            // Act
            var isController = conventions.IsController(typeInfo);

            // Assert
            Assert.False(isController);
        }

        [Fact]
        public void IsController_UserDefinedControllerClass()
        {
            // Arrange
            var conventions = new DefaultActionDiscoveryConventions();
            var typeInfo = typeof(Test.TestControllers.Controller).GetTypeInfo();

            // Act
            var isController = conventions.IsController(typeInfo);

            // Assert
            Assert.False(isController);
        }

        [Fact]
        public void IsController_Interface()
        {
            // Arrange
            var conventions = new DefaultActionDiscoveryConventions();
            var typeInfo = typeof(IController).GetTypeInfo();

            // Act
            var isController = conventions.IsController(typeInfo);

            // Assert
            Assert.False(isController);
        }

        [Fact]
        public void IsController_AbstractClass()
        {
            // Arrange
            var conventions = new DefaultActionDiscoveryConventions();
            var typeInfo = typeof(AbstractController).GetTypeInfo();

            // Act
            var isController = conventions.IsController(typeInfo);

            // Assert
            Assert.False(isController);
        }

        [Fact]
        public void IsController_DerivedAbstractClass()
        {
            // Arrange
            var conventions = new DefaultActionDiscoveryConventions();
            var typeInfo = typeof(DerivedAbstractController).GetTypeInfo();

            // Act
            var isController = conventions.IsController(typeInfo);

            // Assert
            Assert.True(isController);
        }

        [Fact]
        public void IsController_OpenGenericClass()
        {
            // Arrange
            var conventions = new DefaultActionDiscoveryConventions();
            var typeInfo = typeof(OpenGenericController<>).GetTypeInfo();

            // Act
            var isController = conventions.IsController(typeInfo);

            // Assert
            Assert.False(isController);
        }

        [Fact]
        public void IsController_ClosedGenericClass()
        {
            // Arrange
            var conventions = new DefaultActionDiscoveryConventions();
            var typeInfo = typeof(OpenGenericController<string>).GetTypeInfo();

            // Act
            var isController = conventions.IsController(typeInfo);

            // Assert
            Assert.False(isController);
        }

        [Fact]
        public void IsController_DerivedGenericClass()
        {
            // Arrange
            var conventions = new DefaultActionDiscoveryConventions();
            var typeInfo = typeof(DerivedGenericController).GetTypeInfo();

            // Act
            var isController = conventions.IsController(typeInfo);

            // Assert
            Assert.True(isController);
        }

        [Fact]
        public void IsController_Poco_WithNamingConvention()
        {
            // Arrange
            var conventions = new DefaultActionDiscoveryConventions();
            var typeInfo = typeof(PocoController).GetTypeInfo();

            // Act
            var isController = conventions.IsController(typeInfo);

            // Assert
            Assert.True(isController);
        }

        [Fact]
        public void IsController_NoControllerSuffix()
        {
            // Arrange
            var conventions = new DefaultActionDiscoveryConventions();
            var typeInfo = typeof(NoSuffix).GetTypeInfo();

            // Act
            var isController = conventions.IsController(typeInfo);

            // Assert
            Assert.True(isController);
        }
    }
}