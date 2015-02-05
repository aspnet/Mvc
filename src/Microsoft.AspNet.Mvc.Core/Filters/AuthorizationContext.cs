// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc
{
    public class AuthorizationContext : FilterContext
    {
        public AuthorizationContext(
            [NotNull] ActionContext actionContext,
            [NotNull] IList<IFilter> filters)
            : base(actionContext, filters)
        {
        }

        public virtual IActionResult Result { get; set; }
    }
}
