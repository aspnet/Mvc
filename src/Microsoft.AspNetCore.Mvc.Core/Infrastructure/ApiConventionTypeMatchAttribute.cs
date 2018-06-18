// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Mvc.Infrastructure
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class ApiConventionTypeMatchAttribute : Attribute
    {
        public ApiConventionTypeMatchAttribute(ApiConventionTypeMatchBehavior matchBehavior)
        {
            MatchBehavior = matchBehavior;
        }

        public ApiConventionTypeMatchBehavior MatchBehavior { get; }
    }
}
