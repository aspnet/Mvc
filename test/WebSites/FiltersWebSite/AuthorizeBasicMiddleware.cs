// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Builder;
using Microsoft.Framework.WebEncoders;
using Microsoft.Framework.Logging;

namespace FiltersWebSite
{
    public class AuthorizeBasicMiddleware : AuthenticationMiddleware<BasicOptions>
    {
        public AuthorizeBasicMiddleware(
            RequestDelegate next, 
            ILoggerFactory loggerFactory,
            IUrlEncoder encoder,
            string authScheme) : 
                base(next, new BasicOptions { AuthenticationScheme = authScheme }, loggerFactory, 
                     encoder, configureOptions: null)
        {
        }

        protected override AuthenticationHandler<BasicOptions> CreateHandler()
        {
            return new BasicAuthenticationHandler();
        }
    }
}