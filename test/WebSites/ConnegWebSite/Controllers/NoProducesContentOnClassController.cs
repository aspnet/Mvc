﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace ConnegWebsite
{
    public class NoProducesContentOnClassController : ProducesContentBaseController
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var result = context.Result as ObjectResult;
            if (result != null)
            {
                result.Formatters.Add(new CustomFormatter("application/custom_NoProducesContentOnClassController_Action"));
            }

            base.OnActionExecuted(context);
        }

        [Produces("application/custom_NoProducesContentOnClassController_Action")]
        public override string ReturnClassName()
        {
            // should be written using the formatter provided by this action and not the base action.
            return "NoProducesContentOnClassController";
        }
    }
}