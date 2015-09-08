// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Internal;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public abstract class ExceptionFilterAttribute : Attribute, IAsyncExceptionFilter, IExceptionFilter, IOrderedFilter
    {
        public int Order { get; set; }

        public virtual Task OnExceptionAsync([NotNull] ExceptionContext context)
        {
            OnException(context);
            return TaskCache.CompletedTask;
        }

        public virtual void OnException([NotNull] ExceptionContext context)
        {
        }
    }
}
