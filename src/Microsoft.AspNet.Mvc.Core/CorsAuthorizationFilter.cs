// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

            if (context.HttpContext.Request.IsCorsRequest())
            {
                var policy = _corsPolicy;
                var engine = context.HttpContext.RequestServices.GetRequiredService<ICorsEngine>();
                var result = engine.EvaluatePolicy(context.HttpContext, policy);
                if (result.IsValid)
                {
                    WriteCorsHeaders(context.HttpContext, result);
                }

                if (context.HttpContext.Request.IsPreflight())
                {
                    // If this was a preflight, there is no need to run anything else.
                    // Also the response is always 200 so that anyone after mvc can handle the pre filght request.
                    context.Result = new HttpStatusCodeResult(StatusCodes.Status200OK);
                    await Task.FromResult(true);
                }
                else if (!result.IsValid)
                {
                    // Short circuit. We do not run the action in this case.
                    context.Result = new HttpStatusCodeResult(StatusCodes.Status400BadRequest);
                }
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

        private static void WriteCorsHeaders(HttpContext context, CorsResult result)
        {
            foreach (var header in result.ToResponseHeaders())
            {
                context.Response.Headers.Set(header.Key, header.Value);
            }
        }
    }
}
