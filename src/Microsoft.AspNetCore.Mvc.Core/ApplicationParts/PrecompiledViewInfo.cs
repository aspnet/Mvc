// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace Microsoft.AspNetCore.Mvc.Core.ApplicationParts
{
    public class PrecompiledViewInfo
    {
        public PrecompiledViewInfo(string path, Func<Stream> assemblyStreamFactory)
            : this(path, assemblyStreamFactory, pdbStreamFactory: null)
        {
        }

        public PrecompiledViewInfo(
            string path,
            Func<Stream> assemblyStreamFactory,
            Func<Stream> pdbStreamFactory)
        {
            Path = path;
            AssemblyStreamFactory = assemblyStreamFactory;
            PdbStreamFactory = pdbStreamFactory;
        }

        public string Path { get; }

        public Func<Stream> AssemblyStreamFactory { get; }

        public Func<Stream> PdbStreamFactory { get; }
    }
}
