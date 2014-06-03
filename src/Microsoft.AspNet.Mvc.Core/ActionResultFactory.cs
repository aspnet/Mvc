// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc.Core;

namespace Microsoft.AspNet.Mvc
{
    public class ActionResultFactory : IActionResultFactory
    {
        private readonly IActionResultHelper _result;

        public ActionResultFactory(IActionResultHelper result)
        {
            _result = result;
        }

        public IActionResult CreateActionResult([NotNull] Type declaredReturnType,
                                                object actionReturnValue,
                                                ActionContext actionContext)
        {
            // optimize common path
            var actionResult = actionReturnValue as IActionResult;

            if (actionResult != null)
            {
                return actionResult;
            }

            if (typeof(IActionResult).IsAssignableFrom(declaredReturnType) && actionReturnValue == null)
            {
                throw new InvalidOperationException(
                    Resources.FormatActionResult_ActionReturnValueCannotBeNull(declaredReturnType));
            }

            if (declaredReturnType == typeof(void) || actionReturnValue == null)
            {
                return new NoContentResult();
            }

            var actionReturnString = actionReturnValue as string;

            if (actionReturnString != null)
            {
                return new ContentResult
                {
                    ContentType = "text/plain",
                    Content = actionReturnString,
                };
            }

            return _result.Json(actionReturnValue);
        }
    }
}
