﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class RedirectToActionResultExecutor
    {
        private readonly ILogger _logger;
        private readonly IUrlHelperFactory _urlHelperFactory;

        public RedirectToActionResultExecutor(ILoggerFactory loggerFactory, IUrlHelperFactory urlHelperFactory)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            if (urlHelperFactory == null)
            {
                throw new ArgumentNullException(nameof(urlHelperFactory));
            }

            _logger = loggerFactory.CreateLogger<RedirectToActionResult>();
            _urlHelperFactory = urlHelperFactory;
        }

        public void Execute(ActionContext context, RedirectToActionResult result)
        {
            var urlHelper = result.UrlHelper ?? _urlHelperFactory.GetUrlHelper(context);

            var destinationUrl = urlHelper.Action(
                result.ActionName,
                result.ControllerName,
                result.RouteValues,
                protocol: null,
                host: null,
                fragment: result.Fragment);
            if (string.IsNullOrEmpty(destinationUrl))
            {
                throw new InvalidOperationException(Resources.NoRoutesMatched);
            }

            _logger.RedirectToActionResultExecuting(destinationUrl);
            if (result.PreserveMethod == false)
            {
                context.HttpContext.Response.Redirect(destinationUrl, result.Permanent);
            }

            else
            {
                if (result.Permanent)
                {
                    context.HttpContext.Response.StatusCode = 308;
                }
                else
                {
                    context.HttpContext.Response.StatusCode = 307;
                }

                context.HttpContext.Response.Headers[HeaderNames.Location] = destinationUrl;
            }
        }
    }
}