// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.Versioning;
using Microsoft.Dnx.Runtime;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    // Represents an application environment that overrides the base path of the original
    // application environment in order to make it point to the folder of the original web
    // aplication so that components like ViewEngines can find views as if they were executing
    // in a regular context.
    public class TestApplicationEnvironment : IApplicationEnvironment
    {
        private readonly IApplicationEnvironment _originalAppEnvironment;
        private readonly string _applicationBasePath;
        private readonly string _applicationName;

        public TestApplicationEnvironment(IApplicationEnvironment originalAppEnvironment, string appBasePath, string appName)
        {
            _originalAppEnvironment = originalAppEnvironment;
            _applicationBasePath = appBasePath;
            _applicationName = appName;
        }

        public string ApplicationName
        {
            get { return _applicationName; }
        }

        public string ApplicationVersion
        {
            get { return _originalAppEnvironment.ApplicationVersion; }
        }

        public string ApplicationBasePath
        {
            get { return _applicationBasePath; }
        }

        public string Configuration
        {
            get
            {
                return _originalAppEnvironment.Configuration;
            }
        }

        public FrameworkName RuntimeFramework
        {
            get { return _originalAppEnvironment.RuntimeFramework; }
        }

        public object GetData(string name)
        {
            return _originalAppEnvironment.GetData(name);
        }

        public void SetData(string name, object value)
        {
            _originalAppEnvironment.SetData(name, value);
        }
    }
}