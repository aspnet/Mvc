// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Provides a dictionary of action arguments.
    /// </summary>
    public interface IControllerActionArgumentBinder
    {
        /// <summary>
        /// Returns a dictionary of representing the parameter-argument name-value pairs,
        /// which can be used to invoke the action. Also binds properties explicitly marked properties on the 
        /// <paramref name="controller"/>.
        /// </summary>
        /// <param name="context">The action context assoicated with the current action.</param>
        /// <param name="bindingContext">The <see cref="ActionBindingContext"/>.</param>
        /// <param name="controller">The controller object which contains the action.</param>
        Task<IDictionary<string, object>> BindActionArgumentsAsync(
            [NotNull] ActionContext context, 
            [NotNull] ActionBindingContext bindingContext,
            [NotNull] object controller);
    }
}
