// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.TagHelpers
{
    [HtmlTargetElement(Attributes = "asp-auth")]
    public class AuthorizationTagHelper : TagHelper
    {
        public AuthorizationTagHelper(
            IAuthorizationPolicyProvider policyProvider,
            IPolicyEvaluator policyEvaluator)
        {
            PolicyProvider = policyProvider;
            PolicyEvaluator = policyEvaluator;
        }

        public IAuthorizationPolicyProvider PolicyProvider { get; }
        public IPolicyEvaluator PolicyEvaluator { get; }

        [HtmlAttributeName("asp-auth")]
        public bool Authenticated { get; set; }

        [HtmlAttributeName("asp-user-id")]
        public string Id { get; set; }

        [HtmlAttributeName(DictionaryAttributePrefix = "asp-has-claim")]
        public IDictionary<string, bool> RequiredClaims { get; set; } = new Dictionary<string, bool>();

        [HtmlAttributeName(DictionaryAttributePrefix = "asp-claim")]
        public IDictionary<string, string> RequiredClaimValues { get; set; } = new Dictionary<string, string>();

        [HtmlAttributeName("asp-auth-policy")]
        public string PolicyName { get; set; }

        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var originalUser = ViewContext.HttpContext.User;
            try
            {
                var user = ViewContext.HttpContext.User;
                if (PolicyName != null)
                {
                    var policy = await PolicyProvider.GetPolicyAsync(PolicyName);
                    var policyResult = await PolicyEvaluator.AuthenticateAsync(policy, ViewContext.HttpContext);
                    if (!policyResult.Succeeded)
                    {
                        output.SuppressOutput();
                    }
                    else
                    {
                        user = policyResult.Principal;
                    }
                }

                if (!UserIsAllowed(user))
                {
                    output.SuppressOutput();
                }
            }
            finally
            {
                ViewContext.HttpContext.User = originalUser;
            }
        }

        private bool UserIsAllowed(ClaimsPrincipal user)
        {
            if (Authenticated ^ IsAuthenticated())
            {
                return false;
            }

            if (Id != null)
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier);
                if (userId == null || !string.Equals(Id, userId.Value))
                {
                    return false;
                }
            }

            foreach (var claim in RequiredClaims)
            {
                if (claim.Value ^ user.HasClaim(c => string.Equals(c.Type, claim.Key, StringComparison.Ordinal)))
                {
                    return false;
                }
            }

            foreach (var claim in RequiredClaimValues)
            {
                if (!user.HasClaim(c => string.Equals(c.Type, claim.Key, StringComparison.Ordinal) &&
                    string.Equals(c.Value, claim.Value, StringComparison.Ordinal)))
                {
                    return false;
                }
            }

            return true;

            bool IsAuthenticated()
            {
                foreach (var identity in user.Identities)
                {
                    if (identity.IsAuthenticated)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
