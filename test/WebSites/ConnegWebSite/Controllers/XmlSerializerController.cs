﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace ConnegWebsite
{
    public class XmlSerializerController : Controller
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var result = context.Result as ObjectResult;
            if (result != null)
            {
                result.Formatters.Add(new XmlSerializerOutputFormatter());
            }

            base.OnActionExecuted(context);
        }

        [HttpPost]
        public DummyClass GetDummyClass(int sampleInput)
        {
            return new DummyClass { SampleInt = sampleInput };
        }
    }
}