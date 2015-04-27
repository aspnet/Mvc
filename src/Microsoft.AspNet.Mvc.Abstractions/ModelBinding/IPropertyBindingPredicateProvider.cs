// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// Provides a predicate which can determines which model properties should be bound by model binding.
    /// </summary>
    public interface IPropertyBindingPredicateProvider
    {
        /// <summary>
        /// Gets a predicate which can determines which model properties should be bound by model binding.
        /// </summary>
        Func<ModelBindingContext, string, bool> PropertyFilter { get; }
    }
}
