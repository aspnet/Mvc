﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Razor.Evolution;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    public class DefaultRazorProject : RazorProject
    {
        private const string RazorFileExtension = ".cshtml";
        private readonly IFileProvider _provider;

        public DefaultRazorProject(IFileProvider provider)
        {
            _provider = provider;
        }

        public override IEnumerable<RazorProjectItem> EnumerateItems(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (path.Length == 0 || path[0] != '/')
            {
                throw new ArgumentException(Resources.RazorProject_PathMustStartWithForwardSlash);
            }

            return EnumerateFiles(_provider.GetDirectoryContents(path), path, "");
        }

        private IEnumerable<RazorProjectItem> EnumerateFiles(IDirectoryContents directory, string basePath, string prefix)
        {
            if (directory.Exists)
            {
                foreach (var file in directory)
                {
                    if (file.IsDirectory)
                    {
                        var children = EnumerateFiles(_provider.GetDirectoryContents(file.PhysicalPath), basePath, prefix + "/" + file.Name);
                        foreach (var child in children)
                        {
                            yield return child;
                        }
                    }
                    else if (string.Equals(RazorFileExtension, Path.GetExtension(file.Name), StringComparison.OrdinalIgnoreCase))
                    {
                        yield return new DefaultRazorProjectItem(file, basePath, prefix + "/" + file.Name);
                    }
                }
            }
        }
    }
}
