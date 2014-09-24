// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Razor.TagHelpers;

namespace Microsoft.AspNet.Mvc.Razor.Host.TagHelpers
{
    // TODO: Document this in: https://github.com/aspnet/Mvc/issues/1149
    public class MvcTagHelperTypeResolver : ITagHelperTypeResolver
    {
        public IEnumerable<Type> Resolve(string lookupText)
        {
            // TODO: Implement type resolving logic in: https://github.com/aspnet/Mvc/issues/1149
            return Enumerable.Empty<Type>();
        }
    }
}