﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Internal;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Test
{
    public class StubModelBinder : IModelBinder
    {
        private readonly Func<ModelBindingContext, Task> _callback;

        public StubModelBinder()
        {
            _callback = context => TaskCache.CompletedTask;
        }

        public StubModelBinder(ModelBindingResult? result)
        {
            _callback = context =>
            {
                context.Result = result;
                return TaskCache.CompletedTask;
            };
        }

        public StubModelBinder(Action<ModelBindingContext> callback)
        {
            _callback = context =>
            {
                callback(context);
                return TaskCache.CompletedTask;
            };
        }

        public StubModelBinder(Func<ModelBindingContext, Task<ModelBindingResult?>> callback)
        {
            _callback = async context =>
            {
                var result = await callback.Invoke(context);
                context.Result = result;
            };
        }

        public int BindModelCount { get; set; }

        public IModelBinder Object => this;

        public virtual async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            BindModelCount += 1;

            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }
            Debug.Assert(bindingContext.Result == null);

            await _callback.Invoke(bindingContext);
        }
    }
}
