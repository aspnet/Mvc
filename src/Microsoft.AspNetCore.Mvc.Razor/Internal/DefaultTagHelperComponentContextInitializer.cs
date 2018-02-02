// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    /// <summary>
    /// Default implementation of <see cref="ITagHelperComponentContextInitializer"/>.
    /// </summary>
    public class DefaultTagHelperComponentContextInitializer : ITagHelperComponentContextInitializer
    {
        /// <inheritdoc />
        public ITagHelperComponent InitializeViewContext(ITagHelperComponent tagHelperComponent, ViewContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var propertiesToActivate = PropertyActivator<ViewContext>.GetPropertiesToActivate(
                tagHelperComponent.GetType(),
                typeof(ViewContextAttribute),
                CreateActivateInfo);

            for (var i = 0; i < propertiesToActivate.Length; i++)
            {
                var activateInfo = propertiesToActivate[i];
                activateInfo.Activate(tagHelperComponent, context);
            }

            return tagHelperComponent;
        }

        private static PropertyActivator<ViewContext> CreateActivateInfo(PropertyInfo property)
        {
            return new PropertyActivator<ViewContext>(property, viewContext => viewContext);
        }
    }
}
