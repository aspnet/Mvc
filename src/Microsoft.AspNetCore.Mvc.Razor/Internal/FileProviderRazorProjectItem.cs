﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    public class FileProviderProjectItem : RazorProjectItem
    {
        public FileProviderProjectItem(IFileInfo fileInfo, string basePath, string path)
        {
            FileInfo = fileInfo;
            BasePath = basePath;
            Path = path;
        }

        public IFileInfo FileInfo { get; }

        public override string BasePath { get; }

        public override string Path { get; }

        public override bool Exists => FileInfo.Exists;

        public override string PhysicalPath => FileInfo.PhysicalPath;

        public override Stream Read()
        {
            return FileInfo.CreateReadStream();
        }
    }
}
