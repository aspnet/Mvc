using System.Text;

namespace Microsoft.AspNet.Mvc.Internal
{
    public static class EncodingHelper
    {
        public static readonly Encoding UTF8EncodingWithoutBOM
            = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    }
}