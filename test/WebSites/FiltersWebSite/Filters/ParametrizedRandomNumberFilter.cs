// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace FiltersWebSite
{
    public class ParametrizedRandomNumberFilter : ParametrizedFilterBase<ParametrizedRandomNumberAttribute> {
        private RandomNumberService _random;

        public ParametrizedRandomNumberFilter(RandomNumberService random) {
            _random = random;
        }

        public override void OnActionExecuting(ActionExecutingContext context, ParametrizedRandomNumberAttribute data) {
            context.ActionArguments["randomNumber"] = _random.GetRandomNumber(data.MinValue, data.MaxValue);
        }
    }
}