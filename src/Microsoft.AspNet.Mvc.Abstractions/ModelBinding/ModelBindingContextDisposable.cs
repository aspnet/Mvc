// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public struct ModelBindingContextDisposable : IDisposable
    {
        private readonly IModelBindingContext _context;
        public ModelBindingContextDisposable(IModelBindingContext context)
        {
            _context = context;
        }
        public void Dispose()
        {
            _context.PopContext();
        }
    }
}
