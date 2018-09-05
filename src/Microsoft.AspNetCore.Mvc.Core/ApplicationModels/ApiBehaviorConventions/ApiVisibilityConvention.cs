// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    /// <summary>
    /// A <see cref="IControllerModelConvention"/> that sets Api Explorer visibility.
    /// </summary>
    public class ApiVisibilityConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            if (ShouldApply(controller))
            {
                if (controller.ApiExplorer.IsVisible == null)
                {
                    // Enable ApiExplorer for the controller if it wasn't already explicitly configured.
                    controller.ApiExplorer.IsVisible = true;
                }
            }
        }

        protected virtual bool ShouldApply(ControllerModel controller) => true;
    }
}
