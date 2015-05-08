using System.IO;
using System.Text;
using Microsoft.AspNet.Mvc.Internal;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Wraps the supplied <see cref="Stream"/> to prevent it from flushing (which can cause response to be sent
    /// in Chunked encoding) and disposing (as the hosting layers own the stream).
    /// Also wraps the supplied <see cref="Encoding"/> to prevent writing the preamble or BOM bytes to the response.
    /// </summary>
    public class HttpResponseStreamWriter : StreamWriter
    {
        private const int DefaultBufferSize = 1024;

        public HttpResponseStreamWriter(Stream stream, Encoding encoding)
            : this(stream, encoding, DefaultBufferSize)
        {
        }

        public HttpResponseStreamWriter(Stream stream, Encoding encoding, int bufferSize)
            : base(new NonDisposableStream(stream), new NonBomEncodingWrapper(encoding), bufferSize, leaveOpen: true)
        {
        }

        private class NonBomEncodingWrapper : Encoding
        {
            private readonly Encoding _originalEncoding;

            public NonBomEncodingWrapper(Encoding encoding)
            {
                _originalEncoding = encoding;
            }

            public override int GetByteCount(char[] chars, int index, int count)
            {
                return _originalEncoding.GetByteCount(chars, index, count);
            }

            public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
            {
                return _originalEncoding.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
            }

            public override int GetCharCount(byte[] bytes, int index, int count)
            {
                return _originalEncoding.GetCharCount(bytes, index, count);
            }

            public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
            {
                return _originalEncoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
            }

            public override int GetMaxByteCount(int charCount)
            {
                return _originalEncoding.GetMaxByteCount(charCount);
            }

            public override int GetMaxCharCount(int byteCount)
            {
                return _originalEncoding.GetMaxCharCount(byteCount);
            }

            public override byte[] GetPreamble()
            {
                // Returning a byte array of length zero, to indicate that a preamble is not required.
                // From: https://msdn.microsoft.com/en-us/library/system.text.encoding.getpreamble%28v=vs.110%29.aspx
                return new byte[] { };
            }
        }
    }
}
