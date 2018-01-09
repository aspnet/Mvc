// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    internal static class FallbackRazorPagesConventions
    {
        private const string FallbackDirectoryName = "Shared";

        public static bool TryGetSupersedingPath(string viewEnginePath, out string supersedingPath)
        {
            supersedingPath = null;
            var fileSeparatorIndex = viewEnginePath.LastIndexOf('/');
            if (fileSeparatorIndex <= 0)
            {
                return false;
            }

            var directorySeparatorIndex = viewEnginePath.LastIndexOf('/', fileSeparatorIndex - 1);
            if (directorySeparatorIndex == -1)
            {
                return false;
            }

            var directoryNameIndex = directorySeparatorIndex + 1;
            var directoryNameLength = fileSeparatorIndex - directoryNameIndex;
            if (directoryNameLength != FallbackDirectoryName.Length)
            {
                // Ensure the directory is exactly as long as the word "Shared"
                return false;
            }

            if (string.Compare(FallbackDirectoryName, 0, viewEnginePath, directoryNameIndex, directoryNameLength, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return false;
            }

            // viewEnginePath = /Pages/Accounts/Admin/Shared/Profile.cshtml
            // supersedingPath = /Pages/Accounts/Admin/Profile.cshtml
            var builder = new InplaceStringBuilder(viewEnginePath.Length - FallbackDirectoryName.Length - 1);
            builder.Append(viewEnginePath, 0, directorySeparatorIndex);
            builder.Append(viewEnginePath, fileSeparatorIndex, viewEnginePath.Length - fileSeparatorIndex);

            supersedingPath = builder.ToString();
            return true;
        }

        /// <summary>
        /// Determines if <paramref name="fallbackRouteModel"/> is superseded by <paramref name="routeModel"/>.
        /// <paramref name="routeModel"/> is a <see cref=" PageRouteModel"/> that is already registered with <see cref="PageRouteModelProviderContext"/>.
        /// <para>
        /// <paramref name="fallbackRouteModel"/> is considered superseded by <paramref name="routeModel"/> if the two have identical routes determined by
        /// <see cref="PageRouteModel.PageName"/> and
        /// identical <see cref="PageRouteModel.RouteValues"/>.
        /// </para>
        /// </summary>
        public static bool IsSuperseded(PageRouteModel fallbackRouteModel, PageRouteModel routeModel)
        {
            if (ReferenceEquals(fallbackRouteModel, routeModel))
            {
                // A model cannot supersede itself.
                return false;
            }

            if (routeModel.IsFallbackRoute)
            {
                // Fallback routes cannot supersede other fallback routes.
                return false;
            }

            if (!string.Equals(fallbackRouteModel.PageName, routeModel.PageName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (fallbackRouteModel.RouteValues.Count != routeModel.RouteValues.Count)
            {
                return false;
            }

            foreach (var kvp in fallbackRouteModel.RouteValues)
            {
                if (!routeModel.RouteValues.TryGetValue(kvp.Key, out var value) || !string.Equals(kvp.Value, value, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
