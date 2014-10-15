// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace FiltersWebSite
{
    public class FakeRandomNumberProvider : IActionFilter
    {
        private FakeService _fake;

        public FakeRandomNumberProvider(FakeService fake)
        {
            _fake = fake;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            context.ActionArguments["randomNumber"] = _fake.RandomNumber;
        }
    }
}