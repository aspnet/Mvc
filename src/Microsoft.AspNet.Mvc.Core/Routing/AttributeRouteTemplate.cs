﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
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
            if (left == null && right == null)
            {
                return null;
            }
            else if (left == null)
            {
                return right.TrimStart('~').Trim('/');
            }
            else if (right == null)
            {
                return left.TrimStart('~').Trim('/');
            }

            // If the right part starts with "~/" or "/" we don't
            // take into account the left part.
            if (right.StartsWith("~/", StringComparison.OrdinalIgnoreCase) ||
                right.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                return right.TrimStart('~').Trim('/');
            }

            // Neither is null, the left part might start with "~/" or "/",
            // the right part might end with "/".
            var trimmedLeft = left.TrimStart('~').Trim('/');
            var trimmedRight = right.TrimEnd('/');

            if (trimmedLeft == string.Empty)
            {
                return right;
            }
            else if (trimmedRight == string.Empty)
            {
                return trimmedLeft;
            }

            // Both templates contain some text.
            return trimmedLeft + '/' + trimmedRight;
        }
    }
}