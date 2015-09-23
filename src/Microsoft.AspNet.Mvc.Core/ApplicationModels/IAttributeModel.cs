using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.ApplicationModels
{
    public interface IAttributeModel
    {
        IReadOnlyList<object> Attributes { get; }
    }
}