
using System.IO;
using Microsoft.AspNet.Mvc.Rendering;

namespace Microsoft.AspNet.Mvc
{
    public class ViewComponentInvokerContext
    {
        public ViewComponentInvokerContext([NotNull] ViewContext viewContext, [NotNull] TextWriter writer)
        {
            ViewContext = viewContext;
            Writer = writer;
        }

        public TextWriter Writer { get; private set; }

        public ViewContext ViewContext { get; private set; }
    }
}
