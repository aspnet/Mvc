// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LoggingWebSite.Controllers;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Diagnostics.Elm;
using Microsoft.AspNet.Mvc.Logging;
using Microsoft.AspNet.TestHost;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class LoggingTests
    {
        private readonly Action<IApplicationBuilder> _app = new LoggingWebSite.Startup().Configure;
        private const string RequestTraceIdKey = "RequestTraceId";
        private const string StartupLogsKey = "StartupLogs";

        [Fact]
        public async Task ControllerDiscovery()
        {
            // Arrange
            var elmStore = new ElmStore();
            var server = TestServer.Create(GetServiceProvider(elmStore), _app);
            var client = server.CreateClient();
            
            // Act & Assert

            // regular request
            var response = await client.GetAsync("http://localhost/Home/Index");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var data = await response.Content.ReadAsStringAsync();
            Assert.Equal("Home.Index", data);

            //verify logs
            var logMessages = GetLogMessages(elmStore.GetActivities());
            var controllerModelValuesList = logMessages.Where(msg =>
            {
                return msg.Name == "Microsoft.AspNet.Mvc.ControllerActionDescriptorProvider" &&
                        msg.State != null 
                        && msg.State.GetType().Equals(typeof(ControllerModelValues));
            });

            Assert.Equal(1, controllerModelValuesList.Count());
            var homeControllerLogInfo = controllerModelValuesList.First().State as ControllerModelValues;
            Assert.NotNull(homeControllerLogInfo);
            Assert.Equal(typeof(HomeController), homeControllerLogInfo.ControllerType);
            Assert.Equal("Home", homeControllerLogInfo.ControllerName);
            Assert.Equal(1, homeControllerLogInfo.Actions.Count);
            Assert.Equal("Index", homeControllerLogInfo.Actions[0].ActionName);
        }

        private IServiceProvider GetServiceProvider(ElmStore elmStore)
        {
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory, LoggerFactory>(); //TODO: can TestHelper add this always?
            services.AddInstance(elmStore);
            return TestHelper.CreateServices("LoggingWebSite", services);
        }

        /// <summary>
        /// Elm logs are arranged in the form of activities. Each activity could
        /// represent a tree of nodes. So here we traverse through the tree to get a flat list of
        /// log messages for us to enable verifying in the test.
        /// </summary>
        /// <param name="activities">ElmStore's log activities</param>
        /// <returns>Log messages</returns>
        private IList<LogInfo> GetLogMessages(IEnumerable<ActivityContext> activities)
        {
            // Build a flat list of log messages from the log node tree 
            var logMessages = new List<LogInfo>();
            foreach (var activity in activities.Reverse())
            {
                if (!activity.RepresentsScope)
                {
                    // message not within a scope
                    var logInfo = activity.Root.Messages.FirstOrDefault();
                    logMessages.Add(logInfo);
                }
                else
                {
                    Traverse(activity.Root, logMessages);
                }
            }

            return logMessages;
        }

        private void Traverse(ScopeNode node, IList<LogInfo> logMessages)
        {
            // Capture start of scope
            logMessages.Add(new LogInfo()
            {
                Name = node.Name,
                Message = "Begin-Scope:" + node.State
            });

            // Since Elm's ScopeNode arranges the log information in two separate lists, 
            // I am resorting to this weird way of comparing the entries. Following are some issues:
            // a. https://github.com/aspnet/Diagnostics/issues/77
            // b. Sorting by time does not yield correct numbers as I have noticed that the ticks between
            //    the log entries sometimes do not change(probably because the log entries happen pretty fast)
            //    and so the sort can give incorrect order sometimes. Ideally this is an issue with ElmLogger,
            //    where they should maintain a single list of entries to which one can either add a regular message
            //    node or a scope node.
            var list = new List<object>();
            list.AddRange(node.Messages);
            list.AddRange(node.Children);
            list.Sort(new LogTimeComparer());

            foreach (var obj in list)
            {
                // check if the current node is a regular message node
                // or a scope node
                var logInfo = obj as LogInfo;

                if (logInfo != null)
                {
                    logMessages.Add(logInfo);
                }
                else
                {
                    Traverse((ScopeNode)obj, logMessages);
                }
            }

            // Capture end of scope
            logMessages.Add(new LogInfo()
            {
                Name = node.Name,
                Message = "End-Scope:" + node.State
            });
        }
        
        public class LogTimeComparer : IComparer<object>
        {
            public int Compare(object x, object y)
            {
                LogInfo xLogInfo = x as LogInfo;
                ScopeNode xScopeNode = x as ScopeNode;

                LogInfo yLogInfo = y as LogInfo;
                ScopeNode yScopeNode = y as ScopeNode;

                if (xLogInfo != null)
                {
                    if (yLogInfo != null)
                    {
                        return xLogInfo.Time.CompareTo(yLogInfo.Time);
                    }
                    else
                    {
                        return xLogInfo.Time.CompareTo(yScopeNode.StartTime);
                    }
                }
                else
                {
                    if (yScopeNode != null)
                    {
                        return xScopeNode.StartTime.CompareTo(yScopeNode.StartTime);
                    }
                    else
                    {
                        return xScopeNode.StartTime.CompareTo(yLogInfo.Time);
                    }
                }
            }
        }
    }
}