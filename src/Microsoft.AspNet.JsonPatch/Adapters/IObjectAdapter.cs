// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.JsonPatch.Operations;

namespace Microsoft.AspNet.JsonPatch.Adapters
{
    /// <summary>
    /// Defines the operations that can be performed on a JSON patch document.
    /// </summary>  
    public interface IObjectAdapter    
    {
        void Add(Operation operation, dynamic objectToApplyTo);
        void Copy(Operation operation, dynamic objectToApplyTo);
        void Move(Operation operation, dynamic objectToApplyTo);
        void Remove(Operation operation, dynamic objectToApplyTo);
        void Replace(Operation operation, dynamic objectToApplyTo);
    }
}