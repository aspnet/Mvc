// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Mvc.Formatters.Xml
{
    /// <summary>
    /// Creates an <see cref="IWrapperProvider"/> for the type <see cref="Problem"/>.
    /// </summary>
    public class ProblemWrapperProviderFactory : IWrapperProviderFactory
    {
        /// <summary>
        /// Creates an instance of <see cref="ProblemWrapperProviderFactory"/> if the provided
        /// <paramref name="context"/>'s <see cref="WrapperProviderContext.DeclaredType"/> is
        /// <see cref="Problem"/>.
        /// </summary>
        /// <param name="context">The <see cref="WrapperProviderContext"/>.</param>
        /// <returns>
        /// An instance of <see cref="ProblemWrapperProviderFactory"/> if the provided <paramref name="context"/>'s
        /// <see cref="WrapperProviderContext.DeclaredType"/> is
        /// <see cref="Problem"/>; otherwise <c>null</c>.
        /// </returns>
        public IWrapperProvider GetProvider(WrapperProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (typeof(Problem).IsAssignableFrom(context.DeclaredType))
            {
                return new ProblemWrapperProvider();
            }

            return null;
        }
    }
}