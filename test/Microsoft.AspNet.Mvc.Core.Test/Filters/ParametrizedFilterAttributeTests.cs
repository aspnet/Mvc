// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Test
{
    public class ParametrizedFilterAttributeTests {
        public class StubAttribute : ParametrizedFilterAttribute {
        }

        [Fact]
        public void ParametrizedFilterAttribute_CreateInstance_WithValidService_ReturnsWrappedService()
        {
            // Arrange
            var attribute = new StubAttribute();
            var context = ActionFilterAttributeTests.CreateActionExecutingContext(Mock.Of<IFilter>());
            var filter = new Mock<IParametrizedFilter<StubAttribute>>();            

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(p => p.GetService(typeof(IParametrizedFilter<StubAttribute>)))
                .Returns(filter.Object);

            // Act
            var instance = attribute.CreateInstance(serviceProvider.Object) as ParametrizedFilterWrapper<StubAttribute>;
          
            // Assert
            Assert.NotNull(instance);
            instance.OnActionExecuting(context);
            filter.Verify(_ => _.OnActionExecuting(context, attribute));
        }

        [Fact]
        public void ParametrizedFilterAttribute_CreateInstance_WithInvalidService_Throws() {
            // Arrange
            var attribute = new StubAttribute();
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(p => p.GetService(It.IsAny<Type>()))
                .Returns("I am a bad service");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => 
                attribute.CreateInstance(serviceProvider.Object)
            );
        }
    }
}

