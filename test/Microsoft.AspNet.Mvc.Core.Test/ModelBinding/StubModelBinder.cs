﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding;
using System.Diagnostics;

namespace Microsoft.AspNet.Mvc.ModelBinding.Test
{
    public class StubModelBinder : IModelBinder
    {
        private readonly Func<IModelBindingContext, Task> _callback;

        public StubModelBinder()
        {
            _callback = context => Internal.TaskCache.CompletedTask;
        }

        public StubModelBinder(ModelBindingResult? result)
        {
            _callback = context =>
            {
                context.Result = result;
                return Internal.TaskCache.CompletedTask;
            };
        }

        public StubModelBinder(Action<IModelBindingContext> callback)
        {
            _callback = context =>
            {
                callback(context);
                return Internal.TaskCache.CompletedTask;
            };
        }

        public StubModelBinder(Func<IModelBindingContext, Task<ModelBindingResult?>> callback)
        {
            _callback = async context =>
            {
                var result = await callback.Invoke(context);
                context.Result = result;
            };
        }

        public int BindModelCount { get; set; }

        public IModelBinder Object => this;

        public virtual async Task BindModelAsync(IModelBindingContext bindingContext)
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
