﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    public class RedirectToPageResultExecutor
    {
        private readonly ILogger _logger;
        private readonly IUrlHelperFactory _urlHelperFactory;

        public RedirectToPageResultExecutor(ILoggerFactory loggerFactory, IUrlHelperFactory urlHelperFactory)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            if (urlHelperFactory == null)
            {
                throw new ArgumentNullException(nameof(urlHelperFactory));
            }

            _logger = loggerFactory.CreateLogger<RedirectToRouteResult>();
            _urlHelperFactory = urlHelperFactory;
        }

        public void Execute(ActionContext context, RedirectToPageResult result)
        {
            var urlHelper = result.UrlHelper ?? _urlHelperFactory.GetUrlHelper(context);
            var destinationUrl = urlHelper.Page(
                result.PageName,
                result.RouteValues,
                result.Protocol,
                result.Host,
                fragment: result.Fragment);

            if (string.IsNullOrEmpty(destinationUrl))
            {
                throw new InvalidOperationException(Resources.FormatNoRoutesMatched(result.PageName));
            }

            _logger.RedirectToPageResultExecuting(result.PageName);
            context.HttpContext.Response.Redirect(destinationUrl, result.Permanent);
        }
    }
}