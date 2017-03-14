// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc.Razor.TagHelperComponent
{
    public abstract class TagHelperComponentTagHelper : TagHelper
    {
        private readonly IList<ITagHelperComponent> _components;
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

            _components = components.OrderBy(p => p.Order).ToList();
            _logger = loggerFactory.CreateLogger<TagHelperComponentTagHelper>();
        }

        /// <inheritdoc />
        public override void Init(TagHelperContext context)
        {
            foreach (var component in _components)
            {
                if (component.AppliesTo(context))
                {
                    _logger.TagHelperComponentAppliesTo(component.GetType().ToString(), context.TagName);
                    component.Init(context);
                    _logger.ComponentInitialized(component.GetType().ToString());
                }
            }
        }

        /// <inheritdoc />
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            foreach (var component in _components)
            {
                if (component.AppliesTo(context))
                {
                    await component.ProcessAsync(context, output);
                    _logger.ComponentProcessed(component.GetType().ToString());
                }
            }
        }
    }
}
