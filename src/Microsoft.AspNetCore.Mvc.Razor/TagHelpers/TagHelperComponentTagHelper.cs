// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc.Razor.TagHelpers
{
    public abstract class TagHelperComponentTagHelper : TagHelper
    {
        private readonly ILogger _logger;

        private IEnumerable<ITagHelperComponent> _components;

        /// <summary>
        /// Creates a new <see cref="TagHelperComponentTagHelper"/>.
        /// </summary>
        /// <param name="manager">The <see cref="ITagHelperComponentManager"/> which contains the list
        /// of <see cref="ITagHelperComponent"/>s.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public TagHelperComponentTagHelper(ITagHelperComponentManager manager,
            ILoggerFactory loggerFactory)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _components = new List<ITagHelperComponent>(manager.Components.OrderBy(p => p.Order));
            _logger = loggerFactory.CreateLogger(GetType());
        }

        /// <inheritdoc />
        public override void Init(TagHelperContext context)
        {
            foreach (var component in _components)
            {
                component.Init(context);
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.TagHelperComponentInitialized(component.GetType().FullName);
                }
            }
        }

        /// <inheritdoc />
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            foreach (var component in _components)
            {
                await component.ProcessAsync(context, output);
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.TagHelperComponentProcessed(component.GetType().FullName);
                }
            }
        }
    }
}
