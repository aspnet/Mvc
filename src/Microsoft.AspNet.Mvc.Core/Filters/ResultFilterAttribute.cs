﻿using System;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public abstract class ResultFilterAttribute : Attribute, IResultFilter, IAsyncResultFilter, IOrderedFilter
    {
        public int Order { get; set; }

        public virtual void OnResultExecuting([NotNull] ResultExecutingContext context)
        {
        }

        public virtual void OnResultExecuted([NotNull] ResultExecutedContext context)
        {
        }

        public virtual async Task OnResultExecutionAsync([NotNull] ResultExecutingContext context, [NotNull] ResultExecutionDelegate next)
        {
            OnResultExecuting(context);
            if (context.Result == null)
            {
                OnResultExecuted(await next());
            }
        }
    }
}
