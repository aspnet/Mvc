﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace InlineConstraintSample.Web.Controllers
{
    [Route("book/[action]")]
    public class InlineConstraint_Isbn13Controller : Controller
    {
        [HttpGet("{isbnNumber:IsbnDigitScheme13}")]
        public string Index(string isbnNumber)
        {
            return "13 Digit ISBN Number " + isbnNumber;
        }
    }
}