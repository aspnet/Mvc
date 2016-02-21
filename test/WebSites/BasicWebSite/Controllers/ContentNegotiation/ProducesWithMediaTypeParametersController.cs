// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BasicWebSite.Formatters;
using BasicWebSite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BasicWebSite.Controllers.ContentNegotiation
{
    public class ProducesWithMediaTypeParametersController : Controller
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var result = context.Result as ObjectResult;

            if (result != null)
            {
                result.Formatters.Add(new VCardFormatter_V3());
                result.Formatters.Add(new VCardFormatter_V4());
            }
        }

        [Produces("text/vcard;VERSION=V3.0")]
        public Contact ContactInfoUsingV3Format()
        {
            return new Contact()
            {
                Name = "John Williams",
                Gender = GenderType.Male
            };
        }

        [Produces("text/vcard;VERSION=V4.0")]
        public Contact ContactInfoUsingV4Format()
        {
            return new Contact()
            {
                Name = "John Williams",
                Gender = GenderType.Male
            };
        }
    }
}