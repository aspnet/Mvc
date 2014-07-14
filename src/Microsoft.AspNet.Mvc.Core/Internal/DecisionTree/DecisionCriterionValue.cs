// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc
{
    public struct DecisionCriterionValue<U>
    {
        private readonly bool _isCatchAll;
        private readonly U _value;

        public DecisionCriterionValue(U value, bool isCatchAll)
        {
            _value = value;
            _isCatchAll = isCatchAll;
        }

        public bool IsCatchAll
        {
            get { return _isCatchAll; }
        }

        public U Value
        {
            get { return _value; }
        }
    }
}