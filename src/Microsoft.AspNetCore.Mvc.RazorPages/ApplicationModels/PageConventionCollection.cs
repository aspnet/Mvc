// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    public class PageConventionCollection : Collection<IPageConvention>
    {
        public IPageApplicationModelConvention AddConvention(string pageName, Action<PageApplicationModel> action)
        {
            if (string.IsNullOrEmpty(pageName))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(pageName));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return Add(new PageApplicationModelConvention(pageName, action));
        }

        public IPageApplicationModelConvention AddFolderConvention(string folderPath, Action<PageApplicationModel> action)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(folderPath));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return Add(new FolderApplicationModelConvention(folderPath, action));
        }

        public IPageRouteModelConvention AddConvention(string pageName, Action<PageRouteModel> action)
        {
            if (string.IsNullOrEmpty(pageName))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(pageName));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return Add(new PageRouteModelConvention(pageName, action));
        }

        public IPageRouteModelConvention AddFolderConvention(string folderPath, Action<PageRouteModel> action)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(folderPath));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return Add(new FolderRouteModelConvention(folderPath, action));
        }

        private TConvention Add<TConvention>(TConvention convention) where TConvention: IPageConvention
        {
            base.Add(convention);
            return convention;
        }

        private class PageRouteModelConvention : IPageRouteModelConvention
        {
            private readonly string _path;
            private readonly Action<PageRouteModel> _action;

            public PageRouteModelConvention(string path, Action<PageRouteModel> action)
            {
                _path = path;
                _action = action;
            }

            public void Apply(PageRouteModel model)
            {
                if (string.Equals(model.ViewEnginePath, _path, StringComparison.OrdinalIgnoreCase))
                {
                    _action(model);
                }
            }
        }

        private class FolderRouteModelConvention : IPageRouteModelConvention
        {
            private readonly string _folderPath;
            private readonly Action<PageRouteModel> _action;

            public FolderRouteModelConvention(string folderPath, Action<PageRouteModel> action)
            {
                _folderPath = folderPath.TrimEnd('/');
                _action = action;
            }

            public void Apply(PageRouteModel model)
            {
                var viewEnginePath = model.ViewEnginePath;

                if (PathBelongsToFolder(_folderPath, viewEnginePath))
                {
                    _action(model);
                }
            }
        }

        private class PageApplicationModelConvention : IPageApplicationModelConvention
        {
            private readonly string _path;
            private readonly Action<PageApplicationModel> _action;

            public PageApplicationModelConvention(string path, Action<PageApplicationModel> action)
            {
                _path = path;
                _action = action;
            }

            public void Apply(PageApplicationModel model)
            {
                if (string.Equals(model.ViewEnginePath, _path, StringComparison.OrdinalIgnoreCase))
                {
                    _action(model);
                }
            }
        }

        private class FolderApplicationModelConvention : IPageApplicationModelConvention
        {
            private readonly string _folderPath;
            private readonly Action<PageApplicationModel> _action;

            public FolderApplicationModelConvention(string folderPath, Action<PageApplicationModel> action)
            {
                _folderPath = folderPath.TrimEnd('/');
                _action = action;
            }

            public void Apply(PageApplicationModel model)
            {
                var viewEnginePath = model.ViewEnginePath;

                if (PathBelongsToFolder(_folderPath, viewEnginePath))
                {
                    _action(model);
                }
            }
        }

        private static bool PathBelongsToFolder(string folderPath, string viewEnginePath)
        {
            if (folderPath == "/")
            {
                // Root directory covers everything.
                return true;
            }

            return viewEnginePath.Length > folderPath.Length &&
                viewEnginePath.StartsWith(folderPath, StringComparison.OrdinalIgnoreCase) &&
                viewEnginePath[folderPath.Length] == '/';
        }
    }
}
