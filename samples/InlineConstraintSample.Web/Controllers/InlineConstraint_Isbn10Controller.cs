// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace InlineConstraintSample.Web.Controllers
{
    public class InlineConstraint_Isbn10Controller : Controller
    {
        public string Index(string isbnNumber)
        {
            return "10 Digit ISBN Number " + isbnNumber;
        }
    }
}