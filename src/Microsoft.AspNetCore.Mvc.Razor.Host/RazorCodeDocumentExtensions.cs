﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Razor.Evolution;

namespace Microsoft.AspNetCore.Mvc.Razor
{
    public static class RazorCodeDocumentExtensions
    {
        private static readonly object RelativePathKey = new object();

        public static string GetRelativePath(this RazorCodeDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return document.Items[RelativePathKey] as string;
        }


        public static void SetRelativePath(this RazorCodeDocument document, string relativePath)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            document.Items[RelativePathKey] = relativePath;
        }
    }
}
