// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.JsonPatch.Helpers
{
    internal class ActualPathResult
    {
        public int NumericEnd { get; private set; }
        public string PathToProperty { get; set; }
        public bool AppendToList { get; set; }

        public ActualPathResult(
            int numericEnd,
            string pathToProperty,
            bool appendToList)
        {         
            NumericEnd = numericEnd;
            PathToProperty = pathToProperty;
            AppendToList = appendToList;
        }
    }
}
