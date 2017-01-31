// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.AspNetCore.Mvc.Razor
{
    /// <summary>
    /// Contains methods to locate <c>_ViewStart.cshtml</c> and <c>_ViewImports.cshtml</c>
    /// </summary>
    public static class ViewHierarchyUtility
    {
        private const string ViewStartFileName = "_ViewStart.cshtml";

        /// <summary>
        /// File name of <c>_ViewImports.cshtml</c> file
        /// </summary>
        public static readonly string ViewImportsFileName = "_ViewImports.cshtml";

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
            return GetHierarchicalLocations(applicationRelativePath, ViewStartFileName);
        }

        /// <summary>
        /// Gets the locations for <c>_ViewImports</c>s that are applicable to the specified path.
        /// </summary>
        /// <param name="applicationRelativePath">The application relative path of the file to locate
        /// <c>_ViewImports</c>s for.</param>
        /// <returns>A sequence of paths that represent potential <c>_ViewImports</c> locations.</returns>
        /// <remarks>
        /// This method returns paths starting from the directory of <paramref name="applicationRelativePath"/> and
        /// moves upwards until it hits the application root.
        /// e.g.
        /// /Views/Home/View.cshtml -> [ /Views/Home/_ViewImports.cshtml, /Views/_ViewImports.cshtml,
        ///                              /_ViewImports.cshtml ]
        /// </remarks>
        public static IEnumerable<string> GetViewImportsLocations(string applicationRelativePath)
        {
            return GetHierarchicalLocations(applicationRelativePath, ViewImportsFileName);
        }

        /// <summary>
        /// Gets the locations for <paramref name="fileName"/>s that are applicable to the specified path.
        /// </summary>
        /// <param name="applicationRelativePath">The application relative path of the file to locate
        /// files named <paramref name="fileName"/>.</param>
        /// <param name="fileName">The file name.</param>
        /// <returns>A sequence of paths that represent potential <c>_ViewImports</c> locations.</returns>
        /// <remarks>
        /// This method returns paths starting from the directory of <paramref name="applicationRelativePath"/> and
        /// moves upwards until it hits the application root.
        /// e.g.
        /// /Views/Home/View.cshtml -> [ /Views/Home/FileName.cshtml, /Views/FileName.cshtml,
        ///                              /FileName.cshtml ]
        /// </remarks>
        public static IEnumerable<string> GetHierarchicalLocations(string applicationRelativePath, string fileName)
        {
            if (string.IsNullOrEmpty(applicationRelativePath))
            {
                return Enumerable.Empty<string>();
            }

            if (applicationRelativePath.StartsWith("~/", StringComparison.Ordinal))
            {
                applicationRelativePath = applicationRelativePath.Substring(2);
            }

            if (applicationRelativePath.StartsWith("/", StringComparison.Ordinal))
            {
                applicationRelativePath = applicationRelativePath.Substring(1);
            }

            if (string.Equals(Path.GetFileName(applicationRelativePath), fileName, StringComparison.OrdinalIgnoreCase))
            {
                // If the specified path is for the file hierarchy being constructed, then the first file that applies
                // to it is in a parent directory.
                applicationRelativePath = Path.GetDirectoryName(applicationRelativePath);

                if (string.IsNullOrEmpty(applicationRelativePath))
                {
                    return Enumerable.Empty<string>();
                }
            }

            var builder = new StringBuilder(applicationRelativePath);
            builder.Replace('\\', '/');

            if (builder.Length > 0 && builder[0] != '/')
            {
                builder.Insert(0, '/');
            }

            var locations = new List<string>();
            for (var index = builder.Length - 1; index >= 0; index--)
            {
                if (builder[index] == '/')
                {
                    builder.Length = index + 1;
                    builder.Append(fileName);

                    locations.Add(builder.ToString());
                }
            }

            return locations;
        }
    }
}