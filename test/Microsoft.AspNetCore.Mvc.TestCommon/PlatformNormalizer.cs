// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.AspNetCore.Razor;

namespace Microsoft.AspNetCore.Mvc
{
    public static class PlatformNormalizer
    {
        // Each new line character is returned as "_".
        public static string GetNewLinesAsUnderscores(int numberOfNewLines)
        {
            return new string('_', numberOfNewLines * Environment.NewLine.Length);
        }

        public static string NormalizePath(string path)
        {
            return path.Replace('\\', Path.DirectorySeparatorChar);
        }

        // Assuming windows based source location is passed in,
        // it gets normalized to other platforms.
        public static SourceLocation NormalizedSourceLocation(int absoluteIndex, int lineIndex, int characterIndex)
        {
            var windowsNewLineLength = "\r\n".Length;
            var differenceInLength = windowsNewLineLength - Environment.NewLine.Length;
            return new SourceLocation(absoluteIndex - (differenceInLength * lineIndex), lineIndex, characterIndex);
        }
    }
}