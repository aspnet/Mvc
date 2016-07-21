// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace Microsoft.AspNetCore.Mvc.Razor.Precompilation.Internal
{
    public class CompileOutputs : IDisposable
    {
        public CompileOutputs(string relativePath, bool generatePdbs)
        {
            RelativePath = relativePath;
            PdbStream = generatePdbs ? new MemoryStream() : null;
        }

        public MemoryStream AssemblyStream { get; } = new MemoryStream();

        public MemoryStream PdbStream { get; }

        public string RelativePath { get; }

        public void Dispose()
        {
            AssemblyStream.Dispose();
            PdbStream.Dispose();
        }
    }
}
