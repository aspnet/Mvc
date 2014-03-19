
using System.IO;
using Microsoft.AspNet.Mvc.Rendering;

namespace Microsoft.AspNet.Mvc
{
    public class ViewComponentInvokerContext
    {
        public ViewComponentInvokerContext(ViewContext viewContext, TextWriter writer, object[] arguments)
        {
            ViewContext = viewContext;
            Writer = writer;
            Arguments = arguments;
        }

        public object[] Arguments { get; private set; }

        public TextWriter Writer { get; private set; }

        public ViewContext ViewContext { get; private set; }
    }
}
