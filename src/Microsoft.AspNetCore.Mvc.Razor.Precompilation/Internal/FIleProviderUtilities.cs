// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.AspNetCore.Mvc.Razor.Precompilation.Internal
{
    public static class FileProviderUtilities
    {
        public static IEnumerable<RelativeFileInfo> GetRazorFiles(IFileProvider fileProvider)
        {
            var files = new List<RelativeFileInfo>();
            GetRazorFiles(fileProvider, files, root: string.Empty);
            return files;
        }

        private static void GetRazorFiles(IFileProvider fileProvider, List<RelativeFileInfo> razorFiles, string root)
        {
            foreach (var fileInfo in fileProvider.GetDirectoryContents(root))
            {
                var relativePath = Path.Combine(root, fileInfo.Name);
                if (fileInfo.IsDirectory)
                {
                    GetRazorFiles(fileProvider, razorFiles, relativePath);
                }
                else if (fileInfo.Name.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
                {
                    razorFiles.Add(new RelativeFileInfo(fileInfo, relativePath));
                }
            }
        }
    }
}
