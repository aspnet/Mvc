// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Razor;
using Microsoft.Framework.OptionsModel;

namespace RazorCompilerCacheWebSite
{
    public class CustomCompilerCache : CompilerCache
    {
        public CustomCompilerCache(IPrecompiledViewsProvider viewsProvider,
                                   IOptions<RazorViewEngineOptions> optionsAccessor,
                                   CompilerCacheInitialiedService cacheInitializedService)
            : base(viewsProvider, optionsAccessor)
        {
            cacheInitializedService.Initialized = true;
        }
    }
}