// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.Internal.DecisionTree
{
    public class DecisionTreeNode<T, U>
    {
        public List<T> Matches { get; set; }

        public List<DecisionCriterion<T, U>> Criteria { get; set; }
    }
}