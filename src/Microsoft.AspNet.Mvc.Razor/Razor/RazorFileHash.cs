// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.FileProviders;

namespace Microsoft.AspNet.Mvc.Razor
{
    public static class RazorFileHash
    {
        /// <summary>
        /// Version 1 of the hash algorithm used for generating hashes of Razor files.
        /// </summary>
        public static readonly int HashAlgorthmVersion1 = 1;

        public static long GetHash([NotNull] IFileInfo file, int hashAlgorithmVersion)
        {
            if (hashAlgorithmVersion != HashAlgorthmVersion1)
            {
                throw new ArgumentException(Resources.RazorHash_UnsupportedHashAlgorithm,
                                            nameof(hashAlgorithmVersion));
            }

            try
            {
                using (var stream = file.CreateReadStream())
                {
                    return Crc32.Calculate(stream);
                }
            }
            catch (Exception)
            {
                // Don't throw if reading the file fails.
                return 0;
            }
        }
    }
}