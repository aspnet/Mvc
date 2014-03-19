using System;
using System.Text;
using Microsoft.AspNet.Abstractions;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class InputFormatterContext
    {
        public InputFormatterContext([NotNull] HttpContext httpContext,
                                     [NotNull] ModelMetadata metadata, 
                                     [NotNull] ModelStateDictionary modelState)
        {
            HttpContext = httpContext;
            Metadata = metadata;
            ModelState = modelState;
        }

        public HttpContext HttpContext { get; private set; }

        public Encoding ContentEncoding { get; private set; }

        public ModelMetadata Metadata { get; private set; }

        public ModelStateDictionary ModelState { get; private set; }

        public object Model { get; set; }
    }
}
