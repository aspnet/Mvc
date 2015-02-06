// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.Framework.FileSystemGlobbing.Abstractions;

namespace Microsoft.AspNet.Hosting
{
    /// <summary>
    /// Extension methods for <see cref="IHostingEnvironment"/>.
    /// </summary>
    public static class IHostingEnvironmentExtensions
    {
        /// <summary>
        /// Gets the <see cref="IHostingEnvironment.WebRoot"/> as a <see cref="DirectoryInfoBase"/>.
        /// </summary>
        /// <param name="hostingEnvironment">The <see cref="IHostingEnvironment" />.</param>
        /// <returns>The <see cref="IHostingEnvironment.WebRoot"/> as a <see cref="DirectoryInfoBase"/></returns>
        public static DirectoryInfoBase WebRootDirectoryInfo(this IHostingEnvironment hostingEnvironment)
        {
            return new DirectoryInfoWrapper(new DirectoryInfo(hostingEnvironment.WebRoot));
        }
    }
}