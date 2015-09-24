using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Filters;

namespace Microsoft.AspNet.Mvc.ApplicationModels
{
    public interface IFilteringModel
    {
        IList<IFilterMetadata> Filters { get; }
    }
}