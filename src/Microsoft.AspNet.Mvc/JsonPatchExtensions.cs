// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.JsonPatch;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Extensions for <see cref="JsonPatchDocument{T}"/>
    /// </summary>
    public static class JsonPatchExtensions
    {
        /// <summary>
        /// Applies json patch operations on object and logs errors in <see cref="ModelStateDictionary"/>.
        /// </summary>
        /// <param name="patchDoc">The <see cref="JsonPatchDocument{T}"/>.</param>
        /// <param name="objectToApplyTo">The entity on which <see cref="JsonPatchDocument{T}"/> is applied.</param>
        /// <param name="modelState">The <see cref="ModelStateDictionary"/> to add errors.</param>
        public static void ApplyTo<T>(
            [NotNull] this JsonPatchDocument<T> patchDoc,
            [NotNull] T objectToApplyTo,
            [NotNull] ModelStateDictionary modelState) where T : class
        {
            var modelStateError = new ModelStateError(modelState, patchDoc.GetType().Name);
            patchDoc.ApplyTo(objectToApplyTo, modelStateError.AddErrorMessage);
        }
    }
}