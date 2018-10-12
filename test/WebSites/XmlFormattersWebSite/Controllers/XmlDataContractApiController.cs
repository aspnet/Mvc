// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace XmlFormattersWebSite
{
    [SetupOutputFormatters]
    public class XmlDataContractApiController : XmlApiControllerBase
    {
        private class SetupOutputFormattersAttribute : ResultFilterAttribute
        {
            public override void OnResultExecuting(ResultExecutingContext context)
            {
                if (!(context.Result is ObjectResult objectResult))
                {
                    return;
                }

                // Both kinds of Xml serializers are configured for this application and use custom content-types to do formatter
                // selection. The globally configured formatters rely on custom content-type to perform conneg which does not play
                // well the ProblemDetails returning filters that defaults to using application/xml. We'll explicitly select the formatter for this controller.
                var mvcOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<MvcOptions>>();
                var xmlFormatter = mvcOptions.Value.OutputFormatters.OfType<XmlDataContractSerializerOutputFormatter>().First();

                objectResult.Formatters.Add(xmlFormatter);
            }
        }
    }
}