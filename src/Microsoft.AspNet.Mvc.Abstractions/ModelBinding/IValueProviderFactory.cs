// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public interface IValueProviderFactory
    {
        /// <summary>
        /// Get a value provider with values from the given <paramref name="requestContext"/>.
        /// </summary>
        /// <param name="context">ValueProviderFactoryContext that value provider will populate from</param>
        /// <returns>a value provider instance or null</returns>
        Task<IValueProvider> GetValueProviderAsync([NotNull] ValueProviderFactoryContext context);
    }
}
