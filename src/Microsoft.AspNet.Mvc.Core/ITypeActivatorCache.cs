// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Encapsulates information that creates a <typeparamref name="T"/> instance.
    /// </summary>
    public interface ITypeActivatorCache
    {
        /// <summary>
        /// Creates an instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> instance that retrieves services from the
        /// service collection.</param>
        /// <param name="optionType">The <see cref="Type"/> of the <typeparamref name="T"/> to create.</param>
        T CreateInstance<T>(IServiceProvider serviceProvider, Type optionType);
    }
}