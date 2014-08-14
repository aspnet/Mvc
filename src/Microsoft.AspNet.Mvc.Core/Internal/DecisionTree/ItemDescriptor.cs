﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.Internal.DecisionTree
{
    public class ItemDescriptor<T, U>
    {
        public IDictionary<string, DecisionCriterionValue<U>> Criteria { get; set; }

        public int Index { get; set; }

        public T Item { get; set; }
    }
}