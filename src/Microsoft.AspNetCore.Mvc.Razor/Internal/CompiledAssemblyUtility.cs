// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    public static class CompiledAssemblyUtility
    {
        public static Type GetExportedType(Stream assemblyStream, Stream pdbStream)
        {
            var assembly =
#if NET451
                Assembly.Load(GetBytes(assemblyStream), GetBytes(pdbStream));
#else
                System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(assemblyStream, pdbStream);
#endif
            return assembly.GetExportedTypes().FirstOrDefault(a => !a.IsNested);
        }

        private static byte[] GetBytes(Stream stream)
        {
            if (stream == null)
            {
                return null;
            }

            var memoryStream = stream as MemoryStream;
            if (memoryStream == null)
            {
                memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
            }

            return memoryStream.ToArray();
        }
    }
}
