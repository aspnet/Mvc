using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc.ApplicationParts
{
    public class AssemblyPart : ApplicationPart, IExportTypes
    {
        private readonly Assembly _assembly;

        public AssemblyPart(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            _assembly = assembly;
        }

        public override string Name => _assembly.GetName().Name;

        public IEnumerable<TypeInfo> Types => _assembly.DefinedTypes;
    }
}
