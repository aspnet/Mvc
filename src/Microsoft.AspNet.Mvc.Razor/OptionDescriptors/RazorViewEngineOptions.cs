﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.FileSystems;
using Microsoft.AspNet.Mvc.Razor.OptionDescriptors;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Provides programmatic configuration for the default <see cref="Microsoft.AspNet.Mvc.Rendering.IViewEngine"/>.
    /// </summary>
    public class RazorViewEngineOptions
    {
        private IFileSystem _fileSystem;

        /// <summary>
        /// Get a <see cref="IList{T}"/> of descriptors for <see cref="IViewLocationExpander" />s used by this
        /// application.
        /// </summary>
        public IList<ViewLocationExpanderDescriptor> ViewLocationExpanders { get; }
            = new List<ViewLocationExpanderDescriptor>();

        /// <summary>
        /// Gets or sets the <see cref="IFileSystem" /> used by <see cref="RazorViewEngine"/> to locate Razor files on
        /// disk.
        /// </summary>
        /// <remarks>
        /// At startup, this is initialized to an instance of <see cref="PhysicalFileSystem"/> that is rooted at the
        /// application root.
        /// </remarks>
        public IFileSystem FileSystem
        {
            get { return _fileSystem; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _fileSystem = value;
            }
        }
    }
}
