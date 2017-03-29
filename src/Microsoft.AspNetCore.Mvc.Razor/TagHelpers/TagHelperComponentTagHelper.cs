﻿// Copyright (c) .NET Foundation. All rights reserved.
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

        /// <summary>
        /// Creates a new <see cref="TagHelperComponentTagHelper"/>.
        /// </summary>
        /// <param name="components">The list of <see cref="ITagHelperComponent"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public TagHelperComponentTagHelper(IEnumerable<ITagHelperComponent> components,
            ILoggerFactory loggerFactory)
        {
            if (components == null)
            {
                throw new ArgumentNullException(nameof(components));
            }

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));

            }

            Components = components.OrderBy(p => p.Order).ToList();
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public IList<ITagHelperComponent> Components { get; set; }

        /// <inheritdoc />
        public override void Init(TagHelperContext context)
        {
            for (var i = 0; i < Components.Count; i++)
            {
                Components[i].Init(context);
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.TagHelperComponentInitialized(Components[i].ToString());
                }
            }

            Components = Components.OrderBy(p => p.Order).ToList();
        }

        /// <inheritdoc />
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            for (var i = 0; i < Components.Count; i++)
            {
                await Components[i].ProcessAsync(context, output);
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.TagHelperComponentProcessed(Components[i].GetType().ToString());
                }
            }
        }
    }
}
