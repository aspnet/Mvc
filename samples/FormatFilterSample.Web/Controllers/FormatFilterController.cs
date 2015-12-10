// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace FormatFilterSample.Web
{
    [FormatFilter]
    public class FormatFilterController : Controller
    {
        public Product GetProduct(int id = 0)
        {
            return new Product() { SampleInt = id };
        }

        [Produces("application/custom", "application/json", "text/json")]
        public Product ProducesMethod(int id)
        {
            return new Product() { SampleInt = id };
        }
    }
}