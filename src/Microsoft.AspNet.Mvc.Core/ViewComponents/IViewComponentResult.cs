// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Specifies the contract for the result of a <see cref="ViewComponent"/>.
    /// </summary>
    public interface IViewComponentResult
    {
        /// <summary>
        /// Locates and renders a view component specified by <paramref name="context"/>.
        /// </summary>
        /// <param name="context">The <see cref="ViewComponentContext"/> for the current component execution.</param>
        void Execute([NotNull] ViewComponentContext context);

        /// <summary>
        /// Asynchronously locates and renders a view component specified by <paramref name="context"/>.
        /// </summary>
        /// <param name="context">The <see cref="ViewComponentContext"/> for the current component execution.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous execution.</returns>
        Task ExecuteAsync([NotNull] ViewComponentContext context);
    }
}
