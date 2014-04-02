using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNet.Mvc.FunctionalTest.Testing
{
    public class ClassAssemblyControllerProvider<T> : IControllerAssemblyProvider
    {
        public IEnumerable<Assembly> CandidateAssemblies
        {
            get { yield return typeof(T).Assembly; }
        }
    }
}
