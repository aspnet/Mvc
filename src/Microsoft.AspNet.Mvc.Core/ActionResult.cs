// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc
{
    public abstract class ActionResult : IActionResult
    {
        public virtual async Task ExecuteResultAsync(ActionContext context)
        {
            ExecuteResult(context);

            await Task.FromResult(true);
        }

        public virtual void ExecuteResult(ActionContext context)
        {
        }
    }
}