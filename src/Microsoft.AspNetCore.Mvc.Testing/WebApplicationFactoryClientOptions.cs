// Copyright (c) .NET  Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;

namespace Microsoft.AspNetCore.Mvc.Testing
{
    /// <summary>
    /// The default options to use to when creating
    /// <see cref="HttpClient"/> instances by calling
    /// <see cref="WebApplicationFactory{TEntryPoint}.CreateClient(WebApplicationFactoryClientOptions)"/>.
    /// </summary>
    public class WebApplicationFactoryClientOptions
    {
        /// <summary>
        /// Gets or sets the base address of <see cref="HttpClient"/> instances created by calling
        /// <see cref="WebApplicationFactory{TEntryPoint}.CreateClient(WebApplicationFactoryClientOptions)"/>.
        /// </summary>
        public Uri BaseAddress { get; set; } = new Uri("http://localhost");

        /// <summary>
        /// Gets or sets whether or not <see cref="HttpClient"/> instances created by calling
        /// <see cref="WebApplicationFactory{TEntryPoint}.CreateClient(WebApplicationFactoryClientOptions)"/>
        /// should automatically follow redirect responses.
        /// </summary>
        public bool AllowAutoRedirect { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum number of redirect responses that <see cref="HttpClient"/> instances
        /// created by calling <see cref="WebApplicationFactory{TEntryPoint}.CreateClient(WebApplicationFactoryClientOptions)"/>
        /// should follow.
        /// </summary>
        public int MaxAutomaticRedirections { get; set; } = RedirectHandler.DefaultMaxRedirects;

        /// <summary>
        /// Gets or sets whether <see cref="HttpClient"/> instances created by calling 
        /// <see cref="WebApplicationFactory{TEntryPoint}.CreateClient(WebApplicationFactoryClientOptions)"/>
        /// should handle cookies.
        /// </summary>
        public bool HandleCookies { get; set; } = true;

        internal DelegatingHandler[] CreateHandlers()
        {
            return CreateHandlersCore().ToArray();

            IEnumerable<DelegatingHandler> CreateHandlersCore()
            {
                if (AllowAutoRedirect)
                {
                    yield return new RedirectHandler(MaxAutomaticRedirections);
                }
                if (HandleCookies)
                {
                    yield return new CookieContainerHandler();
                }
            }
        }
    }
}
