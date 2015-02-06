// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.ApplicationModels;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// The class used by <see cref="ApplicationModelConventionExtensions"/> to add a
    /// <see cref="IControllerModelConvention"/> to all the controllers in the application.
    /// </summary>
    public class DefaultControllerModelConvention : IApplicationModelConvention
    {
        private IControllerModelConvention _controllerModelConvention;

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultControllerModelConvention"/>.
        /// </summary>
        /// <param name="controllerConvention">The controller convention to be applied on all controllers
        /// in the application.</param>
        public DefaultControllerModelConvention([NotNull] IControllerModelConvention controllerConvention)
        {
            _controllerModelConvention = controllerConvention;
        }

        /// <inheritdoc />
        public void Apply([NotNull] ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                _controllerModelConvention.Apply(controller);
            }
        }
    }
}