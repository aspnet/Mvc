// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    /// <summary>
    /// Return value of <see cref="IModelBindingContext.PushContext"/>. Should be disposed
    /// by caller when child binding context state should be popped off of 
    /// the <see cref="IModelBindingContext"/>.
    /// </summary>
    public struct ModelBindingContextDisposable : IDisposable
    {
        private readonly IModelBindingContext _context;

        /// <summary>
        /// Initializes the disposable for a pushed context.
        /// </summary>
        /// <param name="context"></param>
        public ModelBindingContextDisposable(IModelBindingContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Disposes the child binding context state by calling PopContext.
        /// </summary>
        public void Dispose()
        {
            _context.PopContext();
        }
    }
}
