// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.JsonPatch.Helpers
{
    internal class CheckNumericEndResult
    {
        public bool HasNumericEnd { get; private set; }
        public int NumericEnd { get; private set; }

        public CheckNumericEndResult(bool hasNumericEnd, int? numericEnd)
        {
            HasNumericEnd = hasNumericEnd;
            if (hasNumericEnd)
            {
                NumericEnd = (int)numericEnd;
            }
        }

    }
}
