// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Localization
{
    /// <summary>
    /// A service that provides localized strings for views. Keys are interpreted relative to the path of the executing
    /// view.
    /// </summary>
    public interface IViewLocalizer : IHtmlLocalizer
    {
    }
}