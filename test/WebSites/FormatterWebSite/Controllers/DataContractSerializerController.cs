// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ActionResults;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.Mvc.Formatters;

namespace FormatterWebSite
{
    /// <summary>
    /// Summary description for DataContractSerializerController
    /// </summary>
    public class DataContractSerializerController : Controller
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var result = context.Result as ObjectResult;
            if (result != null)
            {
                result.Formatters.Add(new XmlSerializerOutputFormatter());
                result.Formatters.Add(new XmlDataContractSerializerOutputFormatter());
            }

            base.OnActionExecuted(context);
        }

        [HttpPost]
        public Person GetPerson(string name)
        {
            // The XmlSerializer should skip and the
            // DataContractSerializer should pick up this output.
            return new Person(name);
        }
    }
}