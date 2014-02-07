using System;
using System.IO;
using Microsoft.AspNet.Abstractions;
using Newtonsoft.Json;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class JsonModelBinder : IBodyModelBinder
    {
        public bool BindModel(ModelBindingContext bindingContext)
        {
            HttpRequest request = bindingContext.HttpContext.Request;
            if (!IsSupportedContentType(request))
            {
                return false;
            }

            using (JsonReader jsonReader = new JsonTextReader(new StreamReader(request.Body)))
            {
                jsonReader.CloseInput = false;

                var jsonSerializer = JsonSerializer.CreateDefault();
                EventHandler<Newtonsoft.Json.Serialization.ErrorEventArgs> errorHandler = (sender, e) =>
                {
                    // Error must always be marked as handled
                    // Failure to do so can cause the exception to be rethrown at every recursive level and overflow the stack for x64 CLR processes
                    Exception exception = e.ErrorContext.Error;
                    bindingContext.ModelState.AddModelError(e.ErrorContext.Path, e.ErrorContext.Error);
                    e.ErrorContext.Handled = true;
                };
                jsonSerializer.Error += errorHandler;

                try
                {
                    bindingContext.Model = jsonSerializer.Deserialize(jsonReader, bindingContext.ModelType);
                }
                finally
                {
                    // Clean up the error handler in case CreateJsonSerializer() reuses a serializer
                    jsonSerializer.Error -= errorHandler;
                }
                return true;
            }
        }

        private static bool IsSupportedContentType(HttpRequest request)
        {
            string contentType = request.Headers["Content-Type"];
            return !string.IsNullOrEmpty(contentType) &&
                   contentType.Equals("application/json", StringComparison.OrdinalIgnoreCase);
        }
    }
}
