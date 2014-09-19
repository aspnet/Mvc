using System;
using Microsoft.AspNet.Mvc;

namespace UrlHelperWebSite.Controllers
{
    public class SimplePocoController
    {
        private readonly IUrlHelper _urlHelper;

        public SimplePocoController(IUrlHelper urlHelper)
	    {
            _urlHelper = urlHelper;
	    }

        public IActionResult Index()
        {
            return new ViewResult();
        }

        public string UrlContent()
        {
            return _urlHelper.Content("~/bootstrap.min.css");
        }
    }
}