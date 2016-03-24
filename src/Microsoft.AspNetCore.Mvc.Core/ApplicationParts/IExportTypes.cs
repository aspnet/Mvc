using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc.ApplicationParts
{
    public interface IExportTypes
    {
        IEnumerable<TypeInfo> Types { get; }
    }
}
