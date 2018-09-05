// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    /// <summary>
    /// An <see cref="IActionModelConvention"/> that adds a <see cref="IFilterMetadata"/>
    /// to <see cref="ActionModel"/> that transforms <see cref="IClientErrorActionResult"/>.
    /// </summary>
    public class ClientErrorResultFilterConvention : IActionModelConvention
    {
        private readonly ApiBehaviorOptions _apiBehaviorOptions;
        private readonly ClientErrorResultFilter _clientErrorResultFilter;

        public ClientErrorResultFilterConvention(
            IOptions<ApiBehaviorOptions> apiBehaviorOptions,
            IClientErrorFactory clientErrorFactory,
            ILoggerFactory loggerFactory)
        {
            if (apiBehaviorOptions == null)
            {
                throw new ArgumentNullException(nameof(apiBehaviorOptions));
            }

            if (clientErrorFactory == null)
            {
                throw new ArgumentNullException(nameof(clientErrorFactory));
            }

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _apiBehaviorOptions = apiBehaviorOptions.Value;

            _clientErrorResultFilter = new ClientErrorResultFilter(
                clientErrorFactory,
                loggerFactory.CreateLogger<ClientErrorResultFilter>());
        }

        public void Apply(ActionModel action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (ShouldApply(action))
            {
                action.Filters.Add(_clientErrorResultFilter);
            }
        }

        protected virtual bool ShouldApply(ActionModel action) =>
            !_apiBehaviorOptions.SuppressMapClientErrors;
    }
}
