using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Filters;

namespace Microsoft.AspNet.Mvc.ApplicationModels
{
    public interface ICommonModel
    {
        IList<IFilterMetadata> Filters { get; }
        IDictionary<object, object> Properties { get; }
        ApiExplorerModel ApiExplorer { get; set; }
    }
}
