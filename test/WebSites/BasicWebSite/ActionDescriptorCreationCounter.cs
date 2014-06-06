using Microsoft.AspNet.Mvc;
using System;
using System.Threading;

namespace BasicWebSite
{
    public class ActionDescriptorCreationCounter : IActionDescriptorProvider
    {
        private long _callCount;

        public long CallCount
        {
            get
            {
                long callCount = Interlocked.Read(ref _callCount);

                return callCount;
            }
        }

        public static string CallStacks;

        public int Order
        {
            get
            {
                return ReflectedActionDescriptorProvider.DefaultOrder - 100;
            }
        }

        public void Invoke(ActionDescriptorProviderContext context, Action callNext)
        {
            callNext();

            if (context.Results.Count == 0)
            {
                throw new InvalidOperationException("No actions found!");
            }

            Interlocked.Increment(ref _callCount);
        }
    }
}