// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNet.Mvc.ApplicationModels;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultControllerModelConventionTest
    {
        [Fact]
        public void DefaultControllerModelConvention_AppliesToAllControllers()
        {
            // Arrange
            var app = new ApplicationModel();
            app.Controllers.Add(new ControllerModel(typeof(HelloController).GetTypeInfo(), new List<object>()));
            app.Controllers.Add(new ControllerModel(typeof(WorldController).GetTypeInfo(), new List<object>()));
            var defaultConvention = new DefaultControllerModelConvention(new SimpleControllerConvention());

            // Act
            defaultConvention.Apply(app);

            // Assert
            foreach (var controller in app.Controllers)
            {
                Assert.True(controller.Properties.ContainsKey("TestProperty"));
            }
        }

        private class HelloController : Controller { }
        private class WorldController : Controller { }

        private class SimpleControllerConvention : IControllerModelConvention
        {
            public void Apply([NotNull] ControllerModel controller)
            {
                controller.Properties.Add("TestProperty", "TestValue");
            }
        }
    }
}