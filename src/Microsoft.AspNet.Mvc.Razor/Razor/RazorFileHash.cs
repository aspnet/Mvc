// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.AspNet.FileProviders;

namespace Microsoft.AspNet.Mvc.Razor
{
    public static class RazorFileHash
    {
        public static long GetHash([NotNull] IFileInfo file)
        {
            try
            {
                using (var stream = file.CreateReadStream())
                {
                    return GetHash(stream);
                }
            }
            catch (Exception)
            {
                // Don't throw if reading the file fails.
                return 0;
            }
        }

        internal static long GetHash(Stream stream)
        {
            return Crc32.Calculate(stream);
        }
    }
}