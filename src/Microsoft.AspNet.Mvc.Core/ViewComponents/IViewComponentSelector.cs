
using System;

namespace Microsoft.AspNet.Mvc
{
    public interface IViewComponentSelector
    {
        Type SelectComponent(string componentName);
    }
}
