// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc;

namespace ViewComponentWebSite
{
    public class HomeController : Controller
    {
        public ViewResult ViewWithAsyncComponents()
        {
            return new ViewResult();
        }

        public ViewResult ViewWithSyncComponents()
        {
            return new ViewResult();
        }

        public ViewResult ViewWithIntegerViewComponent()
        {
            return new ViewResult();
        }

        public ViewResult ViewComponentWithEnumerableModel()
        {
            var modelList = new List<SampleModel>()
            {
                new SampleModel { Prop1 = "Hello", Prop2 = "World" },
                new SampleModel { Prop1 = "Sample", Prop2 = "Test" },
            };
            
            return View(modelList.Where(a => a != null));
        }
    }
}