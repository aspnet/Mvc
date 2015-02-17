﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Contains methods to locate <c>_ViewStart.cshtml</c> and <c>_Global.cshtml</c>
    /// </summary>
    public static class ViewHierarchyUtility
    {
        private const string ViewStartFileName = "_ViewStart.cshtml";
        private const string GlobalImportFileName = "_GlobalImport.cshtml";

        /// <summary>
        /// Determines if the given path represents a view start file.
        /// </summary>
        /// <param name="path">The path to inspect.</param>
        /// <returns>True if the path is a view start file, false otherwise.</returns>
        public static bool IsViewStart([NotNull] string path)
        {
            var fileName = Path.GetFileName(path);
            return string.Equals(ViewStartFileName, fileName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the view start locations that are applicable to the specified path.
        /// </summary>
        /// <param name="applicationRelativePath">The application relative path of the file to locate
        /// <c>_ViewStart</c>s for.</param>
        /// <returns>A sequence of paths that represent potential view start locations.</returns>
        /// <remarks>
        /// This method returns paths starting from the directory of <paramref name="applicationRelativePath"/> and
        /// moves upwards until it hits the application root.
        /// e.g.
        /// /Views/Home/View.cshtml -> [ /Views/Home/_ViewStart.cshtml, /Views/_ViewStart.cshtml, /_ViewStart.cshtml ]
        /// </remarks>
        public static IEnumerable<string> GetViewStartLocations(string applicationRelativePath)
        {
            return GetHierarchicalPath(applicationRelativePath, ViewStartFileName);
        }

        /// <summary>
        /// Gets the locations for _GlobalImports that are applicable to the specified path.
        /// </summary>
        /// <param name="applicationRelativePath">The application relative path of the file to locate
        /// <c>_Global</c>s for.</param>
        /// <returns>A sequence of paths that represent potential _Global locations.</returns>
        /// <remarks>
        /// This method returns paths starting from the directory of <paramref name="applicationRelativePath"/> and
        /// moves upwards until it hits the application root.
        /// e.g.
        /// /Views/Home/View.cshtml -> [ /Views/Home/_GlobalImport.cshtml, /Views/_GlobalImport.cshtml,
        ///                              /_GlobalImport.cshtml ]
        /// </remarks>
        public static IEnumerable<string> GetGlobalImportLocations(string applicationRelativePath)
        {
            return GetHierarchicalPath(applicationRelativePath, GlobalImportFileName);
        }

        private static IEnumerable<string> GetHierarchicalPath(string relativePath, string fileName)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return Enumerable.Empty<string>();
            }

            if (relativePath.StartsWith("~/", StringComparison.Ordinal))
            {
                relativePath = relativePath.Substring(2);
            }

            if (relativePath.StartsWith("/", StringComparison.Ordinal))
            {
                relativePath = relativePath.Substring(1);
            }

            if (Path.IsPathRooted(relativePath))
            {
                // If the path looks like it's not app relative, don't attempt to construct paths.
                return Enumerable.Empty<string>();
            }

            if (string.Equals(Path.GetFileName(relativePath), fileName, StringComparison.OrdinalIgnoreCase))
            {
                // If the specified path is for the file hierarchy being constructed, then the first file that applies
                // to it is in a parent directory.
                relativePath = Path.GetDirectoryName(relativePath);
                if (string.IsNullOrEmpty(relativePath))
                {
                    return Enumerable.Empty<string>();
                }
            }

            var locations = new List<string>();
            while (!string.IsNullOrEmpty(relativePath))
            {
                relativePath = Path.GetDirectoryName(relativePath);
                var path = Path.Combine(relativePath, fileName);
                locations.Add(path);
            }

            return locations;
        }
    }
}