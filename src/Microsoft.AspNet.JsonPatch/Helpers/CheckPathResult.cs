// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.JsonPatch.Helpers
{
    internal class CheckPathResult
    {
        public bool IsCorrectlyFormedPath { get; set; }
        public string AdjustedPath { get; set; }

        public CheckPathResult(bool isCorrectlyFormedPath, string adjustedPath)
        {
            IsCorrectlyFormedPath = isCorrectlyFormedPath;
            AdjustedPath = adjustedPath;
        }
    }
}
