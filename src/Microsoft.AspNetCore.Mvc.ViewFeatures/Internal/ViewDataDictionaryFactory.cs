// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    public class ViewDataDictionaryFactory
    {
        public static ViewDataDictionary CreateViewDataDictionary(IModelMetadataProvider metadataProvider)
        {
            var viewDataDictionary = new ViewDataDictionary(metadataProvider);
            return viewDataDictionary;
        }
    }
}
