using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Abstractions;
using Microsoft.AspNet.PipelineCore.Collections;

namespace Microsoft.AspNet.Mvc.FunctionalTest.Testing
{
    public class TestHttpRequest : HttpRequest
    {
        private HttpContext _context;
        private ReadableStringCollection _query;
        private IDictionary<string, string[]> _rawCookies;
        private IDictionary<string, string[]> _rawHeaders;

        public TestHttpRequest(string method, string requestUri)
        {
            Uri uri = new Uri(requestUri);
            Host = HostString.FromUriComponent(requestUri);
            Method = method;
            Path = PathString.FromUriComponent(uri);
            PathBase = new PathString("");
            Protocol = "HTTP/1.1";
            Scheme = uri.Scheme;
            QueryString = QueryString.FromUriComponent(uri);
            Body = new MemoryStream(new byte[0], false);
            _rawCookies = new Dictionary<string, string[]>();
            _rawHeaders = new Dictionary<string, string[]>();
            _context = new TestHttpContext(this);
        }

        public override string Protocol { get; set; }

        public override string Method { get; set; }

        public override string Scheme { get; set; }

        public override HostString Host { get; set; }

        public override PathString PathBase { get; set; }

        public override PathString Path { get; set; }

        public override QueryString QueryString { get; set; }

        public override IReadableStringCollection Query
        {
            get
            {
                if (_query == null && QueryString.HasValue)
                {
                    var query = from kvp in QueryString.Value.Substring(1).Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                                let pair = kvp.Split(new char[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries)
                                select new KeyValuePair<string, string>(pair[0], pair[1]);

                    var qv = query.GroupBy(kvp => kvp.Key)
                        .ToDictionary(group => group.Key, group => group.Select(p => p.Value).ToArray());
                    _query = new ReadableStringCollection(qv);
                }
                else if (_query == null)
                {
                    _query = new ReadableStringCollection(new Dictionary<string, string[]>());
                }

                return _query;
            }
        }

        public override IHeaderDictionary Headers
        {
            get { return new HeaderDictionary(_rawHeaders); }
        }

        public override IReadableStringCollection Cookies
        {
            get { return new ReadableStringCollection(_rawCookies); }
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

        public override CancellationToken CallCanceled { get; set; }

        public override bool IsSecure
        {
            get { return Scheme.Equals("https", StringComparison.OrdinalIgnoreCase); }
        }

        public override Task<IReadableStringCollection> GetFormAsync()
        {
            throw new NotImplementedException();
        }
    }
}
