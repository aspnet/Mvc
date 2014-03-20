using System;
using System.Runtime.Versioning;
using Microsoft.Net.Runtime;

namespace Microsoft.AspNet.Mvc.FunctionalTest.Testing
{
    public class TestApplicationEnvironment : IApplicationEnvironment
    {
        public string ApplicationName
        {
            get { return "Microsoft.AspNet.Mvc.FunctionalTest"; }
        }

        public string Version
        {
            get { return "0.1-alpha"; }
        }

        public string ApplicationBasePath
        {
            get { return Environment.CurrentDirectory; }
        }

        public FrameworkName TargetFramework
        {
            get { return new FrameworkName(".NET Framework", new Version("4.5")); }
        }
    }
}
