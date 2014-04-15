﻿using Microsoft.AspNet.Abstractions;
using System.Security.Principal;

namespace Microsoft.AspNet.Mvc
{
    // Provides an abstraction around something that can validate anti-XSRF tokens
    internal interface ITokenValidator
    {
        // Determines whether an existing cookie token is valid (well-formed).
        // If it is not, the caller must call GenerateCookieToken() before calling GenerateFormToken().
        bool IsCookieTokenValid(AntiForgeryToken cookieToken);

        // Validates a (cookie, form) token pair.
        void ValidateTokens(HttpContext httpContext, IIdentity identity, AntiForgeryToken cookieToken, AntiForgeryToken formToken);
    }
}