// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Microsoft.AspNetCore.Mvc.Razor
{
    internal static class RazorFileHierarchy
    {
        private const string ViewStartFileName = "_ViewStart.cshtml";

        public static IEnumerable<string> FindViewImports(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(path));
            }

            if (path[0] != '/')
            {
                throw new ArgumentException(Resources.RazorProject_PathMustStartWithForwardSlash, nameof(path));
            }

            var basePath = "/";
            Debug.Assert(!string.IsNullOrEmpty(path));
            if (path.Length == 1)
            {
                yield break;
            }

            if (!path.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            {
                yield break;
            }

            StringBuilder builder;
            var fileNameIndex = path.LastIndexOf('/');
            var length = path.Length;
            Debug.Assert(fileNameIndex != -1);
            if (string.Compare(path, fileNameIndex + 1, ViewStartFileName, 0, ViewStartFileName.Length) == 0)
            {
                // If the specified path is for the file hierarchy being constructed, then the first file that applies
                // to it is in a parent directory.
                builder = new StringBuilder(path, 0, fileNameIndex, fileNameIndex + ViewStartFileName.Length);
                length = fileNameIndex;
            }
            else
            {
                builder = new StringBuilder(path);
            }

            var maxDepth = 255;
            var index = length;
            while (maxDepth-- > 0 && index > basePath.Length && (index = path.LastIndexOf('/', index - 1)) != -1)
            {
                builder.Length = index + 1;
                builder.Append(ViewStartFileName);

                var itemPath = builder.ToString();
                yield return itemPath;
            }
        }
    }
}
