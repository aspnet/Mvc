// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.PageExecutionInstrumentation;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Represents a <see cref="IRazorPageFactory"/> that creates <see cref="RazorPage"/> instances
    /// from razor files in the file system.
    /// </summary>
    public class VirtualPathRazorPageFactory : IRazorPageFactory
    {
        private readonly IRazorCompilationService _compilationService;
        private readonly ITypeActivator _activator;
        private readonly IServiceProvider _serviceProvider;
        private readonly IFileInfoCache _fileInfoCache;
        private readonly bool _isInstrumentationEnabled;

        public VirtualPathRazorPageFactory(IRazorCompilationService compilationService,
                                           ITypeActivator typeActivator,
                                           IServiceProvider serviceProvider,
                                           IFileInfoCache fileInfoCache,
                                           IContextAccessor<HttpContext> contextAccessor)
        {
            _compilationService = compilationService;
            _activator = typeActivator;
            _serviceProvider = serviceProvider;
            _fileInfoCache = fileInfoCache;
            _isInstrumentationEnabled = IsInstrumentationEnabled(contextAccessor.Value);
        }

        /// <inheritdoc />
        public IRazorPage CreateInstance([NotNull] string relativePath)
        {
            var fileInfo = _fileInfoCache.GetFileInfo(relativePath);

            if (fileInfo != null)
            {
                var relativeFileInfo = new RelativeFileInfo()
                {
                    FileInfo = fileInfo,
                    RelativePath = relativePath,
                };

                var result = _compilationService.Compile(relativeFileInfo, _isInstrumentationEnabled);
                var page = (IRazorPage)_activator.CreateInstance(_serviceProvider, result.CompiledType);
                page.Path = relativePath;

                return page;
            }

            return null;
        }

        private static bool IsInstrumentationEnabled(HttpContext context)
        {
            return context.GetFeature<IPageExecutionListenerFeature>() != null;
        }
    }
}
