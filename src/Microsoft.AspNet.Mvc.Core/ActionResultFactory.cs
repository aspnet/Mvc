// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

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
