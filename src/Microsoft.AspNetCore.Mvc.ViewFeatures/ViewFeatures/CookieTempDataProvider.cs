﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Internal;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    /// <summary>
    /// Provides data from cookie to the current <see cref="ITempDataDictionary"/> object.
    /// </summary>
    public class CookieTempDataProvider : ITempDataProvider
    {
        public static readonly string CookieName = ".AspNetCore.Mvc.CookieTempDataProvider";
        private static readonly string Purpose = "Microsoft.AspNetCore.Mvc.CookieTempDataProviderToken.v1";
        private const byte TokenVersion = 0x01;
        private readonly IDataProtector _dataProtector;
        private readonly TempDataSerializer _tempDataSerializer;
        private readonly ChunkingCookieManager _chunkingCookieManager;
        private readonly IOptions<CookieTempDataProviderOptions> _options;

        public CookieTempDataProvider(IDataProtectionProvider dataProtectionProvider, IOptions<CookieTempDataProviderOptions> options)
        {
            _dataProtector = dataProtectionProvider.CreateProtector(Purpose);
            _tempDataSerializer = new TempDataSerializer();
            _chunkingCookieManager = new ChunkingCookieManager();
            _options = options;
        }

        public IDictionary<string, object> LoadTempData(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Request.Cookies.ContainsKey(CookieName))
            {
                var encodedValue = _chunkingCookieManager.GetRequestCookie(context, CookieName);
                if (!string.IsNullOrEmpty(encodedValue))
                {
                    var protectedData = Base64UrlTextEncoder.Decode(encodedValue);
                    var unprotectedData = _dataProtector.Unprotect(protectedData);
                    return _tempDataSerializer.Deserialize(unprotectedData);
                }
            }

            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var cookieOptions = new CookieOptions()
            {
                // Check for PathBase as it can empty in which case the clients would not send the cookie
                // in subsequent requests.
                Path = string.IsNullOrEmpty(_options.Value.Path) ? GetPathBase(context) : _options.Value.Path,
                Domain = string.IsNullOrEmpty(_options.Value.Domain) ? null : _options.Value.Domain,
                HttpOnly = true,
                Secure = context.Request.IsHttps,
            };

            var hasValues = (values != null && values.Count > 0);
            if (hasValues)
            {
                var bytes = _tempDataSerializer.Serialize(values);
                bytes = _dataProtector.Protect(bytes);
                var encodedValue = Base64UrlTextEncoder.Encode(bytes);
                _chunkingCookieManager.AppendResponseCookie(context, CookieName, encodedValue, cookieOptions);
            }
            else
            {
                _chunkingCookieManager.DeleteCookie(context, CookieName, cookieOptions);
            }
        }

        private string GetPathBase(HttpContext httpContext)
        {
            var pathBase = httpContext.Request.PathBase.ToString();
            if (string.IsNullOrEmpty(pathBase))
            {
                pathBase = "/";
            }
            return pathBase;
        }
    }
}
