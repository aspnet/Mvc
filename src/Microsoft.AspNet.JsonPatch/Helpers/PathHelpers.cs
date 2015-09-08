﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.JsonPatch.Exceptions;

namespace Microsoft.AspNet.JsonPatch.Helpers
{
    internal static class PathHelpers
    {
        internal static string NormalizePath(string path)
        {
            // check for most common path errors on create.  This is not
            // absolutely necessary, but it allows us to already catch mistakes
            // on creation of the patch document rather than on execute.

            if (path.Contains(".") || path.Contains("//") || path.Contains(" ") || path.Contains("\\"))
            {
                throw new JsonPatchException(Resources.FormatInvalidValueForPath(path), null); 
            }

            if (!(path.StartsWith("/")))
            {
                return "/" + path;
            }
            else
            {
                return path;
            }
        }
    }
}
