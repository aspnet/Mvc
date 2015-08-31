using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc
{
    public static class TaskCache
    {
#if DNX451 || DNX452
        /// <summary>A task that's already been completed successfully.</summary>
        private static Task s_completedTask;
#endif

        /// <summary>Gets a task that's already been completed successfully.</summary>
        /// <remarks>May not always return the same instance.</remarks>        
        public static Task CompletedTask
        {
            get
            {
#if DNX451 || DNX452
                var completedTask = s_completedTask;
                if (completedTask == null)
                {
                    completedTask = new Task(() => { }, default(CancellationToken)); // benign initialization race condition
                    completedTask.Start();
                    s_completedTask = completedTask;
                }
                return completedTask;
#else
                return Task.CompletedTask;
#endif
            }
        }
    }

}
