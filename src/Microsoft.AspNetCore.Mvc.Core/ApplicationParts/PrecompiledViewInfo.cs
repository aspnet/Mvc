// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace Microsoft.AspNetCore.Mvc.Core.ApplicationParts
{
    /// <summary>
    /// Provides information for precompiled views.
    /// </summary>
    public class PrecompiledViewInfo
    {
        /// <summary>
        /// Creates a new instance of <see cref="PrecompiledViewInfo" />.
        /// </summary>
        /// <param name="path">The path of the view.</param>
        /// <param name="assemblyStreamFactory">Factory that provides the <see cref="Stream"/> for the view assembly.</param>
        /// <param name="pdbStreamFactory">Factory that provides the <see cref="Stream"/> for the view pdb.</param>
        public PrecompiledViewInfo(
            string path,
            Func<Stream> assemblyStreamFactory,
            Func<Stream> pdbStreamFactory)
        {
            Path = path;
            AssemblyStreamFactory = assemblyStreamFactory;
            PdbStreamFactory = pdbStreamFactory;
        }

        /// <summary>
        /// The path of the view.
        /// </summary>
        public string Path { get; }


        public Func<Stream> AssemblyStreamFactory { get; }

        public Func<Stream> PdbStreamFactory { get; }
    }
}
