// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.AspNet.FileProviders;
using Microsoft.Framework.FileSystemGlobbing.Abstractions;

namespace Microsoft.AspNet.Mvc.TagHelpers.Internal
{
    public class FileProviderGlobbingFile : FileInfoBase
    {
        private readonly IFileInfo _fileInfo;

        public FileProviderGlobbingFile([NotNull] IFileInfo fileInfo, [NotNull] DirectoryInfoBase parent)
        {
            _fileInfo = fileInfo;
            ParentDirectory = parent;
            FullName = ParentDirectory.FullName + Path.DirectorySeparatorChar + Name;
        }

        public override string FullName { get; }

        public override string Name
        {
            get
            {
                return _fileInfo.Name;
            }
        }

        public override DirectoryInfoBase ParentDirectory { get; }
    }
}