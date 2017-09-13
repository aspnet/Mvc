﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


namespace Microsoft.AspNetCore.Mvc.ErrorDescription
{
    public interface IErrorDescriptorProvider
    {
        int Order { get; }

        void OnProvidersExecuting(ErrorDescriptionContext context);

        void OnProvidersExecuted(ErrorDescriptionContext context);
    }
}
