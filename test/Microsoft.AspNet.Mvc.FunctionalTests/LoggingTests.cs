// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if ASPNET50  // json won't serialize in CoreCLR
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoggingWebSite;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using Microsoft.Framework.Logging;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class LoggingTests
    {
        private readonly IServiceProvider _services = TestHelper.CreateServices("LoggingWebSite", newServices: null, loggerFactory: new LoggerFactory());
        private readonly Action<IApplicationBuilder> _app = new Startup().Configure;
        private List<WriteContext> _contexts;

        private async Task<List<WriteContext>> GetContextsForStartup()
        {
            if (_contexts == null)
            {
                // Arrange
                var server = TestServer.Create(_services, _app);
                var client = server.CreateClient();

                // Act
                var response = await client.GetStringAsync("http://localhost/");

                _contexts = JsonConvert.DeserializeObject<List<WriteContext>>(response);

                // Assert
                Assert.NotEmpty(_contexts);
            }
            return _contexts;
        }
            

        [Fact]
        public async Task AssemblyValues_LoggedAtStartup()
        {
            // Arrange and Act
            var contexts = await GetContextsForStartup();

            // Assert
            dynamic assembly = contexts[0].State;
            Assert.NotNull(assembly);
            Assert.Equal("LoggingWebSite, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", assembly.AssemblyName.ToString());
            Assert.False(Boolean.Parse(assembly.IsDynamic.ToString()));
        }

        [Fact]
        public async Task IsControllerValues_LoggedAtStartup()
        {
            // Arrange and Act
            var contexts = await GetContextsForStartup();

            // Assert
            var potentialControllerCount = contexts.Count - 3; // 1 assembly, 1 controller, 1 action
            for (var i = 1; i <= potentialControllerCount; i++)
            {
                dynamic isController = contexts[i].State;
                if (isController.Type.ToString().StartsWith("LoggingWebSite.Controllers.HomeController"))
                {
                    Assert.Equal(0, int.Parse(isController.Status.ToString()));
                }
                else
                {
                    Assert.NotEqual(0, int.Parse(isController.Status.ToString()));
                }
            }
        }

        [Fact]
        public async Task ControllerModelValues_LoggedAtStartup()
        {
            // Arrange and Act
            var contexts = await GetContextsForStartup();

            // Assert
            dynamic controller = contexts[contexts.Count - 2].State;
            Assert.Equal("Home", controller.ControllerName.ToString());
            Assert.True(controller.ControllerType.ToString().StartsWith(typeof(LoggingWebSite.Controllers.HomeController).ToString()));
            Assert.Equal("Index", controller.Actions[0].ActionName.ToString());
            Assert.Null(controller.ApiExplorer.Isvisible);
            Assert.Empty(controller.ApiExplorer.GroupName.ToString());
            Assert.Equal("[]", controller.Attributes.ToString());
            Assert.Equal("[]", controller.Filters.ToString());
            Assert.Equal("[]", controller.ActionConstraints.ToString());
            Assert.Equal("[]", controller.RouteConstraints.ToString());
            Assert.Equal("[]", controller.AttributeRoutes.ToString());
        }

        [Fact]
        public async Task ActionDescriptorValues_LoggedAtStartup()
        {
            // Arrange and Act
            var contexts = await GetContextsForStartup();

            // Assert
            dynamic action = contexts[contexts.Count - 1].State;
            Assert.Equal("Index", action.Name.ToString());
            Assert.Equal("[]", action.Parameters.ToString());
            Assert.Equal("[]", action.FilterDescriptors.ToString());
            Assert.Equal("controller", action.RouteConstraints[0].RouteKey.ToString());
            Assert.Equal("action", action.RouteConstraints[1].RouteKey.ToString());
            Assert.Equal("{}", action.RouteValueDefaults.ToString());
            Assert.Empty(action.ActionConstraints.ToString());
            Assert.Empty(action.HttpMethods.ToString());
            Assert.Equal("{}", action.Properties.ToString());
            Assert.Equal("Home", action.ControllerName.ToString());
        }
    }
}
#endif