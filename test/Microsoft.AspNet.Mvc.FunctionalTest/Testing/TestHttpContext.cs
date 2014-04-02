using System;
using System.Collections.Generic;
using Microsoft.AspNet.Abstractions;

namespace Microsoft.AspNet.Mvc.FunctionalTest.Testing
{
    public class TestHttpContext : HttpContext
    {
        IDictionary<object, object> _items;
        HttpRequest _request;
        HttpResponse _response;

        public TestHttpContext(TestHttpRequest request)
        {
            _request = request;
            _response = new TestHttpResponse(this);
            _items = new Dictionary<object, object>();
        }

        public override IServiceProvider ApplicationServices { get; set; }

        public override IServiceProvider RequestServices { get; set; }

        public override IDictionary<object, object> Items
        {
            get { return _items; }
        }

        public override HttpRequest Request
        {
            get { return _request; }
        }

        public override HttpResponse Response
        {
            get { return _response; }
        }

        public override object GetFeature(Type type)
        {
            throw new NotImplementedException();
        }

        public override void SetFeature(Type type, object instance)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
        }
    }
}
