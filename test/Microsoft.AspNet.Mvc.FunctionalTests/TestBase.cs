// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Runtime.Infrastructure;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    /// <summary>
    /// Summary description for TestBase
    /// </summary>
    public abstract class TestBase
    {
        private readonly IServiceProvider _provider;
        private readonly Action<IBuilder> _app;
        private TestServer _server;

        public TestBase(string appName, Action<IBuilder> appBuilderFactory)
        {
            _app = appBuilderFactory;

            var originalProvider = CallContextServiceLocator.Locator.ServiceProvider;
            var appEnvironment = originalProvider.GetService<IApplicationEnvironment>();

            // When an application executes in a regular context, the application base path points to the root
            // directory where the application is located, for example MvcSample.Web. However, when executing
            // an aplication as part of a test, the ApplicationBasePath of the IApplicationEnvironment points
            // to the root folder of the test project.
            // To compensate for this, we need to calculate the original path and override the application
            // environment value so that components like the view engine work properly in the context of the
            // test.
            string appBasePath = CalculateApplicationBasePath(appEnvironment, appName);
            _provider = new ServiceCollection()
                .AddInstance(typeof(IApplicationEnvironment), new TestApplicationEnvironment(appEnvironment, appBasePath))
                .BuildServiceProvider(originalProvider);
        }

        public IServiceProvider ServiceProvider
        {
            get { return _provider; }
        }

        public Action<IBuilder> App
        {
            get { return _app; }
        }

        public TestServer Server
        {
            get
            {
                if (_server == null)
                {
                    _server = TestServer.Create(_provider, _app);
                }
                return _server;
            }
        }

        public TestClient Client
        {
            get { return Server.Handler; }
        }

        // Calculate the path relative to the current application base path.
        private static string CalculateApplicationBasePath(IApplicationEnvironment appEnvironment,
                                                           string appName)
        {
            // Mvc/test/Microsoft.AspNet.Mvc.FunctionalTests
            var appBase = appEnvironment.ApplicationBasePath;

            // Mvc/test
            var test = Path.GetDirectoryName(appBase);

            // Mvc/test/WebSites/BasicWebSite
            return Path.GetFullPath(Path.Combine(appBase, "..", "WebSites", appName));
        }
    }
}