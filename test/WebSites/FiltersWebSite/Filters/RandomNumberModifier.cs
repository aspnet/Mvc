﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace FiltersWebSite
{
    public class RandomNumberModifier : IActionFilter
    {
        private RandomNumberService _random;

        public RandomNumberModifier(RandomNumberService random)
        {
            _random = random;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var paramterValue = (int)context.ActionArguments["randomNumber"];
            context.ActionArguments["randomNumber"] = paramterValue + _random.GetRandomNumber();
        }
    }
}