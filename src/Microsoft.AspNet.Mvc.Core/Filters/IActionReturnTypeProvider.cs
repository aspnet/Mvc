// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Represents a type which can specify the entity type returned by an action.
    /// </summary>
    public interface IActionReturnTypeProvider
    {
        Type Type { get; set; }
    }
}