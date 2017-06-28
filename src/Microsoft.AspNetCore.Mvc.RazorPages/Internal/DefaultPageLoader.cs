﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    public class DefaultPageLoader : IPageLoader
    {
        private readonly IPageApplicationModelProvider[] _applicationModelProviders;
        private readonly IViewCompilerProvider _viewCompilerProvider;
        private readonly RazorPagesOptions _options;

        public DefaultPageLoader(
            IEnumerable<IPageApplicationModelProvider> applicationModelProviders,
            IViewCompilerProvider viewCompilerProvider,
            IOptions<RazorPagesOptions> pageOptions)
        {
            _applicationModelProviders = applicationModelProviders.ToArray();
            _viewCompilerProvider = viewCompilerProvider;
            _options = pageOptions.Value;
        }

        private IViewCompiler Compiler => _viewCompilerProvider.GetCompiler();

        public CompiledPageActionDescriptor Load(PageActionDescriptor actionDescriptor)
        {
            if (actionDescriptor == null)
            {
                throw new ArgumentNullException(nameof(actionDescriptor));
            }

            var compileTask = Compiler.CompileAsync(actionDescriptor.RelativePath);
            var viewDescriptor = compileTask.GetAwaiter().GetResult();
            var pageAttribute = (RazorPageAttribute)viewDescriptor.ViewAttribute;

            var context = new PageApplicationModelProviderContext(actionDescriptor, pageAttribute.ViewType.GetTypeInfo());
            for (var i = 0; i < _applicationModelProviders.Length; i++)
            {
                _applicationModelProviders[i].OnProvidersExecuting(context);
            }

            for (var i = _applicationModelProviders.Length - 1; i >= 0; i--)
            {
                _applicationModelProviders[i].OnProvidersExecuted(context);
            }

            for (var i = 0; i < _options.ApplicationModelConventions.Count; i++)
            {
                _options.ApplicationModelConventions[i].Apply(context.PageApplicationModel);
            }

            return CompiledPageActionDescriptorBuilder.Build(context.PageApplicationModel);
        }
    }
}