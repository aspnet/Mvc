﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc;

namespace TagHelperSample.Web
{
    public class TagHelperPrecompilation : RazorPreCompileModule
    {
        public TagHelperPrecompilation(IServiceProvider provider)
            : base(provider)
        {
        }
    }
}