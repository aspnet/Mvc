// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// An <see cref="ObjectResult"/> that when executed will produce a <see cref="StatusCodes.Status403Forbidden"/> response.
    /// </summary>
    [DefaultStatusCode(DefaultStatusCode)]
    public class ForbidObjectResult : ObjectResult
    {
        private const int DefaultStatusCode = StatusCodes.Status403Forbidden;

        /// <summary>
        /// Initializes a new instance of <see cref="ForbidObjectResult"/>.
        /// </summary>
        /// <param name="value">Response payload.</param>
        public ForbidObjectResult(object value)
            : this(value, Array.Empty<string>())
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ForbidObjectResult"/> with the
        /// specified authentication scheme.
        /// </summary>
        /// <param name="authenticationScheme">The authentication scheme to challenge.</param>
        /// <param name="value">Response payload.</param>
        public ForbidObjectResult(object value, string authenticationScheme)
            : this(value, new[] { authenticationScheme })
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ForbidObjectResult"/> with the
        /// specified authentication schemes.
        /// </summary>
        /// <param name="authenticationSchemes">The authentication schemes to challenge.</param>
        /// <param name="value">Response payload.</param>
        public ForbidObjectResult(object value, IList<string> authenticationSchemes)
            : this(value, authenticationSchemes, properties: null)
        {
        }
        /// <summary>
        /// Initializes a new instance of <see cref="ForbidObjectResult"/> with the
        /// specified <paramref name="properties"/>.
        /// </summary>
        /// <param name="properties"><see cref="AuthenticationProperties"/> used to perform the authentication
        /// challenge.</param>
        /// <param name="value">Response payload.</param>
        public ForbidObjectResult(object value, AuthenticationProperties properties)
            : this(value, Array.Empty<string>(), properties)
        {
        }
        /// <summary>
        /// Initializes a new instance of <see cref="ForbidObjectResult"/> with the
        /// specified authentication scheme and <paramref name="properties"/>.
        /// </summary>
        /// <param name="authenticationScheme">The authentication schemes to challenge.</param>
        /// <param name="properties"><see cref="AuthenticationProperties"/> used to perform the authentication
        /// challenge.</param>
        /// <param name="value">Response payload.</param>
        public ForbidObjectResult(object value, string authenticationScheme, AuthenticationProperties properties)
            : this(value, new[] { authenticationScheme }, properties)
        {
        }
        /// <summary>
        /// Initializes a new instance of <see cref="ForbidObjectResult"/> with the
        /// specified authentication schemes and <paramref name="properties"/>.
        /// </summary>
        /// <param name="authenticationSchemes">The authentication scheme to challenge.</param>
        /// <param name="properties"><see cref="AuthenticationProperties"/> used to perform the authentication
        /// challenge.</param>
        /// <param name="value">Response payload.</param>
        public ForbidObjectResult(object value, IList<string> authenticationSchemes, AuthenticationProperties properties)
            : base(value)
        {
            AuthenticationSchemes = authenticationSchemes;
            Properties = properties;
        }

        /// <summary>
        /// Gets or sets the authentication schemes that are challenged.
        /// </summary>
        public IList<string> AuthenticationSchemes { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="AuthenticationProperties"/> used to perform the authentication challenge.
        /// </summary>
        public AuthenticationProperties Properties { get; set; }

        /// <inheritdoc />
        public override async Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var executor = context.HttpContext.RequestServices.GetRequiredService<IActionResultExecutor<ForbidObjectResult>>();
            var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<ForbidResult>();

            logger.ForbidResultExecuting(AuthenticationSchemes);

            if (AuthenticationSchemes != null && AuthenticationSchemes.Count > 0)
            {
                for (var i = 0; i < AuthenticationSchemes.Count; i++)
                {
                    await context.HttpContext.ForbidAsync(AuthenticationSchemes[i], Properties);
                }
            }
            else
            {
                await context.HttpContext.ForbidAsync(Properties);
            }

            await executor.ExecuteAsync(context, this);
        }
    }
}