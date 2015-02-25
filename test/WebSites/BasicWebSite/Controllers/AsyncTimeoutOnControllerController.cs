// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

namespace BasicWebSite.Controllers
{
    [AsyncTimeout(1000)]
    public class AsyncTimeoutOnControllerController : Controller
    {
        private static List<string> TimeoutTriggerLog = new List<string>();

        public string ActionWithTimeoutAttribute(CancellationToken timeoutToken)
        {
            if (timeoutToken != CancellationToken.None)
            {
                return "CancellationToken is present";
            }

            return "CancellationToken is not present";
        }

        [AsyncTimeout(500)]
        public async Task<string> LongRunningAction(CancellationToken timeoutToken)
        {
            timeoutToken.Register((requestCorrelationId) =>
            {
                TimeoutTriggerLog.Add(requestCorrelationId.ToString());
            }, Request.Headers.Get("CorrelationId"));

            await Task.Delay(3 * 1000);

            return "Hello World!";
        }

        public List<string> TimeoutTriggerLogs()
        {
            return TimeoutTriggerLog;
        }
    }
}