// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <inheritdoc />
    public class ViewStartProvider : IViewStartProvider
    {
        private const string ViewStartFileName = "_ViewStart.cshtml";
        private readonly string _appRoot;
        
        public ViewStartProvider(string appRoot)
        {
            _appRoot = appRoot;
        }

        /// <inheritdoc />
        public bool IsViewStart([NotNull] string viewFile)
        {
            var fileName = Path.GetFileName(viewFile);
            return string.Equals(fileName, ViewStartFileName, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetViewStartLocations([NotNull] string viewFile)
        {
            var viewStartPaths = new List<string>();
            var viewPath = Path.GetFullPath(Path.Combine(_appRoot, viewFile.Trim('/', '~')));
            var currentDir = viewPath;
            do
            {
                currentDir = Path.GetDirectoryName(currentDir);
                var viewStartPath = Path.Combine(currentDir, ViewStartFileName);

                if (File.Exists(viewStartPath))
                {
                    viewStartPaths.Add(viewStartPath);
                }
            } while (IsSubDirectory(currentDir));

            // Reverse it so the outermost ViewStart (the one closest to the app root) appears first in the list.
            viewStartPaths.Reverse();

            return viewStartPaths;
        }

        private bool IsSubDirectory(string currentDir)
        {
            return _appRoot.Length <= currentDir.Length &&
                   currentDir.StartsWith(_appRoot, StringComparison.OrdinalIgnoreCase);
        }
    }
}