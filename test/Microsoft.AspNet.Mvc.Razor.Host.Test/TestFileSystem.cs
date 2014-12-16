// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.AspNet.FileSystems;
using Microsoft.Framework.Cache.Memory;
using Microsoft.Framework.Expiration.Interfaces;

namespace Microsoft.AspNet.Mvc.Razor
{
    public class TestFileSystem : IFileSystem
    {
        private readonly Dictionary<string, IFileInfo> _lookup =
            new Dictionary<string, IFileInfo>(StringComparer.Ordinal);

        private readonly Dictionary<string, CancellationTokenSource> _triggerLookup =
            new Dictionary<string, CancellationTokenSource>(StringComparer.Ordinal);

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            throw new NotSupportedException();
        }

        public void AddFile(string path, string contents)
        {
            var fileInfo = new TestFileInfo
            {
                Content = contents,
                PhysicalPath = path,
                Name = Path.GetFileName(path),
                LastModified = DateTime.UtcNow,
            };

            AddFile(path, fileInfo);
        }

        public void AddFile(string path, TestFileInfo contents)
        {
            _lookup[path] = contents;
        }

        public void DeleteFile(string path)
        {
            _lookup.Remove(path);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (_lookup.ContainsKey(subpath))
            {
                return _lookup[subpath];
            }
            else
            {
                return new NotFoundFileInfo(subpath);
            }
        }

        public IExpirationTrigger Watch(string filter)
        {
            var tokenSource = GetTriggerTokenSource(filter);
            return new CancellationTokenTrigger(tokenSource.Token);
        }

        public CancellationTokenSource GetTriggerTokenSource(string filter)
        {
            CancellationTokenSource tokenSource;
            if (!_triggerLookup.TryGetValue(filter, out tokenSource))
            {
                tokenSource = new CancellationTokenSource();
                tokenSource.Token.Register(() =>
                {
                    _triggerLookup.Remove(filter);
                });
                _triggerLookup[filter] = tokenSource;
            }

            return tokenSource;
        }
    }
}