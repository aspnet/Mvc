// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;

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

                objectResult.Formatters.Add(new XmlDataContractSerializerOutputFormatter());
            }
        }
    }
}