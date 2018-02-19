// Copyright (c) .NET  Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Mvc.Testing
{
    public class CookieContainerHandler : DelegatingHandler
    {
        public CookieContainerHandler()
            : this(new CookieContainer())
        {
        }

        public CookieContainerHandler(CookieContainer cookieContainer)
        {
            Container = cookieContainer;
        }

        public CookieContainer Container { get; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var cookieHeader = Container.GetCookieHeader(request.RequestUri);
            request.Headers.Add(HeaderNames.Cookie, cookieHeader);

            var response = await base.SendAsync(request, cancellationToken);

            if (response.Headers.TryGetValues(HeaderNames.SetCookie, out var setCookieHeaders))
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