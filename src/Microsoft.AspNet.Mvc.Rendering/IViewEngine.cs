﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public interface IViewEngine
    {
        // TODO: Relayer to allow this to be ActionContext. We probably need the common MVC assembly
        Task<ViewEngineResult> FindView(object actionContext, string viewName);
    }
}
