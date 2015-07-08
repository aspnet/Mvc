// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.JsonPatch.Operations;

namespace Microsoft.AspNet.JsonPatch.Exceptions
{
    public class JsonPatchException : Exception 
    {
        public Operation FailedOperation { get; private set; }
        public object AffectedObject { get; private set; }
 

        public JsonPatchException()
        {

        }

        public JsonPatchException(JsonPatchError jsonPatchError, Exception innerException)
            : base(jsonPatchError.ErrorMessage, innerException)
        {
            FailedOperation = jsonPatchError.Operation;
            AffectedObject = jsonPatchError.AffectedObject;
        }

        public JsonPatchException(JsonPatchError jsonPatchError)
          : this(jsonPatchError, null)          
        {
        } 

        public JsonPatchException(string message, Exception innerException)
            : base (message, innerException)
        {
           
        }
    }
}