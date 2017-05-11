// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Mvc.TagHelpers
{
    using System;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Razor.TagHelpers;
    using Microsoft.Extensions.Primitives;

    /// <summary>
    ///   Renders content based on authentication/authorization status/roles of user
    /// </summary>
    /// <example>
    ///   &lt;authorized&gt;You are logged in!&lt;/authorized&gt;
    /// </example>
    /// <example>
    ///   &lt;authorized is-inverted=&quot;true&quot;&gt;You are not logged in!&lt;/authorized&gt;
    /// </example>
    /// <example>
    ///   &lt;authorized roles="Developer,Admin" &gt;You are a developer or admin!&lt;/authorized&gt;
    /// </example>
    /// <example>
    ///   &lt;authorized roles="Developer,Admin" is-inverted=&quot;true&quot;&gt;You are not a developer nor an admin!&lt;/authorized&gt;
    /// </example>
    public class AuthorizedTagHelper : TagHelper
    {
        private static readonly char[] NameSeparator = { ',' };

        private const string RolesAttributeName = "roles";
        private const string IsInvertedAttributeName = "is-inverted";

        /// <summary>
        /// Creates a new <see cref="AuthorizedTagHelper"/>.
        /// </summary>
        /// <param name="httpContextAccessor">The <see cref="IHttpContextAccessor"/>.</param>
        public AuthorizedTagHelper(IHttpContextAccessor httpContextAccessor)
        {
            this.HttpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc />
        public override int Order => -1000;

        /// <summary>
        /// A comma separated list of roles for which the content should be rendered.
        /// </summary>
        /// <remarks>
        /// The specified role names are compared case insensitively IPrincipal.Identity.IsInRole(). Leaving Roles blank will render content for authorized users only. Specify RoleCheck to the AuthorizedRoleCheck.NotInRole to show content when user is not in role. Leaving Roles blank and specifying RoleCheck to the AuthorizedRoleCheck.NotInRole will show content only for non-authenticated users.
        /// </remarks>
        [HtmlAttributeName(RolesAttributeName)]
        public string Roles { get; set; }

        /// <summary>
        /// Boolean determining whether you must be in the role or not in the role.
        /// </summary>
        /// <remarks>
        /// Setting this to true will show content for unauthenticated users only (if roles are left blank) or only those users not in any of the roles specified under roles.
        /// </remarks>
        [HtmlAttributeName(IsInvertedAttributeName)]
        public bool IsInverted { get; set; }

        protected IHttpContextAccessor HttpContextAccessor { get; }

        /// <inheritdoc />
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            // Always strip the outer tag name as we never want <environment> to render
            output.TagName = null;

            var user = this.HttpContextAccessor.HttpContext.User;
            var isAuthenticated = user?.Identity?.IsAuthenticated ?? false;
            if (!isAuthenticated && !this.IsInverted)
            {
                // Not authenticated, suppress output
                output.SuppressOutput();
                return;
            }

            var roles = this.Roles ?? string.Empty;
            var tokenizer = new StringTokenizer(roles, NameSeparator);
            var hasMatch = false;
            var hasAnyRole = false;
            foreach (var item in tokenizer)
            {
                var roleSegment = item.Trim();
                if (roleSegment.HasValue && roleSegment.Length > 0)
                {
                    hasAnyRole = true;
                    var role = roleSegment.ToString();
                    if (user != null && user.IsInRole(role))
                    {
                        hasMatch = true;
                        break;
                    }
                }
            }

            var shouldSuppressOutput = this.IsInverted ? hasMatch || isAuthenticated && !hasAnyRole : !hasMatch && hasAnyRole;

            if (shouldSuppressOutput)
            {
                // This instance had at least one non-empty environment specified but none of these
                // environments matched the current environment. Suppress the output in this case.
                output.SuppressOutput();
            }
        }
    }
}
