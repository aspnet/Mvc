// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AuthenticateAttribute :
        Attribute, IAsyncAuthenticationFilter, IAuthenticationFilter
    {
        private string _authTypes;
        private string[] _authTypesSplit;

        public AuthenticateAttribute() { }

        public AuthenticateAttribute(string authenticationTypes)
        {
            AuthenticationTypes = authenticationTypes;
        }

        /// <summary>
        /// Comma separated list of authentication types
        /// </summary>
        public string AuthenticationTypes {
            get { return _authTypes; }
            set
            {
                _authTypes = value;
                _authTypesSplit = string.IsNullOrWhiteSpace(_authTypes) ? null : _authTypes.Split(',');
            }
        }

        public virtual async Task OnAuthenticationAsync([NotNull] AuthenticationContext context)
        {
            // REVIEW: Do nothing if no auth types requested?
            if (_authTypesSplit == null)
            {
                return;
            }
            var results = await context.HttpContext.AuthenticateAsync(_authTypesSplit);
            context.HttpContext.User = new ClaimsPrincipal(results.Select(r => r.Identity));
        }

        public virtual void OnAuthentication([NotNull] AuthenticationContext context)
        {
            // REVIEW: Do nothing if no auth types requested?
            if (_authTypesSplit == null)
            {
                return;
            }
            var results = context.HttpContext.Authenticate(_authTypesSplit);
            context.HttpContext.User = new ClaimsPrincipal(results.Select(r => r.Identity));
        }
    }
}
