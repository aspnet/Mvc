﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// A marker interface for <see cref="IActionResult"/> types which will perform
    /// a redirect, and need to have temp data saved.
    /// </summary>
    public interface IRedirectResult : IActionResult
    {
    }
}
