using System.IO;
using Microsoft.AspNet.Abstractions;

namespace Microsoft.AspNet.Mvc.FunctionalTest.Testing
{
    public static class HttpResponseExtensions
    {
        public static string ReadAsString([NotNull]this HttpResponse response)
        {
            return new StreamReader(response.Body).ReadToEnd();
        }
    }
}
