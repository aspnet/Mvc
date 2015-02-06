// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Mvc.ApplicationModels;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Contains the extension methods for <see cref="MvcOptions.Conventions"/>.
    /// </summary>
    public static class ApplicationModelConventionExtensions
    {
        /// <summary>
        /// Adds a <see cref="IControllerModelConvention"/> to all the controllers in the application.
        /// </summary>
        /// <param name="conventions">The list of <see cref="IApplicationModelConvention">
        /// in <see cref="MvcOptions"/>.</param>
        /// <param name="controllerModelConvention">The <see cref="IControllerModelConvention"/> which needs to be
        /// added.</param>
        public static void Add(
            [NotNull] this List<IApplicationModelConvention> conventions,
            [NotNull] IControllerModelConvention controllerModelConvention)
        {
            conventions.Add(new DefaultControllerModelConvention(controllerModelConvention));
        }

        /// <summary>
        /// Adds a <see cref="IActionModelConvention"/> to all the actions in the application.
        /// </summary>
        /// <param name="conventions">The list of <see cref="IApplicationModelConvention">
        /// in <see cref="MvcOptions"/>.</param>
        /// <param name="actionModelConvention">The <see cref="IActionModelConvention"/> which needs to be
        /// added.</param>
        public static void Add(
            this List<IApplicationModelConvention> conventions,
            IActionModelConvention actionModelConvention)
        {
            conventions.Add(new DefaultActionModelConvention(actionModelConvention));
        }
    }
}