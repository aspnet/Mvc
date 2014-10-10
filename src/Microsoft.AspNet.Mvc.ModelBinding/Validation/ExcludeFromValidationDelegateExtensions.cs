// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// Extensions for <see cref="MvcOptions.ExcludedValidationTypesPredicates"/>.
    /// </summary>
    public static class ExcludeFromValidationDelegateExtensions
    {
        /// <summary>
        /// Adds a delegate to the specified <paramref name="list" /> that excludes all the specified
        /// <paramref name="type" />'s children from validation.
        /// </summary>
        /// <param name="list"><see cref="IList{T}"/> of <see cref="ExcludeFromValidationDelegate"/>.</param>
        /// <param name="type"><see cref="Type"/> which should be excluded from validation.</param>
        public static void Add(this IList<ExcludeFromValidationDelegate> list, Type type)
        {
            list.Add(t => t.IsAssignableFrom(type));
        }
    }
}