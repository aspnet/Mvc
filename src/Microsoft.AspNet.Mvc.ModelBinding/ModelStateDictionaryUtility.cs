// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// This class is a utility for ModelStateDictionary
    /// </summary>
    public static class ModelStateDictionaryUtility
    {
        /// <summary>
        /// Clears ModelStateDictionary.
        /// </summary>
        /// <param name="modelState">The <see cref="ModelStateDictionary"/>.</param>
        public static void ClearModelStateDictionary(ModelStateDictionary modelState)
        {
            foreach(var entry in modelState)
            {
                entry.Value.Errors.Clear();
                entry.Value.ValidationState = ModelValidationState.Unvalidated;
            }
        }
    }
}