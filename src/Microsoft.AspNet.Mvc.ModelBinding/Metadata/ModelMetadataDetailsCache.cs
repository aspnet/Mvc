// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class ModelMetadataDetailsCache
    {
        public ModelMetadataDetailsCache(ModelMetadataIdentity key, IReadOnlyList<object> attributes)
        {
            Key = key;
            Attributes = attributes;
        }

        public IReadOnlyList<object> Attributes { get; }

        public ModelMetadataBindingDetails BindingDetails { get; set; }

        public ModelMetadataDisplayDetails DisplayDetails { get; set; }

        public ModelMetadataIdentity Key { get; }

        public Func<object, object> PropertyAccessor { get; set; }

        public Action<object, object> PropertySetter { get; set; }

        public ModelMetadataValidationDetails ValidationDetails { get; set; }
    }
}