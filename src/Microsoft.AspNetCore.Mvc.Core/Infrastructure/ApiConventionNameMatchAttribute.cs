// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Mvc.Infrastructure
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class ApiConventionNameMatchAttribute : Attribute
    {
        public ApiConventionNameMatchAttribute(ApiConventionNameMatchBehavior matchBehavior)
        {
            MatchBehavior = matchBehavior;
        }

        public ApiConventionNameMatchBehavior MatchBehavior { get; }
    }
}
