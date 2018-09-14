// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Internal;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    /// <summary>
    /// An <see cref="IValueProvider"/> adapter for data stored in an <see cref="IFormFileCollection"/>.
    /// Does not actually read the form collection, but provides prefix matches.
    /// </summary>
    internal class FormFileValueProvider : IEnumerableValueProvider
    {
        private readonly IFormFileCollection _files;
        private PrefixContainer _prefixContainer;

        /// <summary>
        /// Creates a value provider for <see cref="IFormFileCollection"/>.
        /// </summary>
        /// <param name="files">The key value pairs to wrap.</param>
        public FormFileValueProvider(IFormFileCollection files)
        {
            _files = files ?? throw new ArgumentNullException(nameof(files));
        }

        internal PrefixContainer PrefixContainer
        {
            get
            {
                if (_prefixContainer == null)
                {
                    var fileNames = new List<string>();
                    for (var i = 0; i < _files.Count; i++)
                    {
                        var file = _files[i];
                        if (file.Length == 0 || string.IsNullOrEmpty(file.Name))
                        {
                            continue;
                        }

                        fileNames.Add(file.Name);
                    }
                    
                    _prefixContainer = new PrefixContainer(fileNames);
                }

                return _prefixContainer;
            }
        }

        /// <inheritdoc />
        public bool ContainsPrefix(string prefix)
        {
            return PrefixContainer.ContainsPrefix(prefix);
        }

        /// <inheritdoc />
        public virtual IDictionary<string, string> GetKeysFromPrefix(string prefix)
        {
            if (prefix == null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            return PrefixContainer.GetKeysFromPrefix(prefix);
        }

        /// <inheritdoc />
        public ValueProviderResult GetValue(string key)
        {
            return ValueProviderResult.None;
        }
    }
}
