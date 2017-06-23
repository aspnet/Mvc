﻿using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc.Testing
{
    /// <summary>
    /// Delegating handler for managing cookies on functional tests.
    /// </summary>
    public class CookieContainerHandler : DelegatingHandler
    {
        public CookieContainerHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        public CookieContainer Container { get; } = new CookieContainer();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var cookieHeader = Container.GetCookieHeader(request.RequestUri);
            request.Headers.Add("Cookie", cookieHeader);

            var response = await base.SendAsync(request, cancellationToken);

            if (response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
            {
                foreach (var header in setCookieHeaders)
                {
                    Container.SetCookies(response.RequestMessage.RequestUri, header);
                }
            }

            return response;
        }
    }
}