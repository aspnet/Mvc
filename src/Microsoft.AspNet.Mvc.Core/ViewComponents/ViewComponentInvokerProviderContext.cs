
using System;

namespace Microsoft.AspNet.Mvc
{
    public class ViewComponentInvokerProviderContext
    {
        public ViewComponentInvokerProviderContext([NotNull] Type componentType, object[] arguments)
        {
            ComponentType = componentType;
            Arguments = arguments;
        }

        public object[] Arguments { get; private set; }

        public Type ComponentType { get; private set; }

        public IViewComponentInvoker Result { get; set; }
    }
}
