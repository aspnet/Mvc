// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc
{
    public class DecisionCriterionValueEqualityComparer<U> : IEqualityComparer<DecisionCriterionValue<U>>
    {
        public DecisionCriterionValueEqualityComparer(IEqualityComparer<U> innerComparer)
        {
            InnerComparer = innerComparer;
        }

        public IEqualityComparer<U> InnerComparer { get; private set; }

        public bool Equals(DecisionCriterionValue<U> x, DecisionCriterionValue<U> y)
        {
            return x.IsCatchAll == y.IsCatchAll || InnerComparer.Equals(x.Value, y.Value);
        }

        public int GetHashCode(DecisionCriterionValue<U> obj)
        {
            if (obj.IsCatchAll)
            {
                return 0;
            }
            else
            {
                return InnerComparer.GetHashCode(obj.Value);
            }
        }
    }
}