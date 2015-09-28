// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.Filters
{
    public class ResultExecutingContext : FilterContext
    {
        public ResultExecutingContext(
            [NotNull] ActionContext actionContext,
            [NotNull] IList<IFilterMetadata> filters,
            [NotNull] IActionResult result,
            object controller)
            : base(actionContext, filters)
        {
            Result = result;
            Controller = controller;
        }

        public virtual object Controller { get; }

        public virtual IActionResult Result { get; set; }

        public virtual bool Cancel { get; set; }
    }
}
