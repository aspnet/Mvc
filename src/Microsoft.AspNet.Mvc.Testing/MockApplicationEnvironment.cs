// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.Versioning;
using Microsoft.Framework.Runtime;

namespace Microsoft.AspNet.Mvc.Testing
{
    internal class MockApplicationEnvironment : IApplicationEnvironment
    {
        public string ApplicationBasePath
        {
            get;
            set;
        }

        public string ApplicationName
        {
            get;
            set;
        }

        public string Configuration
        {
            get;
            set;
        }

        public FrameworkName RuntimeFramework
        {
            get;
            set;
        }

        public string Version
        {
            get;
            set;
        }
    }
}