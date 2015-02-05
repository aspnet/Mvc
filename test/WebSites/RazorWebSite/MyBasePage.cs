﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Razor;

namespace RazorWebSite
{
    public abstract class MyBasePage<TModel> : RazorPage<TModel>
    {
        public override void WriteLiteral(object value)
        {
            base.WriteLiteral("WriteLiteral says:");
            base.WriteLiteral(value);
        }

        public override void Write(object value)
        {
            base.WriteLiteral("Write says:");
            base.Write(value);
        }
    }
}