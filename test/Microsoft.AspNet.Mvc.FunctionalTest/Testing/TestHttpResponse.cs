using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.Abstractions;
using Microsoft.AspNet.PipelineCore.Collections;

namespace Microsoft.AspNet.Mvc.FunctionalTest.Testing
{
    public class TestHttpResponse : HttpResponse
    {
        private HttpContext _context;
        private IDictionary<string, string[]> _rawHeaders;
        private StreamWriter _writer;
        public TestHttpResponse(HttpContext context)
        {
            _context = context;
            _rawHeaders = new Dictionary<string, string[]>();
            Body = new MemoryStream();
            _writer = new StreamWriter(Body);
            _writer.AutoFlush = true;
        }

        public override int StatusCode { get; set; }

        public override IHeaderDictionary Headers
        {
            get { return new HeaderDictionary(_rawHeaders); }
        }

        public override string ContentType
        {
            get
            {
                string[] contentTypeHeader;
                if (_rawHeaders.TryGetValue("Content-Type", out contentTypeHeader))
                {
                    return contentTypeHeader[0];
                }
                return null;
            }
            set
            {
                if (value == null)
                {
                    _rawHeaders.Remove("Content-Type");
                }
                else
                {
                    _rawHeaders["Content-Type"] = new string[] { value };
                }
            }
        }

        public override IResponseCookies Cookies
        {
            get { return new ResponseCookies(Headers); }
        }

        public override Stream Body { get; set; }

        public override long? ContentLength
        {
            get
            {
                return Body.Length;
            }
            set
            {
                if (value.HasValue)
                {
                    Body.SetLength(value.Value);
                }
            }
        }

        public override HttpContext HttpContext
        {
            get { return _context; }
        }

        public async override Task WriteAsync(string data)
        {
            await _writer.WriteAsync(data);
        }

        public override void OnSendingHeaders(Action<object> callback, object state)
        {
            throw new NotImplementedException();
        }

        public override void Redirect(string location)
        {
            throw new NotImplementedException();
        }
    }
}
