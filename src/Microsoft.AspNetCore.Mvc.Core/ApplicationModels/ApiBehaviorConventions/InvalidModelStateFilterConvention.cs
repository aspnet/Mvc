// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resources = Microsoft.AspNetCore.Mvc.Core.Resources;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    /// <summary>
    /// A <see cref="IActionModelConvention"/> that adds a <see cref="ModelStateInvalidFilter"/>
    /// to <see cref="ActionModel"/>.
    /// </summary>
    public class InvalidModelStateFilterConvention : IActionModelConvention
    {
        private readonly ApiBehaviorOptions _apiBehaviorOptions;
        private readonly ModelStateInvalidFilter _modelStateInvalidFilter;

        public InvalidModelStateFilterConvention(
            IOptions<ApiBehaviorOptions> apiBehaviorOptions,
            ILoggerFactory loggerFactory)
        {
            if (apiBehaviorOptions == null)
            {
                throw new ArgumentNullException(nameof(apiBehaviorOptions));
            }

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _apiBehaviorOptions = apiBehaviorOptions.Value;

            if (!_apiBehaviorOptions.SuppressModelStateInvalidFilter && _apiBehaviorOptions.InvalidModelStateResponseFactory == null)
            {
                throw new ArgumentException(Resources.FormatPropertyOfTypeCannotBeNull(
                    typeof(ApiBehaviorOptions),
                    nameof(ApiBehaviorOptions.InvalidModelStateResponseFactory)));
            }

            _modelStateInvalidFilter = new ModelStateInvalidFilter(
                apiBehaviorOptions.Value,
                loggerFactory.CreateLogger<ModelStateInvalidFilter>());
        }

        public void Apply(ActionModel action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (ShouldApply(action))
            {
                action.Filters.Add(_modelStateInvalidFilter);
            }
        }

        protected virtual bool ShouldApply(ActionModel actionModel) => 
            !_apiBehaviorOptions.SuppressModelStateInvalidFilter;
    }
}
