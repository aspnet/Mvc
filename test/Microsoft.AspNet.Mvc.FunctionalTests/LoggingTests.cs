// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if ASPNET50  // json won't serialize in CoreCLR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoggingWebSite;
using LoggingWebSite.Controllers;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Mvc.ApplicationModels;
using Microsoft.AspNet.Mvc.Logging;
using Microsoft.AspNet.TestHost;
using Microsoft.Framework.Logging;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class LoggingTests
    {
        private readonly IServiceProvider _services = TestHelper.CreateServices("LoggingWebSite");
        private readonly Action<IApplicationBuilder> _app = new Startup().Configure;

        private async Task<List<WriteContext>> GetContextsForStartup()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Startup", "true");

            // Act
            var response = await client.GetStringAsync("http://localhost/");

            var contexts = JsonConvert.DeserializeObject<List<WriteContext>>(response);

            // Assert
            Assert.NotEmpty(contexts);
            return contexts;
        }


        [Fact]
        public async Task AssemblyValues_LoggedAtStartup()
        {
            // Arrange and Act
            var contexts = await GetContextsForStartup();
            contexts = contexts.Where(c => c.StructureType.Equals(typeof(AssemblyValues))).ToList();

            // Assert
            foreach (var context in contexts)
            {
                dynamic assembly = context.State;
                Assert.NotNull(assembly);
                Assert.Equal("LoggingWebSite, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", assembly.AssemblyName.ToString());
            }
        }

        [Fact]
        public async Task IsControllerValues_LoggedAtStartup()
        {
            // Arrange and Act
            var contexts = await GetContextsForStartup();
            contexts = contexts.Where(c => c.StructureType.Equals(typeof(DefaultControllerModelBuilder))).ToList();

            // Assert
            foreach (var context in contexts)
            {
                dynamic isController = context.State;
                if (isController.Type.ToString().StartsWith(typeof(HomeController).ToString()))
                {
                    Assert.Equal(
                        ControllerStatus.IsController,
                        Enum.Parse(typeof(ControllerStatus), isController.Status.ToString()));
                }
                else
                {
                    Assert.NotEqual(ControllerStatus.IsController,
                        Enum.Parse(typeof(ControllerStatus), isController.Status.ToString()));
                }
            }
        }

        [Fact]
        public async Task ControllerModelValues_LoggedAtStartup()
        {
            // Arrange and Act
            var contexts = await GetContextsForStartup();
            contexts = contexts.Where(c => c.StructureType.Equals(typeof(ControllerModelValues))).ToList();

            // Assert
            Assert.Single(contexts);
            dynamic controller = contexts[0].State;
            Assert.Equal("Home", controller.ControllerName.ToString());
            Assert.Equal(typeof(HomeController).AssemblyQualifiedName, controller.ControllerType.ToString());
            Assert.Equal("Index", controller.Actions[0].ActionName.ToString());
            Assert.Empty(controller.ApiExplorer.IsVisible);
            Assert.Empty(controller.ApiExplorer.GroupName.ToString());
            Assert.Empty(controller.Attributes);
            Assert.Empty(controller.Filters);
            Assert.Empty(controller.ActionConstraints);
            Assert.Empty(controller.RouteConstraints);
            Assert.Empty(controller.AttributeRoutes);
        }

        [Fact]
        public async Task ActionDescriptorValues_LoggedAtStartup()
        {
            // Arrange and Act
            var contexts = await GetContextsForStartup();
            contexts = contexts.Where(c => c.StructureType.Equals(typeof(ActionDescriptorValues))).ToList();

            // Assert
            Assert.Single(contexts);
            dynamic action = contexts[0].State;
            Assert.Equal("Index", action.Name.ToString());
            Assert.Empty(action.Parameters);
            Assert.Empty(action.FilterDescriptors);
            Assert.Equal("controller", action.RouteConstraints[0].RouteKey.ToString());
            Assert.Equal("action", action.RouteConstraints[1].RouteKey.ToString());
            Assert.Empty(action.RouteValueDefaults);
            Assert.Empty(action.ActionConstraints.ToString());
            Assert.Empty(action.HttpMethods.ToString());
            Assert.Empty(action.Properties);
            Assert.Equal("Home", action.ControllerName.ToString());
        }
    }
}
#endif