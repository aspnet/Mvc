﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc
{
    public static class DefaultOrder
    {
        /// <summary>
        /// The default order for sorting is -1000. Other framework code
        /// the depends on order should be ordered between -1 to -1999.
        /// User code should order at bigger than 0 or smaller than -2000.
        /// </summary>
        public static readonly int DefaultFrameworkSortOrder = -1000;

        /// <summary>
        /// The default order for <see cref="CorsAuthorizationFilter"/>, <see cref="CorsAuthorizationFilterFactory"/>
        /// and <see cref="DisableCorsAuthorizationFilter"/>.
        /// </summary>
        public static readonly int DefaultCorsSortOrder = int.MaxValue - 100;
    }
}
