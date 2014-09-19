// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.Razor
{
    public class CompilerCacheEntry
    {
        public CompilerCacheEntry([NotNull] RazorFileInfo info, [NotNull] Type viewType)
        {
            ViewType = viewType;
            RelativePath = info.RelativePath;
            Length = info.Length;
            CompiledTimeStamp = info.LastModified;
            Hash = info.Hash;
        }

        public CompilerCacheEntry([NotNull] RelativeFileInfo info, [NotNull] Type viewType, string hash)
        {
            ViewType = viewType;
            RelativePath = info.RelativePath;
            Length = info.FileInfo.Length;
            CompiledTimeStamp = info.FileInfo.LastModified;
            RuntimeTimeStamp = info.FileInfo.LastModified;
            Hash = hash;
        }

        public Type ViewType { get; set; }
        public string RelativePath { get; set; }
        public long Length { get; set; }
        public DateTime CompiledTimeStamp { get; set; }
        public DateTime? RuntimeTimeStamp { get; set; }
        public string Hash { get; set; }
    }
}
