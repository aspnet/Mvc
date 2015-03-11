// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Cors.Core;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.WebUtilities;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// A filter which applies the given <see cref="CorsPolicy"/> and adds appropriate response headers.
    /// </summary>
    public class CorsAuthorizationFilter : ICorsAuthorizationFilter
    {
        private readonly CorsPolicy _corsPolicy;

        /// <summary>
        /// Creates a new instace of <see cref="CorsAuthorizationFilter"/>.
        /// </summary>
        /// <param name="corsPolicy">The <see cref="CorsPolicy"/> which needs to be applied.</param>
        public CorsAuthorizationFilter([NotNull] CorsPolicy corsPolicy)
        {
            _corsPolicy = corsPolicy;
        }

        /// <inheritdoc />
        public async Task OnAuthorizationAsync([NotNull] AuthorizationContext context)
        {
            // If this filter is not closest to the action, it is not applicable.
            if (!IsClosestToAction(context.ActionDescriptor))
            {
                return;
            }

            if (context.HttpContext.Request.Headers.ContainsKey(CorsConstants.Origin))
            {
                var policy = _corsPolicy;
                var corsService = context.HttpContext.RequestServices.GetRequiredService<ICorsService>();
                var result = corsService.EvaluatePolicy(context.HttpContext, policy);
                corsService.ApplyResult(result, context.HttpContext.Response);

                var accessControlRequestMethod = 
                        context.HttpContext.Request.Headers.Get(CorsConstants.AccessControlRequestMethod);
                if (string.Equals(
                        context.HttpContext.Request.Method,
                        CorsConstants.PreflightHttpMethod,
                        StringComparison.Ordinal) &&
                    accessControlRequestMethod != null)
                {
                    // If this was a preflight, there is no need to run anything else.
                    // Also the response is always 200 so that anyone after mvc can handle the pre filght request.
                    context.Result = new HttpStatusCodeResult(StatusCodes.Status200OK);
                    await Task.FromResult(true);
                }

                // Continue with other filters and action.
            }
        }

        private bool IsClosestToAction(ActionDescriptor actionDescriptor)
        {
            // If there are multiple ICorsAuthorizationFilter which are defined at the class and
            // at the action level, the one closest to the action overrides the others. 
            // Since filterdescriptor collection is ordered (the last filter is the one closest to the action),
            // we apply this constraint only if there is no ICorsAuthorizationFilter after this.
            return actionDescriptor.FilterDescriptors.Last(
                filter => filter.Filter is ICorsAuthorizationFilter).Filter == this;
        }
    }
}
