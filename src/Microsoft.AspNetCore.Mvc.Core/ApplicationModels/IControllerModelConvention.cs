// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    /// <summary>
    /// Allows customization of the <see cref="ControllerModel"/>.
    /// </summary>
    /// <remarks>
    /// To use this interface, create an <see cref="System.Attribute"/> class which implements the interface and
    /// place it on a controller class.
    ///
    /// <see cref="IControllerModelConvention"/> customizations run after
    /// <see cref="IApplicationModelConvention"/> customizations and before
    /// <see cref="IActionModelConvention"/> customizations.
    /// </remarks>
    public interface IControllerModelConvention
    {
        /// <summary>
        /// Called to apply the convention to the <see cref="ControllerModel"/>.
        /// </summary>
        /// <param name="controller">The <see cref="ControllerModel"/>.</param>
        void Apply(ControllerModel controller);
    }
}
