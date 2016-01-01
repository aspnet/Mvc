// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BasicWebSite.Models;
using Microsoft.AspNet.Mvc;

namespace BasicWebSite.Controllers.ActionConstraints
{
    [Route("ConsumesAttribute_Company/[action]")]
    public class ConsumesAttribute_WithFallbackActionController : Controller
    {
        [Consumes("application/json")]
        public Product CreateProduct([FromBody] Product_Json jsonInput)
        {
            return jsonInput;
        }

        [Consumes("application/xml")]
        public Product CreateProduct([FromBody] Product_Xml xmlInput)
        {
            return xmlInput;
        }

        public Product CreateProduct([FromBody] Product_Text defaultInput)
        {
            return defaultInput;
        }
    }
}