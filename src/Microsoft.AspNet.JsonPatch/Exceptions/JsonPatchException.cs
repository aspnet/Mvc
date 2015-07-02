// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.JsonPatch.Operations;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.JsonPatch.Exceptions
{
    public class JsonPatchException<TModel> : JsonPatchException where TModel : class
    {
        public Operation<TModel> FailedOperation { get; private set; }
        public new TModel AffectedObject { get; private set; }

        private string _message = string.Empty;

        public override string Message
        {
            get
            {
                return _message;
            }

        }

        public JsonPatchException()
        {

        }

        public JsonPatchException([NotNull] JsonPatchError<TModel> jsonPatchError)
        {
            FailedOperation = jsonPatchError.Operation;
            _message = jsonPatchError.ErrorMessage;
            AffectedObject = jsonPatchError.AffectedObject;
        }

        public JsonPatchException([NotNull] JsonPatchError<TModel> jsonPatchError, Exception innerException)
            : this(jsonPatchError)
        {
            InnerException = innerException;
        }
    }
}