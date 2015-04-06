// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Adds errors to <see cref="ModelStateDictionary"/>.
    /// </summary>
    public class ModelStateError
    {
        public ModelStateDictionary ModelState { get; set; }

        public string Key { get; set; }

        public ModelStateError(ModelStateDictionary modelState, string key)
        {
            ModelState = modelState;
            Key = key;
        }

        public void AddErrorMessage(string errorMessage)
        {
            ModelState.TryAddModelError(Key, errorMessage);
        }
    }
}