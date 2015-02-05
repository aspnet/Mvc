﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace MvcSample.Web.Controllers
{
    [Route("[controller]/[action]/{id}.{format?}")]
    public class FormatFilterController : Controller
    {
        public Product GetProduct(int id)
        {
            return new Product() { SampleInt = id };
        }

        [Produces("application/json", "text/json")]
        public Product ProducesMethod(int id)
        {
            return new Product() { SampleInt = id }; ;
        }
    }
}