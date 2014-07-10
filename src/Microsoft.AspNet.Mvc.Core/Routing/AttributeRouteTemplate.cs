// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.Routing
{
    /// <summary>
    /// Functionality supporting route templates for attribute routes.
    /// </summary>
    public static class AttributeRouteTemplate
    {
        /// <summary>
        /// Combines attribute routing templates.
        /// </summary>
        /// <param name="left">The left template.</param>
        /// <param name="right">The right template.</param>
        /// <returns>A combined template.</returns>
        public static string Combine(string left, string right)
        {
            var result = CombineCore(left, right);
            return result == null ? null : result.TrimStart('~').Trim('/');
        }

        private static string CombineCore(string left, string right)
        {
            if (left == null && right == null)
            {
                return null;
            }
            else if (left == null)
            {
                return right;
            }
            else if (right == null)
            {
                return left;
            }

            if (right.StartsWith("~/", StringComparison.OrdinalIgnoreCase) ||
                right.StartsWith("/", StringComparison.OrdinalIgnoreCase) ||
                left.Equals("~/", StringComparison.OrdinalIgnoreCase) ||
                left.Equals("/", StringComparison.OrdinalIgnoreCase))
            {
                return right;
            }

            if (left.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                return left + right;
            }

            // Both templates contain some text.
            return left + '/' + right;
        }
    }
}