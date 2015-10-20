// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Mvc
{
    internal static class ChallengeResultLoggerExtenstions
    {
        private static Action<ILogger, string, Exception> _resulExecuted;

        static ChallengeResultLoggerExtenstions()
        {
            _resulExecuted = LoggerMessage.Define<string>(LogLevel.Information, 122,
                "ChallengeResult for action {ActionName} executed.");
        }

        public static void ChallengeResultExecuted(this ILogger logger, ActionContext context,
            Exception exception = null)
        {
            var actionName = context.ActionDescriptor.DisplayName;
            _resulExecuted(logger, actionName, exception);
        }
    }

    public class ChallengeResult : ActionResult
    {
        public ChallengeResult()
            : this(new string[] { })
        {
        }

        public ChallengeResult(string authenticationScheme)
            : this(new[] { authenticationScheme })
        {
        }

        public ChallengeResult(IList<string> authenticationSchemes)
            : this(authenticationSchemes, properties: null)
        {
        }

        public ChallengeResult(AuthenticationProperties properties)
            : this(new string[] { }, properties)
        {
        }

        public ChallengeResult(string authenticationScheme, AuthenticationProperties properties)
            : this(new[] { authenticationScheme }, properties)
        {
        }

        public ChallengeResult(IList<string> authenticationSchemes, AuthenticationProperties properties)
        {
            AuthenticationSchemes = authenticationSchemes;
            Properties = properties;
        }

        public IList<string> AuthenticationSchemes { get; set; }

        public AuthenticationProperties Properties { get; set; }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var logFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<ChallengeResult>();
            var auth = context.HttpContext.Authentication;
            if (AuthenticationSchemes.Count > 0)
            {
                foreach (var scheme in AuthenticationSchemes)
                {
                    await auth.ChallengeAsync(scheme, Properties);
                }
            }
            else
            {
                await auth.ChallengeAsync(Properties);
            }


            logger.ChallengeResultExecuted(context);
        }
    }
}
