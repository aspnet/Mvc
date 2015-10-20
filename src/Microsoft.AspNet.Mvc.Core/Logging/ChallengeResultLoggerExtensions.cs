using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    public static class ChallengeResultLoggerExtenstions
    {
        private static Action<ILogger, string, Exception> _challengeResultExecuted;

        static ChallengeResultLoggerExtenstions()
        {
            _challengeResultExecuted = LoggerMessage.Define<string>(LogLevel.Information, 1, "ChallengeResult for action {ActionName} executed.");
        }

        public static void ChallengeResultExecuted(this ILogger logger, ActionContext context)
        {
            var actionName = context.ActionDescriptor.DisplayName;
            _challengeResultExecuted(logger, actionName, null);
        }
    }
}
