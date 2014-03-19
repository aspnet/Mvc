
using System;
using Microsoft.AspNet.Mvc.Rendering;

namespace Microsoft.AspNet.Mvc
{
    public class ViewComponentInvokerProviderContext
    {
        public ViewComponentInvokerProviderContext([NotNull] Type componentType, [NotNull] object[] arguments, [NotNull] ViewContext viewContext)
        {
            ComponentType = componentType;
            Arguments = arguments;
            ViewContext = viewContext;
        }

        public object[] Arguments { get; private set; }

        public Type ComponentType { get; private set; }

        public IViewComponentInvoker Result { get; set; }

        public ViewContext ViewContext { get; private set; }
    }
}
