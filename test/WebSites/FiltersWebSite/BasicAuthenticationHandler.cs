// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Http.Authentication;

namespace FiltersWebSite
{
    public class BasicAuthenticationHandler : AuthenticationHandler<BasicOptions>
    {
        protected override void ApplyResponseChallenge()
        {
        }

        protected override void ApplyResponseGrant()
        {
        }

        protected override AuthenticationTicket AuthenticateCore()
        {
            var principal = new ClaimsPrincipal();
            principal.AddIdentity(new ClaimsIdentity(
                new Claim[] {
                    new Claim("Permission", "CanViewPage"),
                    new Claim("Manager", "yes"),
                    new Claim(ClaimTypes.Role, "Administrator"),
                    new Claim(ClaimTypes.NameIdentifier, "John")
                },
                Options.AuthenticationScheme));
            return new AuthenticationTicket(principal, new AuthenticationProperties(), Options.AuthenticationScheme);
        }
    }
}
