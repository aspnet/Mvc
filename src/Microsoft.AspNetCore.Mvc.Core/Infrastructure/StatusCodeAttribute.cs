// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Mvc.Infrastructure
{
    /// <summary>
    /// Specifies the default status code associated with an <see cref="ActionResult"/>.
    /// </summary>
    /// <remarks>
    /// This attribute is informational only and does not have any runtime effects.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class StatusCodeAttribute : Attribute
    {
        public StatusCodeAttribute(int statusCode)
        {
            StatusCode = statusCode;
        }

        public int StatusCode { get; }
    }
}
