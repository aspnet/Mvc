﻿using Microsoft.AspNet.Mvc;
using System;

namespace RoutingWebSite
{
    public class BanksController : Controller
    {
        private readonly TestResponseGenerator _generator;

        public BanksController(TestResponseGenerator generator)
	    {
            _generator = generator;
	    }

        [HttpGet("Banks/[action]/{id}")]
        [HttpGet("Bank/[action]/{id}")]
        public ActionResult Get(int id)
        {
            return _generator.Generate(
                Url.Action(),
                Url.RouteUrl(new { }));
        }
    }
}