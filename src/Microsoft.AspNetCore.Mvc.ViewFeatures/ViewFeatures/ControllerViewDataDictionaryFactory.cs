// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    /// <summary>
    /// A factory which provides access to an <see cref="ViewDataDictionary"/> instance associated with the current controller instance.
    /// </summary>
    public class ControllerViewDataDictionaryFactory
    {
        private static readonly object Key = typeof(ViewDataDictionary);
        private readonly IModelMetadataProvider _modelMetadataProvider;

        public ControllerViewDataDictionaryFactory(IModelMetadataProvider modelMetadataProvider)
        {
            _modelMetadataProvider = modelMetadataProvider ?? throw new ArgumentNullException(nameof(modelMetadataProvider));
        }

        public ViewDataDictionary GetViewDataDictionary(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ViewDataDictionary result;
            var items = context.HttpContext.Items;
            if (items.TryGetValue(Key, out var value))
            {
                result = (ViewDataDictionary)value;
            }
            else
            {
                result = new ViewDataDictionary(_modelMetadataProvider, context.ModelState);
                items.Add(Key, result);
            }

            return result;
        }
    }
}
