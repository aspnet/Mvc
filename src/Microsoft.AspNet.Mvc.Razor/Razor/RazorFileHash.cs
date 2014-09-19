// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Cryptography;
using Microsoft.AspNet.FileSystems;

namespace Microsoft.AspNet.Mvc.Razor
{
    public static class RazorFileHash
    {
        public static string GetHash([NotNull] IFileInfo file)
        {
            try
            {
                using (var stream = file.CreateReadStream())
                using (var md5 = MD5.Create())
                {
                    return BitConverter.ToString(md5.ComputeHash(stream));
                }
            }
            catch (Exception)
            {
                // Don't throw if reading the file fails.
                return string.Empty;
            }
        }
    }
}