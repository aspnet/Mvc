// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Abstractions;
using Microsoft.AspNet.Routing;

namespace Microsoft.AspNet.Mvc.Infrastructure
{
    public interface IActionSelector
    {
        ActionDescriptor SelectAsync(RouteContext context);
    }
}
