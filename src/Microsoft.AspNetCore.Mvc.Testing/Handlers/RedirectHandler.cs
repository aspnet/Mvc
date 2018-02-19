// Copyright (c) .NET  Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc.Testing
{
    public class RedirectHandler : DelegatingHandler
    {
        public RedirectHandler()
            : this(maxRedirects: 7)
        {
        }

        public RedirectHandler(int maxRedirects)
        {
            MaxRedirects = maxRedirects;
        }

        public int MaxRedirects { get; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var remainingRedirects = MaxRedirects;

            var originalRequestContent = HasBody(request) ? await DuplicateRequestContent(request) : null;
            var response = await base.SendAsync(request, cancellationToken);
            while (IsRedirect(response) && remainingRedirects >= 0)
            {
                remainingRedirects--;
                var redirectRequest = GetRedirectRequest(response, originalRequestContent);
                originalRequestContent = HasBody(redirectRequest) ? await DuplicateRequestContent(redirectRequest) : null;
                response = await base.SendAsync(redirectRequest, cancellationToken);
            }

            return response;
        }

        private static bool HasBody(HttpRequestMessage request) =>
            request.Method == HttpMethod.Post || request.Method == HttpMethod.Put;

        private static async Task<HttpContent> DuplicateRequestContent(HttpRequestMessage request)
        {
            if (request.Content == null)
            {
                return null;
            }
            var originalRequestContent = request.Content;
            var (originalBody, copy) = await CopyBody(request);

            var contentCopy = new StreamContent(copy);
            request.Content = new StreamContent(originalBody);

            CopyContentHeaders(originalRequestContent, request.Content, contentCopy);

            return contentCopy;
        }

        private static void CopyContentHeaders(
            HttpContent originalRequestContent,
            HttpContent newRequestContent,
            HttpContent contentCopy)
        {
            foreach (var header in originalRequestContent.Headers)
            {
                contentCopy.Headers.Add(header.Key, header.Value);
                newRequestContent.Headers.Add(header.Key, header.Value);
            }
        }

        private static async Task<(Stream originalBody, Stream copy)> CopyBody(HttpRequestMessage request)
        {
            var originalBody = await request.Content.ReadAsStreamAsync();
            var bodyCopy = new MemoryStream();
            await originalBody.CopyToAsync(bodyCopy);
            bodyCopy.Seek(0, SeekOrigin.Begin);
            if (originalBody.CanSeek)
            {
                originalBody.Seek(0, SeekOrigin.Begin);
            }
            else
            {
                originalBody = new MemoryStream();
                await bodyCopy.CopyToAsync(originalBody);
                originalBody.Seek(0, SeekOrigin.Begin);
                bodyCopy.Seek(0, SeekOrigin.Begin);
            }

            return (originalBody, bodyCopy);
        }

        private static HttpRequestMessage GetRedirectRequest(
            HttpResponseMessage response,
            HttpContent originalContent)
        {
            var location = response.Headers.Location;
            if (!location.IsAbsoluteUri)
            {
                location = new Uri(
                    new Uri(response.RequestMessage.RequestUri.GetLeftPart(UriPartial.Authority)),
                    location);
            }

            var redirect = !ShouldKeepVerb(response) ?
                new HttpRequestMessage(HttpMethod.Get, location) :
                new HttpRequestMessage(response.RequestMessage.Method, location)
                {
                    Content = originalContent
                };

            foreach (var header in response.RequestMessage.Headers)
            {
                redirect.Headers.Add(header.Key, header.Value);
            }

            foreach (var property in response.RequestMessage.Properties)
            {
                redirect.Properties.Add(property.Key, property.Value);
            }

            return redirect;
        }

        private static bool ShouldKeepVerb(HttpResponseMessage response)
        {
            return response.StatusCode == HttpStatusCode.RedirectKeepVerb ||
                            (int)response.StatusCode == 308;
        }

        private bool IsRedirect(HttpResponseMessage response)
        {
            return response.StatusCode == HttpStatusCode.MovedPermanently ||
                response.StatusCode == HttpStatusCode.Redirect ||
                response.StatusCode == HttpStatusCode.TemporaryRedirect ||
                (int)response.StatusCode == 308;

        }
    }
}
