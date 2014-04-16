﻿using Microsoft.AspNet.Abstractions;

namespace Microsoft.AspNet.Mvc
{
    // Provides an abstraction around how tokens are persisted and retrieved for a request
    internal interface ITokenStore
    {
        AntiForgeryToken GetCookieToken(HttpContext httpContext);
        AntiForgeryToken GetFormToken(HttpContext httpContext);
        void SaveCookieToken(HttpContext httpContext, AntiForgeryToken token);
    }
}