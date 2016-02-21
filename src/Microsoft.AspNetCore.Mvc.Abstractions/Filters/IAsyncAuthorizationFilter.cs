// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc.Filters
{
    public interface IAsyncAuthorizationFilter : IFilterMetadata
    {
        Task OnAuthorizationAsync(AuthorizationFilterContext context);
    }
}
