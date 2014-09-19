// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using System.IO;
using Microsoft.AspNet.FileSystems;

namespace Microsoft.AspNet.Mvc.Razor
{
    internal static class RelativePath
    {
        private static string _trailingSlash = Path.DirectorySeparatorChar.ToString();

        public static string GetRelativePath([NotNull] string basePath, [NotNull] IFileInfo file)
        {
            Contract.Assert(file.PhysicalPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase));
            basePath = EnsureTrailingSlash(basePath);

            var rootRelativePath = file.PhysicalPath.Substring(basePath.Length);

            return rootRelativePath;
        }

        public static string EnsureTrailingSlash([NotNull]string path)
        {
            if (!path.EndsWith(_trailingSlash))
            {
                path += Path.DirectorySeparatorChar;
            }

            return path;
        }
    }
}