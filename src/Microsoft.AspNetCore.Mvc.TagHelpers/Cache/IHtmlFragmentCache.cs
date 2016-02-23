// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;

namespace Microsoft.AspNetCore.Mvc.TagHelpers.Cache
{
    public interface IHtmlFragmentCache
    {
        Task<IHtmlContent> SetAsync(string key, Func<Task<IHtmlContent>> renderContent, HtmlFragmentCacheContext context);
        Task<IHtmlContent> GetValueAsync(string key, HtmlFragmentCacheContext context);
    }
}
