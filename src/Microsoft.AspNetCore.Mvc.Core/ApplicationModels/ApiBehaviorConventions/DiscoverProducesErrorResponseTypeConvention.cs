// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    /// <summary>
    /// A <see cref="IActionModelConvention"/> that discovers and sets metadata inferred from <see cref="ProducesErrorResponseTypeAttribute"/>
    /// to be consumed by ApiExplorer.
    /// </summary>
    public class DiscoverProducesErrorResponseTypeConvention : IActionModelConvention
    {
        private readonly ProducesErrorResponseTypeAttribute DefaultErrorType = new ProducesErrorResponseTypeAttribute(typeof(ProblemDetails));
        private readonly ApiBehaviorOptions _apiBehaviorOptions;

        public DiscoverProducesErrorResponseTypeConvention(IOptions<ApiBehaviorOptions> apiBehaviorOptions)
        {
            _apiBehaviorOptions = apiBehaviorOptions?.Value ?? throw new ArgumentNullException(nameof(apiBehaviorOptions));
        }

        public void Apply(ActionModel action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (ShouldApply(action))
            {
                DiscoverErrorResponseType(action);
            }
        }

        protected virtual bool ShouldApply(ActionModel action) => true;

        internal void DiscoverErrorResponseType(ActionModel actionModel)
        {
            var errorTypeAttribute =
                actionModel.Attributes.OfType<ProducesErrorResponseTypeAttribute>().FirstOrDefault() ??
                actionModel.Controller.Attributes.OfType<ProducesErrorResponseTypeAttribute>().FirstOrDefault() ??
                actionModel.Controller.ControllerType.Assembly.GetCustomAttribute<ProducesErrorResponseTypeAttribute>();

            if (!_apiBehaviorOptions.SuppressMapClientErrors)
            {
                // If ClientErrorFactory is being used and the application does not supply a error response type, assume ProblemDetails.
                errorTypeAttribute = errorTypeAttribute ?? DefaultErrorType;
            }

            if (errorTypeAttribute != null)
            {
                actionModel.Properties[typeof(ProducesErrorResponseTypeAttribute)] = errorTypeAttribute;
            }
        }
    }
}
