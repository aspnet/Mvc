// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.ModelBinding.Metadata
{
    public class DefaultMetadataDetailsCache
    {
        public DefaultMetadataDetailsCache(ModelMetadataIdentity key, IReadOnlyList<object> attributes)
        {
            Key = key;
            Attributes = attributes;
        }

        public IReadOnlyList<object> Attributes { get; }

        public BindingMetadata BindingMetadata { get; set; }

        public DisplayMetadata DisplayMetadata { get; set; }

        public ModelMetadataIdentity Key { get; }

        public Func<object, object> PropertyAccessor { get; set; }

        public Action<object, object> PropertySetter { get; set; }

        public ValidationMetadata ValidationMetadata { get; set; }
    }
}