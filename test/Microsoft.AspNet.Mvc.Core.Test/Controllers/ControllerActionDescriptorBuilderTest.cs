// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Mvc.ApplicationModels;
using Microsoft.AspNet.Mvc.ModelBinding;
using Xunit;

namespace Microsoft.AspNet.Mvc.Controllers
{
    public class ControllerActionDescriptorBuilderTest
    {
        [Fact]
        public void Build_WithControllerPropertiesSet_AddsPropertiesWithBinderMetadataSet()
        {
            // Arrange
            var applicationModel = new ApplicationModel();
            var controller = new ControllerModel(
                typeof(TestController).GetTypeInfo(),
                new List<object>() { });

            var propertyInfo = controller.ControllerType.GetProperty("BoundProperty");
            controller.ControllerProperties.Add(
                new PropertyModel(
                    propertyInfo,
                    new List<object>() { })
                {
                    BindingInfo = BindingInfo.GetBindingInfo(new object[] { new FromQueryAttribute() }),
                    PropertyName = "BoundProperty"
                });

            controller.ControllerProperties.Add(
               new PropertyModel(controller.ControllerType.GetProperty("UnboundProperty"), new List<object>() { }));

            controller.Application = applicationModel;
            applicationModel.Controllers.Add(controller);

            var methodInfo = typeof(TestController).GetMethod("SomeAction");
            var actionModel = new ActionModel(methodInfo, new List<object>() { });
            actionModel.Controller = controller;
            controller.Actions.Add(actionModel);

            // Act
            var descriptors = ControllerActionDescriptorBuilder.Build(applicationModel);

            // Assert
            var controllerDescriptor = Assert.Single(descriptors);

            var parameter = Assert.Single(controllerDescriptor.BoundProperties);
            var property = Assert.IsType<ControllerBoundPropertyDescriptor>(parameter);
            Assert.Equal("BoundProperty", property.Name);
            Assert.Equal(propertyInfo, property.PropertyInfo);
            Assert.Equal(typeof(string), property.ParameterType);
            Assert.Equal(BindingSource.Query, property.BindingInfo.BindingSource);
        }

        [Fact]
        public void Build_WithPropertiesSet_FromApplicationModel()
        {
            // Arrange
            var applicationModel = new ApplicationModel();
            applicationModel.Properties["test"] = "application";

            var controller = new ControllerModel(typeof(TestController).GetTypeInfo(),
                                                 new List<object>() { });
            controller.Application = applicationModel;
            applicationModel.Controllers.Add(controller);

            var methodInfo = typeof(TestController).GetMethod("SomeAction");
            var actionModel = new ActionModel(methodInfo, new List<object>() { });
            actionModel.Controller = controller;
            controller.Actions.Add(actionModel);

            // Act
            var descriptors = ControllerActionDescriptorBuilder.Build(applicationModel);

            // Assert
            Assert.Equal("application", descriptors.Single().Properties["test"]);
        }

        [Fact]
        public void Build_WithPropertiesSet_ControllerOverwritesApplicationModel()
        {
            // Arrange
            var applicationModel = new ApplicationModel();
            applicationModel.Properties["test"] = "application";

            var controller = new ControllerModel(typeof(TestController).GetTypeInfo(),
                                                 new List<object>() { });
            controller.Application = applicationModel;
            controller.Properties["test"] = "controller";
            applicationModel.Controllers.Add(controller);

            var methodInfo = typeof(TestController).GetMethod("SomeAction");
            var actionModel = new ActionModel(methodInfo, new List<object>() { });
            actionModel.Controller = controller;
            controller.Actions.Add(actionModel);

            // Act
            var descriptors = ControllerActionDescriptorBuilder.Build(applicationModel);

            // Assert
            Assert.Equal("controller", descriptors.Single().Properties["test"]);
        }

        [Fact]
        public void Build_WithPropertiesSet_ActionOverwritesApplicationAndControllerModel()
        {
            // Arrange
            var applicationModel = new ApplicationModel();
            applicationModel.Properties["test"] = "application";

            var controller = new ControllerModel(typeof(TestController).GetTypeInfo(),
                                                 new List<object>() { });
            controller.Application = applicationModel;
            controller.Properties["test"] = "controller";
            applicationModel.Controllers.Add(controller);

            var methodInfo = typeof(TestController).GetMethod("SomeAction");
            var actionModel = new ActionModel(methodInfo, new List<object>() { });
            actionModel.Controller = controller;
            actionModel.Properties["test"] = "action";
            controller.Actions.Add(actionModel);

            // Act
            var descriptors = ControllerActionDescriptorBuilder.Build(applicationModel);

            // Assert
            Assert.Equal("action", descriptors.Single().Properties["test"]);
        }

        private class TestController
        {
            [FromQuery]
            public string BoundProperty { get; set; }

            public string UnboundProperty { get; set; }

            public void SomeAction() { }
        }
    }
}