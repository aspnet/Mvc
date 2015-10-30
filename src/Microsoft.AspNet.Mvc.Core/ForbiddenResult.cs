﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Mvc.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// An <see cref="ActionResult"/> that on execution issues a 403 forbidden response
    /// if the authentication challenge is unacceptable.
    /// </summary>
    public class ForbiddenResult : ActionResult
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ForbiddenResult"/>.
        /// </summary>
        public ForbiddenResult()
            : this(new string[] { })
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ForbiddenResult"/> with the
        /// specified authentication scheme.
        /// </summary>
        public ForbiddenResult(string authenticationScheme)
            : this(new[] { authenticationScheme })
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ForbiddenResult"/> with the
        /// specified authentication schemes.
        /// </summary>
        public ForbiddenResult(IList<string> authenticationSchemes)
            : this(authenticationSchemes, properties: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ForbiddenResult"/> with the
        /// specified <paramref name="properties"/>.
        /// </summary>
        public ForbiddenResult(AuthenticationProperties properties)
            : this(new string[] { }, properties)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ForbiddenResult"/> with the
        /// specified authentication scheme and <paramref name="properties"/>.
        /// </summary>
        public ForbiddenResult(string authenticationScheme, AuthenticationProperties properties)
            : this(new[] { authenticationScheme }, properties)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ForbiddenResult"/> with the
        /// specified authentication schemes and <paramref name="properties"/>.
        /// </summary>
        public ForbiddenResult(IList<string> authenticationSchemes, AuthenticationProperties properties)
        {
            AuthenticationSchemes = authenticationSchemes;
            Properties = properties;
        }

        /// <summary>
        /// The list of authentication components that should handle the authentication challenge
        /// invoked by this instance of <see cref="ForbiddenResult"/>.
        /// </summary>
        public IList<string> AuthenticationSchemes { get; set; }

        /// <summary>
        /// <see cref="AuthenticationProperties"/> used to perform authentication.
        /// </summary>
        public AuthenticationProperties Properties { get; set; }

        /// <inheritdoc />
        public override async Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<ForbiddenResult>();

            var authentication = context.HttpContext.Authentication;

            if (AuthenticationSchemes != null && AuthenticationSchemes.Count > 0)
            {
                for (var i = 0; i < AuthenticationSchemes.Count; i++)
                {
                    await authentication.ForbidAsync(AuthenticationSchemes[i], Properties);
                }
            }
            else
            {
                await authentication.ForbidAsync(Properties);
            }

            logger.ForbiddenResultExecuting(AuthenticationSchemes);
        }
    }
}
