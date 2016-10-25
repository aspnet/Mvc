// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure
{
    public abstract class RazorProject
    {
        public static readonly string RazorExtension = ".cshtml";

        public abstract IEnumerable<RazorProjectItem> EnumerateItems(string path);
    }
}