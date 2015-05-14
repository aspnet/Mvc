// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;
using ModelBindingWebSite.Models;

namespace ModelBindingWebSite.Controllers
{
    [Route("Validation/[Action]")]
    public class ValidationController : Controller
    {
        [FromServices]
        public ITestService ControllerService { get; set; }

        public object SkipValidation(Resident resident)
        {
            return ModelState;
        }

        public object AvoidRecursive(SelfishPerson selfishPerson)
        {
            return ModelState;
        }

        public object DoNotValidateParameter([FromServices] ITestService service)
        {
            return ModelState;
        }
    }

    public class SelfishPerson
    {
        public string Name { get; set; }
        public SelfishPerson MySelf { get { return this; } }
    }
}