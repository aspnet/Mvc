
using System.IO;

namespace Microsoft.AspNet.Mvc
{
    public class ComponentInvokerContext
    {
        public ComponentInvokerContext(ViewContext viewContext, TextWriter writer)
        {
            ViewContext = viewContext;
            Writer = writer;
        }

        public TextWriter Writer { get; private set; }

        public ViewContext ViewContext { get; private set; }
    }
}
