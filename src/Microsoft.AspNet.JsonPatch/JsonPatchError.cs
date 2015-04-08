// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.JsonPatch.Operations;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.JsonPatch
{
    /// <summary>
    /// Captures error message and the related entity and operation that caused it.
    /// </summary>
    public class JsonPatchError<T> where T : class
    {
        public JsonPatchError(
            [NotNull] T affectedObject,
            [NotNull] Operation<T> operation,
            [NotNull] string errorMessage)
        {
            AffectedObject = affectedObject;
            Operation = operation;
            ErrorMessage = errorMessage;
        }

        public T AffectedObject { get; private set; }

        public Operation<T> Operation { get; private set; }

        public string ErrorMessage { get; private set; }
    }
}