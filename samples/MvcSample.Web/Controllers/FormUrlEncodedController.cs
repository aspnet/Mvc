// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNet.Mvc;

namespace MvcSample.Web.Controllers
{
    public class FormUrlEncodedController : Controller
    {
        [Route("[controller]")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("[controller]/[action]")]
        public bool IsValidPerson(Person person)
        {
            return (person != null)
                && (person.Name != null)
                && (person.Address.Street != null)
                && (person.Address.City != null)
                && (person.Address.State != null)
                && (person.Address.Street != null)
                && (person.Address.Pin != 0)
                && (person.PastJobs.Any(j => j.JobTitle != null))
                && (person.PastJobs.Any(j => j.EmployerName != null))
                && (person.PastJobs.Any(j => j.YearsOfExperience != 0));
        }
    }
}