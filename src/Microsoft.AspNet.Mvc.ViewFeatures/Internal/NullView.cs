// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Mvc.ViewEngines;

namespace Microsoft.AspNet.Mvc.ViewFeatures.Internal
{
    public class NullView : IView
    {
        public static readonly NullView Instance = new NullView();

        public string Path => string.Empty;

        public Task RenderAsync(ViewContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return Task.FromResult(0);
        }
    }
}
