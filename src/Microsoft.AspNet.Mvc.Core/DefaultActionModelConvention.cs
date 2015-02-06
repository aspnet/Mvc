// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.ApplicationModels;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// The class used by <see cref="ApplicationModelConventionExtensions"/> to add a
    /// <see cref="IActionModelConvention"/> to all the actions in the application.
    /// </summary>
    public class DefaultActionModelConvention : IApplicationModelConvention
    {
        private IActionModelConvention _actionModelConvention;

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultActionModelConvention"/>.
        /// </summary>
        /// <param name="actionModelConvention">The action convention to be applied on all actions
        /// in the application.</param>
        public DefaultActionModelConvention([NotNull] IActionModelConvention actionModelConvention)
        {
            _actionModelConvention = actionModelConvention;
        }

        /// <inheritdoc />
        public void Apply([NotNull] ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                foreach (var action in controller.Actions)
                {
                    _actionModelConvention.Apply(action);
                }
            }
        }
    }
}