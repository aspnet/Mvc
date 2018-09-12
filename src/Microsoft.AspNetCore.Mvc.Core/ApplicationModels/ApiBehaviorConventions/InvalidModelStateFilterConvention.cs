// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    /// <summary>
    /// An <see cref="IControllerModelConvention"/> that adds a <see cref="IFilterMetadata"/>
    /// to <see cref="ActionModel"/> that responds to invalid <see cref="ActionContext.ModelState"/>
    /// </summary>
    public class InvalidModelStateFilterConvention : IControllerModelConvention
    {
        private readonly ModelStateInvalidFilterFactory _filterFactory = new ModelStateInvalidFilterFactory();

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
                action.Filters.Add(_filterFactory);
            }
        }

        protected virtual bool ShouldApply(ControllerModel controller) => true;
    }
}
