// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc
{
    public interface IAsyncAuthorizationFilter : IFilterMetadata
    {
        Task OnAuthorizationAsync([NotNull] AuthorizationContext context);
    }
}
