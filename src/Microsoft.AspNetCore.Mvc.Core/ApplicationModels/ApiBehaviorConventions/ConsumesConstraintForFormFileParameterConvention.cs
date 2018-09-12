// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    /// <summary>
    /// An <see cref="IControllerModelConvention"/> that adds a <see cref="ConsumesAttribute"/> with <c>multipart/form-data</c> 
    /// to controllers containing form file (<see cref="BindingSource.FormFile"/>) parameters.
    /// </summary>
    public class ConsumesConstraintForFormFileParameterConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            if (!ShouldApply(controller))
            {
                return;
            }

            foreach (var action in controller.Actions)
            {
                AddMultipartFormDataConsumesAttribute(action);
            }
        }

        protected virtual bool ShouldApply(ControllerModel controller) => true;

        // Internal for unit testing
        internal void AddMultipartFormDataConsumesAttribute(ActionModel controller)
        {
            // Add a ConsumesAttribute if the request does not explicitly specify one.
            if (controller.Filters.OfType<IConsumesActionConstraint>().Any())
            {
                return;
            }

            foreach (var parameter in controller.Parameters)
            {
                var bindingSource = parameter.BindingInfo?.BindingSource;
                if (bindingSource == BindingSource.FormFile)
                {
                    // If an controller accepts files, it must accept multipart/form-data.
                    controller.Filters.Add(new ConsumesAttribute("multipart/form-data"));
                    return;
                }
            }
        }
    }
}
