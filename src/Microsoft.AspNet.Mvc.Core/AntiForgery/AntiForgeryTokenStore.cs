// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Abstractions;

namespace Microsoft.AspNet.Mvc
{
    // Saves anti-XSRF tokens split between HttpRequest.Cookies and HttpRequest.Form
    internal sealed class AntiForgeryTokenStore : ITokenStore
    {
        private readonly IAntiForgeryConfig _config;
        private readonly IAntiForgeryTokenSerializer _serializer;

        internal AntiForgeryTokenStore(IAntiForgeryConfig config, IAntiForgeryTokenSerializer serializer)
        {
            _config = config;
            _serializer = serializer;
        }

        public AntiForgeryToken GetCookieToken(HttpContext httpContext)
        {
            var cookie = httpContext.Request.Cookies[_config.CookieName];
            if (String.IsNullOrEmpty(cookie))
            {
                // did not exist
                return null;
            }

            return _serializer.Deserialize(cookie);
        }

        public AntiForgeryToken GetFormToken(HttpContext httpContext)
        {
            // TODO: Add proper exception handling.
            string value = httpContext.Request.GetFormAsync().Result[_config.FormFieldName];
            if (String.IsNullOrEmpty(value))
            {
                // did not exist
                return null;
            }

            return _serializer.Deserialize(value);
        }

        public void SaveCookieToken(HttpContext httpContext, AntiForgeryToken token)
        {
            string serializedToken = _serializer.Serialize(token);
            CookieOptions options = new CookieOptions() { HttpOnly = true };

            // Note: don't use "newCookie.Secure = _config.RequireSSL;" since the default
            // value of newCookie.Secure is automatically populated from the <httpCookies>
            // config element.
            if (_config.RequireSSL)
            {
                options.Secure = true;
            }

            httpContext.Response.Cookies.Append(_config.CookieName, serializedToken, options);
        }
    }
}