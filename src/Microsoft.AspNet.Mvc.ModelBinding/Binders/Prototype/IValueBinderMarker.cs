// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// Represents a binder marker which identifies a binder which is based off of a value provider.
    /// </summary>
    public interface IValueBinderMarker : IBinderMarker
    {
    }
}
