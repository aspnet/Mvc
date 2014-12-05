// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace ModelBindingWebSite.Controllers
{
    [Route("Blog")]
    public class FromHeader_BlogController : Controller
    {
        // Echo back the header value
        [HttpGet("BindToStringParameter")]
        public object BindToStringParameter(int id, [FromHeader] string transactionId, string q)
        {
            if (transactionId == null)
            {
                System.Diagnostics.Debugger.Launch();
                System.Diagnostics.Debugger.Break();
            }

            return new Result() { HeaderValue = transactionId };
        }

        // Echo back the header values
        [HttpGet("BindToStringArrayParameter")]
        public object BindToStringArrayParameter(int id, [FromHeader] string[] transactionIds)
        {
            return new Result() { HeaderValues = transactionIds };
        }

        private class Result
        {
            public string HeaderValue { get; set; }

            public string[] HeaderValues { get; set; }

            public string[] ModelStateErrors { get; set; }
        }
    }
}