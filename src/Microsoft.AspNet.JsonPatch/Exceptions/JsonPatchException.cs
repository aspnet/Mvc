// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.JsonPatch.Operations;

namespace Microsoft.AspNet.JsonPatch.Exceptions
{
    public class JsonPatchException : JsonPatchExceptionBase //where TModel : class
    {
        public Operation FailedOperation { get; private set; }
        public new object AffectedObject { get; private set; }

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

        public JsonPatchException(JsonPatchError jsonPatchError)
        {
            FailedOperation = jsonPatchError.Operation;
            _message = jsonPatchError.ErrorMessage;
            AffectedObject = jsonPatchError.AffectedObject;
        }

        public JsonPatchException(JsonPatchError jsonPatchError, Exception innerException)
            : this(jsonPatchError)
        {
            InnerException = innerException;
        }

        public JsonPatchException(string message, Exception innerException)
        {
            _message = message;
            InnerException = innerException;
        }
    }
}