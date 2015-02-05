﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using Microsoft.AspNet.FileProviders;
using Microsoft.Framework.Expiration.Interfaces;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Default implementation for the <see cref="IRazorFileProviderCache"/> interface that caches
    /// the results of <see cref="RazorViewEngineOptions.FileProvider"/>.
    /// </summary>
    public class DefaultRazorFileProviderCache : IRazorFileProviderCache
    {
        private readonly ConcurrentDictionary<string, ExpiringFileInfo> _fileInfoCache =
            new ConcurrentDictionary<string, ExpiringFileInfo>(StringComparer.Ordinal);

        private readonly IFileProvider _fileProvider;
        private readonly TimeSpan _offset;

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultRazorFileProviderCache"/>.
        /// </summary>
        /// <param name="optionsAccessor">Accessor to <see cref="RazorViewEngineOptions"/>.</param>
        public DefaultRazorFileProviderCache(IOptions<RazorViewEngineOptions> optionsAccessor)
        {
            _fileProvider = optionsAccessor.Options.FileProvider;
            _offset = optionsAccessor.Options.ExpirationBeforeCheckingFilesOnDisk;
        }

        protected virtual DateTime UtcNow
        {
            get
            {
                return DateTime.UtcNow;
            }
        }

        /// <inheritdoc />
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return _fileProvider.GetDirectoryContents(subpath);
        }

        /// <inheritdoc />
        public IFileInfo GetFileInfo(string subpath)
        {
            ExpiringFileInfo expiringFileInfo;
            var utcNow = UtcNow;

            if (_fileInfoCache.TryGetValue(subpath, out expiringFileInfo) &&
                expiringFileInfo.ValidUntil > utcNow)
            {
                return expiringFileInfo.FileInfo;
            }
            else
            {
                var fileInfo = _fileProvider.GetFileInfo(subpath);

                expiringFileInfo = new ExpiringFileInfo()
                {
                    FileInfo = fileInfo,
                    ValidUntil = _offset == TimeSpan.MaxValue ? DateTime.MaxValue : utcNow.Add(_offset),
                };

                _fileInfoCache[subpath] = expiringFileInfo;

                return fileInfo;
            }
        }

        /// <inheritdoc />
        public IExpirationTrigger Watch(string filter)
        {
            return _fileProvider.Watch(filter);
        }

        private class ExpiringFileInfo
        {
            public IFileInfo FileInfo { get; set; }
            public DateTime ValidUntil { get; set; }
        }
    }
}