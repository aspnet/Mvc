// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.Internal.DecisionTree
{
    public class DecisionCriterion<T, U>
    {
        public string Key { get; set; }

        public Dictionary<U, DecisionTreeNode<T, U>> Branches { get; set; }

        public DecisionTreeNode<T, U> Fallback { get; set; }
    }
}