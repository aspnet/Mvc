// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Mvc.Performance
{
    public class HelperPerformanceBenchmark : ViewEngineBenchmarkBase
    {
        private Random _rand = new Random();

        public HelperPerformanceBenchmark() 
            : base(
                "~/Views/HelperTyped.cshtml",
                "~/Views/HelperDynamic.cshtml",
                "~/Views/HelperPartialSync.cshtml",
                "~/Views/HelperPartialAsync.cshtml",
                "~/Views/HelperExtensions.cshtml",
                "~/Views/HelperPartialTagHelper.cshtml",
                "~/Views/HelperSyntheticSync.cshtml",
                "~/Views/HelperSyntheticAsync.cshtml")
        {
        }

        protected override object Model => _rand.Next().ToString();
    }
}
