﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoggingWebSite;
using LoggingWebSite.Controllers;
using LoggingWebSite.Models;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Mvc.Logging;
using Microsoft.AspNet.TestHost;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class LoggingTests
    {
        private readonly IServiceProvider _services = TestHelper.CreateServices(nameof(LoggingWebSite));
        private readonly Action<IApplicationBuilder> _app = new LoggingWebSite.Startup().Configure;
        
        [Fact]
        public async Task AssemblyValues_LoggedAtStartup()
        {
            // Arrange and Act
            var logs = await GetLogsForStartupAsync();
            logs = logs.Where(c => c.StateType.Equals(typeof(AssemblyValues))).ToList();

            // Assert
            Assert.NotEmpty(logs);
            foreach (var log in logs)
            {
                dynamic assembly = log.State;
                Assert.NotNull(assembly);
                Assert.Equal(
                    "LoggingWebSite, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", 
                    assembly.AssemblyName.ToString());
            }
        }

        [Fact]
        public async Task IsControllerValues_LoggedAtStartup()
        {
            // Arrange and Act
            var logs = await GetLogsForStartupAsync();
            logs = logs.Where(c => c.StateType.Equals(typeof(IsControllerValues)));

            // Assert
            Assert.NotEmpty(logs);
            foreach (var log in logs)
            {
                dynamic isController = log.State;
                if (string.Equals(typeof(HomeController).AssemblyQualifiedName, isController.Type.ToString()))
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
            var logs = await GetLogsForStartupAsync();
            logs = logs.Where(c => c.StateType.Equals(typeof(ControllerModelValues)));

            // Assert
            Assert.Single(logs);
            dynamic controller = logs.First().State;
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
            var logs = await GetLogsForStartupAsync();
            logs = logs.Where(c => c.StateType.Equals(typeof(ActionDescriptorValues))).ToList();

            // Assert
            Assert.Single(logs);
            dynamic action = logs.First().State;
            Assert.Equal("Index", action.Name.ToString());
            Assert.Empty(action.Parameters);
            Assert.Empty(action.FilterDescriptors);
            Assert.Equal("action", action.RouteConstraints[0].RouteKey.ToString());
            Assert.Equal("controller", action.RouteConstraints[1].RouteKey.ToString());
            Assert.Empty(action.RouteValueDefaults);
            Assert.Empty(action.ActionConstraints.ToString());
            Assert.Empty(action.HttpMethods.ToString());
            Assert.Empty(action.Properties);
            Assert.Equal("Home", action.ControllerName.ToString());
        }

        private async Task<IEnumerable<LogInfoDto>> GetLogsForStartupAsync()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetStringAsync("http://localhost/logs");

            var activityDtos = JsonConvert.DeserializeObject<List<ActivityContextDto>>(response);
            var logInfos = GetAllLogInfos(activityDtos);

            // Assert
            Assert.NotEmpty(logInfos);
            return logInfos;
        }

        // Elm logs are arranged in the form of activities. Each activity could
        // represent a tree of nodes. So here we traverse through the tree to get a flat list of
        // log messages for us to enable verifying in the test.
        private IEnumerable<LogInfoDto> GetAllLogInfos(IEnumerable<ActivityContextDto> activities)
        {
            // Build a flat list of log messages from the log node tree 
            var logInfos = new List<LogInfoDto>();
            foreach (var activity in activities)
            {
                if (!activity.RepresentsScope)
                {
                    // message not within a scope
                    var logInfo = activity.Root.Messages.FirstOrDefault();
                    logInfos.Add(logInfo);
                }
                else
                {
                    Traverse(activity.Root, logInfos);
                }
            }

            return logInfos;
        }

        private void Traverse(ScopeNodeDto node, IList<LogInfoDto> logInfoDtos)
        {
            foreach (var logInfo in node.Messages)
            {
                logInfoDtos.Add(logInfo);
            }

            foreach (var scopeNode in node.Children)
            {
                Traverse(scopeNode, logInfoDtos);
            }
        }
    }
}
