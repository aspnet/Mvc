using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.ApplicationModels
{
    public interface IPropertyModel
    {
        IDictionary<object, object> Properties { get; }
    }
}