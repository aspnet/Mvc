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

        public string Get(int id)
        {
            return "value:" + id;
        }
    }
}