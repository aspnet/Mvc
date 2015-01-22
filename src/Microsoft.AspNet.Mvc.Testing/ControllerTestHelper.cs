// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using Microsoft.AspNet.Http.Interfaces.Security;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.Runtime;

namespace Microsoft.AspNet.Mvc.Testing
{
    public static class ControllerTestHelper
    {
        public static Controller Initialize(Controller controller, ControllerTestHelperContext helperContext)
        {
            helperContext = helperContext ?? new ControllerTestHelperContext();

            if (helperContext.RequestAborted)
            {
                helperContext.HttpContext.Abort();
            }

            helperContext.HttpContext.GetFeature<IHttpAuthenticationFeature>().Handler
                = helperContext?.ResponseAuthenticationHandler;

            helperContext.HttpContext.RequestServices =
                helperContext.RequestServices.BuildServiceProvider();

            // TODO: Is there any more services to pre-populate? 
            helperContext.ApplicationServices.AddSingleton<IApplicationEnvironment>(n => helperContext.ApplicationEnvironment);
            helperContext.HttpContext.ApplicationServices = helperContext.ApplicationServices.BuildServiceProvider();

            var userIdentity = new MockUserIdentity() { UserContext = helperContext.User };
            // TODO: Need to setup user identity here based on the helperContext.User
            helperContext.HttpContext.User = new ClaimsPrincipal(userIdentity);

            controller.ActionContext = new ActionContext(
                helperContext.HttpContext,
                helperContext.RouteData,
                new ActionDescriptor());

            controller.Url = new MockUrlHelper()
            {
                OnAction = helperContext.Url.OnAction
            };

            return controller;
        }
    }
}