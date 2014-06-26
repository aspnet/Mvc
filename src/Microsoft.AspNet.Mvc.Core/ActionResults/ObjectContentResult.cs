// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc
{
    public class ObjectContentResult : ActionResult
    {
        public object Value { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public ObjectContentResult(object value)
        {
            Value = value;
            StatusCode = HttpStatusCode.OK;
        }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            ActionResult result;
            var actionReturnString = Value as string;
            if (Value == null)
            {
                result = new HttpStatusCodeResult((int)StatusCode);
            }
            else if (actionReturnString != null)
            {
                result  = new ContentResult
                {
                    ContentType = "text/plain",
                    Content = actionReturnString,
                };
            }
            else
            {
                result = new JsonResult(Value);
            }

            await result.ExecuteResultAsync(context);
        }
    }


    public class ObjectContetResult<TVal> : ObjectContentResult
    {
        public ObjectContetResult(TVal value)
            : base(value)
        {
        }

        public new TVal Value
        {
            get
            {
                return (TVal)base.Value;
            }
            set
            {
                base.Value = value;
            }
        }

        public static implicit operator ObjectContetResult<TVal>(TVal value)
        {
            return new ObjectContetResult<TVal>(value);
        }

        public static implicit operator ObjectContetResult<TVal>(HttpStatusCode statusCode)
        {
            return new ObjectContetResult<TVal>(value: default(TVal))
            {
                StatusCode = statusCode
            };
        }
    }
}