// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNet.FileProviders;
using Microsoft.Framework.FileSystemGlobbing.Abstractions;

namespace Microsoft.AspNet.Mvc.TagHelpers.Internal
{
    public class FileProviderGlobbingDirectory : DirectoryInfoBase
    {
        private readonly IFileProvider _fileProvider;
        private readonly IFileInfo _fileInfo;
        private readonly FileProviderGlobbingDirectory _parent;

        public FileProviderGlobbingDirectory(
            [NotNull] IFileProvider fileProvider,
            IFileInfo fileInfo,
            FileProviderGlobbingDirectory parent)
        {
            _fileProvider = fileProvider;
            _fileInfo = fileInfo;
            _parent = parent;

            if (_parent != null && !string.IsNullOrEmpty(_parent.RelativePath) && _fileInfo != null)
            {
                // We have a parent and they have a relative path so concat that with my name
                RelativePath = _parent.RelativePath + Path.DirectorySeparatorChar + _fileInfo.Name;
            }
            else if (_fileInfo != null)
            {
                // We don't have a parent so just use my name
                RelativePath = _fileInfo.Name;
            }
            else
            {
                // We're the root of the directory tree
                RelativePath = string.Empty;
            }
        }

        public string RelativePath { get; }

        public override string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(_parent?.FullName))
                {
                    // We have no parent (we're the root) so just use our name
                    return Name;
                }
                
                return _parent.FullName + Path.DirectorySeparatorChar + Name;
            }
        }

        public override string Name
        {
            get
            {
                return _fileInfo?.Name;
            }
        }

        public override DirectoryInfoBase ParentDirectory
        {
            get
            {
                return _parent;
            }
        }

        public override IEnumerable<FileSystemInfoBase> EnumerateFileSystemInfos(
            string searchPattern,
            SearchOption searchOption)
        {
            if (!string.Equals(searchPattern, "*", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Only full wildcard searches are supported, i.e. \"*\"", "searchPattern");
            }

            foreach (var fileInfo in _fileProvider.GetDirectoryContents(RelativePath))
            {
                yield return BuildFileResult(fileInfo);
            }
        }

        private FileSystemInfoBase BuildFileResult(IFileInfo fileInfo)
        {
            if (fileInfo.IsDirectory)
            {
                return new FileProviderGlobbingDirectory(_fileProvider, fileInfo, this);
            }

            return new FileProviderGlobbingFile(this, fileInfo);
        }
    }
}