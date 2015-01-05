// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;
using Microsoft.Framework.Logging;

namespace LoggingWebSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger _logger;

        public HomeController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create<HomeController>();
        }

        public IActionResult Index()
        {
            _logger.WriteInformation("A");

            using (_logger.BeginScope("Scope1"))
            {
                _logger.WriteInformation("B");

                using (_logger.BeginScope("Scope1.1"))
                {
                    _logger.WriteInformation("C");
                }

                _logger.WriteInformation("D");
            }

            _logger.WriteInformation("E");

            return View();
        }
    }
}