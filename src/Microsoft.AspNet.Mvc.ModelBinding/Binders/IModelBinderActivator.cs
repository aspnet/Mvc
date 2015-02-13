// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// Encapsulates information that creates a modelbinder.
    /// </summary>
    public interface IModelBinderActivator
    {
        /// <summary>
        /// Creates an instance of modelbinder.
        /// </summary>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> instance that retrieves services from the
        /// service collection.</param>
        /// <param name="binderType">The <see cref="Type"/> modelbinder to create.</param>
        object CreateInstance(IServiceProvider provider, Type binderType);
    }
}