using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;

namespace InlineConstraints
{
    public class TestControllerAssemblyProvider : IControllerAssemblyProvider
    {
        public IEnumerable<Assembly> CandidateAssemblies
        {
            get
            {
                return new[] { Assembly.GetExecutingAssembly() };
            }
        }
    }
}
