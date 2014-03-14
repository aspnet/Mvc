﻿using Microsoft.AspNet.Mvc;
using MvcSample.Web.Filters;
using MvcSample.Web.Models;

namespace MvcSample.Web
{
    [ServiceFilter(typeof(PassThroughAttribute), Order = 1)]
    [ServiceFilter(typeof(PassThroughAttribute))]
    [PassThrough(Order = 0)]
    [PassThrough(Order = 2)]
    [InspectResultPage]
    [UserNameProvider(Order = -1)]
    public class FiltersController : Controller
    {
        private readonly User _user = new User() { Name = "User Name", Address = "Home Address" };

        // TODO: Add a real filter here
        [ServiceFilter(typeof(PassThroughAttribute))]
        [AgeEnhancer]
        public IActionResult Index(int age, string userName)
        {
            if (!string.IsNullOrEmpty(userName))
            {
                _user.Name = userName;
            }

            _user.Age = age;

            return View("MyView", _user);
        }
    }   
}